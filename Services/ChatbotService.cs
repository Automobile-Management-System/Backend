using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Ai;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Data.Common;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace automobile_backend.Services;

public class ChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _dbContext;
    private readonly string _apiKey;
    private readonly string _geminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    // --- KNOWLEDGE BASE (Unchanged) ---
    private const string KnowledgeBase = """
        You are a helpful assistant for "AutoServe 360".
        
        ## System Overview
        Our project, the Automobile Service Time Logging & Appointment System (AutoServe 360), is
        developed to overcome issues like manual booking and payment tracking. The system brings customers, employees, and
        administrators into one digital platform where everything can be managed efficiently.
        
        ## User Roles & Features
        ### Customer
        - **Dashboard:** View personal data and added vehicles.
        - **Service:** Book new appointments for services.
        - **Modification:** Request modifications for their vehicles.
        - **Payment:** Make payments for services.
        ### Employee
        - **Dashboard:** View personalized employee data.
        - **Work Progress:** Start and stop time logs for tasks.
        - **View Services/Modifications:** Can view all available services and modifications.
        ### Admin
        - **Dashboard:** See all revenue, user counts, and system metrics.
        - **User Management:** Add employees, manage user roles, edit user data, etc.
        - **Service Management:** Create new services.
        - **Payment Management:** Manually mark cash payments as complete.
        - **Manage Appointments:** View and manage all service and modification appointments.
        - **Analytics:** Generate reports and view system analytics.
        """;

    // --- DATABASE SCHEMA (Unchanged) ---
    private const string DatabaseSchema = """
        You are a professional SQL generation AI for a car service management system.
        Your SOLE task is to convert natural language questions into a single, valid, optimized SQL Server SELECT query.
        You MUST ONLY return the SQL query, without any markdown (```), explanation, or commentary.
        DO NOT generate any INSERT, UPDATE, or DELETE queries. The connection is READ-ONLY.

        --- DATABASE SCHEMA ---
        ## TABLES
        - Users (UserId, FirstName, LastName, Role)
        - CustomerVehicles (VehicleId, RegistrationNumber, FuelType, ChassisNumber, Brand, Model, Year, UserId)
        - Appointments (AppointmentId, DateTime, Status, Type, Amount, StartDateTime, EndDateTime, UserId, VehicleId)
        - Services (ServiceId, ServiceName, Description, BasePrice)
        - ModificationRequests (ModificationId, Title, Description, AppointmentId)
        - Payments (PaymentId, Amount, Status, PaymentMethod, PaymentDateTime, InvoiceLink, AppointmentId)
        - Reviews (ReviewId, Rating, ReviewText, DateTime, AppointmentId)
        - TimeLogs (LogId, StartDateTime, EndDateTime, HoursLogged, AppointmentId, UserId)
        - Reports (ReportId, StartDate, EndDate, GeneratedDate, ReportLink, UserId)
        - AppointmentServices (AppointmentId, ServiceId) -- [Many-to-Many Join Table]
        - EmployeeAppointments (AppointmentId, UserId) -- [Many-to-Many Join Table]

        ## ENUMERATIONS (Use these numerical values)
        - Users.Role: { 0 = Admin, 1 = Employee, 2 = Customer }
        - CustomerVehicles.FuelType: { 0 = Petrol, 1 = Diesel, 2 = Electric, 3 = Hybrid }
        - Appointments.Status (AppointmentStatus): { 0 = Pending, 1 = Upcoming, 2 = InProgress, 3 = Completed, 4 = Rejected }
        - Appointments.Type: { 0 = Modifications, 1 = Service }
        - Payments.Status (PaymentStatus): { 0 = Pending, 1 = Completed, 2 = Failed, 3 = Refunded }
        - Payments.PaymentMethod: { 0 = CreditCard, 1 = DebitCard, 2 = Cash, 3 = BankTransfer }
        
        ## TABLE RELATIONSHIPS (How to JOIN)
        - Users.UserId (Role=2) -> Appointments.UserId
        - Users.UserId (Role=2) -> CustomerVehicles.UserId
        - Users.UserId (Role=1) -> TimeLogs.UserId
        - Users.UserId (Role=0) -> Reports.UserId
        - Appointments.AppointmentId -> Payments.AppointmentId (One-to-One)
        - Appointments.AppointmentId -> Reviews.AppointmentId (One-to-One)
        - Appointments.AppointmentId -> ModificationRequests.AppointmentId (One-to-Many)
        - JOIN Appointments <-> Services via AppointmentServices
        - JOIN Appointments <-> Users (Employee) via EmployeeAppointments

        --- CRITICAL SQL RULES ---
        1. **Strictly use the table and column names listed above.**
        2. **ALWAYS USE ISNULL(AGGREGATE_FUNCTION, 0)** for all SUM, AVG, and COUNT.
        3. **Role-Based Joins**: Filter `Users.Role = 1` for employees, `Users.Role = 2` for customers, `Users.Role = 0` for admins.
        4. **String Matching**: Use `LIKE '%value%'` for partial string matching.
        """;

    public ChatbotService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext dbContext)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _dbContext = dbContext;
        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("GemGini:ApiKey not found in configuration.");
    }

    // --- Main Router Method (Unchanged) ---
    public async Task<string> AnswerQuestionAsync(string userQuestion, int userId, string userRole)
    {
        string category = await ClassifyQuestionAsync(userQuestion);

        switch (category)
        {
            case "DATABASE":
                return await AnswerDatabaseQuestionAsync(userQuestion, userId, userRole);
            case "GENERAL":
            default:
                return await AnswerGeneralQuestionAsync(userQuestion);
        }
    }

    // --- General Knowledge Pipeline (Unchanged) ---
    private async Task<string> AnswerGeneralQuestionAsync(string userQuestion)
    {
        string prompt = $"""
            {KnowledgeBase}
            ---
            Based ONLY on the knowledge base provided above, answer the user's question.
            Be friendly, concise, and helpful. If the question is a simple greeting like "Hi" or "Hello", respond with a friendly greeting and a brief welcome to AutoServe 360.
            User Question: "{userQuestion}"
            Answer:
            """;

        var request = new GeminiRequest { Contents = new List<Content> { new() { Parts = new List<Part> { new() { Text = prompt } } } } };
        string finalAnswer = await SendRequestToGemini(request, "You are a friendly, helpful customer service assistant for AutoServe 360.");
        return finalAnswer.Trim().Replace("\"", "");
    }

    // --- Database Pipeline (Unchanged) ---
    private async Task<string> AnswerDatabaseQuestionAsync(string userQuestion, int userId, string userRole)
    {
        string sqlQuery = await GetSqlQueryFromGemini(userQuestion, userId, userRole);
        if (string.IsNullOrWhiteSpace(sqlQuery) || sqlQuery.Contains("Access Denied"))
        {
            // If the AI correctly denies access, we catch it here.
            return "I'm sorry, I can only provide information related to your own account.";
        }

        string? jsonResult = await ExecuteSqlQuery(sqlQuery);
        if (jsonResult == null)
        {
            return "I executed the query but found no data matching your request, or the query failed structurally. Check backend logs.";
        }

        return await GetConversationalAnswerFromGemini(userQuestion, sqlQuery, jsonResult);
    }


    // --- Classifier (Unchanged) ---
    private async Task<string> ClassifyQuestionAsync(string userQuestion)
    {
        string prompt = $"""
            You are a query classifier. Your only job is to return a single word: DATABASE or GENERAL.
            - Return DATABASE for questions about specific data (counts, lists, sums, averages, e.g., "how many cars", "list all appointments", "total revenue for John").
            - Return GENERAL for all other questions (greetings, how-to questions, system features, e.g., "hi", "how do I book an appointment", "what is AutoServe 360", "how to use this system").

            User Question: "{userQuestion}"
            Classification:
            """;

        var request = new GeminiRequest { Contents = new List<Content> { new() { Parts = new List<Part> { new() { Text = prompt } } } } };
        string classification = await SendRequestToGemini(request);
        classification = classification.Trim().ToUpper();

        if (classification == "DATABASE") return "DATABASE";
        if (classification == "GENERAL") return "GENERAL";

        // Fallback logic
        var lowerQuestion = userQuestion.ToLower();
        if (lowerQuestion.StartsWith("hi") || lowerQuestion.StartsWith("hello") || lowerQuestion.Contains("how to") || lowerQuestion.Contains("what is"))
        {
            return "GENERAL";
        }
        return "DATABASE";
    }


    // --- UPDATED: GetSqlQueryFromGemini (Significantly More Secure) ---
    private async Task<string> GetSqlQueryFromGemini(string userQuestion, int userId, string userRole)
    {
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine(DatabaseSchema); // Base schema

        // --- NEW: Add dynamic, role-specific security rules ---
        if (userRole == "Customer")
        {
            promptBuilder.AppendLine("### SECURITY CONTEXT (MANDATORY) ###");
            promptBuilder.AppendLine($"- You are generating this query on behalf of a CUSTOMER with UserId = {userId}.");
            promptBuilder.AppendLine($"- **PRIMARY RULE:** All queries on customer-related tables (Appointments, CustomerVehicles, Payments, Reviews, Users) MUST contain a `WHERE` clause filtering for this user (e.g., `... WHERE UserId = {userId}`).");
            promptBuilder.AppendLine($"- **CONFLICT RULE:** If the user's question (e.g., 'show me Sara Smith's car') *conflicts* with your PRIMARY RULE, the PRIMARY RULE **ALWAYS WINS**.");
            promptBuilder.AppendLine("- In case of a conflict, you MUST generate a query that includes *both* the user's request AND the security filter. This will correctly return no results.");
            promptBuilder.AppendLine("- **Correct (Safe) Query for 'Show me Sara Smith's car':**");
            promptBuilder.AppendLine($"`SELECT v.Brand, v.Model FROM CustomerVehicles v JOIN Users u ON v.UserId = u.UserId WHERE u.FirstName = 'Sara' AND u.LastName = 'Smith' AND v.UserId = {userId};`");
            promptBuilder.AppendLine("- **Incorrect (UNSAFE) Query:** `SELECT ... WHERE u.FirstName = 'Sara';` (This is a security breach and must be avoided).");
        }
        else if (userRole == "Employee")
        {
            promptBuilder.AppendLine("### SECURITY CONTEXT (MANDATORY) ###");
            promptBuilder.AppendLine($"- You are generating this query on behalf of an EMPLOYEE with UserId = {userId}.");
            promptBuilder.AppendLine($"- **ALLOWED (PERSONAL):** You can query your *own* personal data. This includes your `TimeLogs` and your *assigned appointments* from `EmployeeAppointments`.");
            promptBuilder.AppendLine($"- **ALLOWED (JOINING):** To find your assigned appointments, you MUST join `EmployeeAppointments` with `Appointments` and filter `EmployeeAppointments.UserId = {userId}`.");
            promptBuilder.AppendLine($"- **ALLOWED (GENERAL):** You can query *all* data from general tables like `Services` and `ModificationRequests`.");
            promptBuilder.AppendLine($"- **DENIED (CUSTOMER DATA):** You MUST NOT generate queries that select *another user's* private data. This includes `Users`, `CustomerVehicles`, `Payments`, and `Reviews`.");
            promptBuilder.AppendLine($"- **CONFLICT RULE:** If the user asks for *another user's* data (e.g., 'Show me Mike's car' or 'list all customer appointments'), you MUST NOT generate a query. Instead, you MUST return the single string: `Access Denied`");
            promptBuilder.AppendLine("- **Example (Allowed):** `SELECT * FROM Services;`");
            promptBuilder.AppendLine("- **Example (Allowed):** `SELECT SUM(HoursLogged) FROM TimeLogs WHERE UserId = {userId};`");
            promptBuilder.AppendLine($"- **Example (Allowed for 'What appointments am I assigned to?'):** `SELECT a.AppointmentId, a.DateTime, a.Status FROM Appointments a JOIN EmployeeAppointments ea ON a.AppointmentId = ea.AppointmentId WHERE ea.UserId = {userId};`");
            promptBuilder.AppendLine("- **Example (Denied):** `SELECT * FROM CustomerVehicles WHERE Brand = 'Toyota';`");
            promptBuilder.AppendLine("- **Example (Denied):** `SELECT * FROM Appointments;` (This is unsafe as it's not filtered by employee ID).");
        }
        else if (userRole == "Admin")
        {
            promptBuilder.AppendLine("### SECURITY CONTEXT (ADMIN) ###");
            promptBuilder.AppendLine("- You are generating this query on behalf of an ADMIN.");
            promptBuilder.AppendLine("- You have full access to all tables and all user data. No security filters are required. You can fulfill any data request.");
        }
        // ----------------------------------------------------

        promptBuilder.AppendLine("\n--- END OF RULES ---");
        promptBuilder.AppendLine($"\nUser Question: {userQuestion}\n\nSQL Query:");

        string fullPrompt = promptBuilder.ToString();

        var request = new GeminiRequest { Contents = new List<Content> { new() { Parts = new List<Part> { new() { Text = fullPrompt } } } } };
        return await SendRequestToGemini(request);
    }

    // --- UNCHANGED HELPER: ExecuteSqlQuery ---
    private async Task<string?> ExecuteSqlQuery(string sqlQuery)
    {
        // First, check if the AI denied access at the generation stage (for Employees)
        if (sqlQuery.Trim() == "Access Denied")
        {
            Console.WriteLine("\n--- SQL Execution Halted: Access Denied by prompt rule. ---");
            return "[]"; // Return empty JSON to trigger "no results"
        }

        DbConnection connection = _dbContext.Database.GetDbConnection();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sqlQuery;
            command.CommandTimeout = 30;

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var reader = await command.ExecuteReaderAsync();
            if (!reader.HasRows)
            {
                return "[]";
            }

            var results = new List<Dictionary<string, object?>>();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.GetValue(i);
                    row[columnName] = value == DBNull.Value ? null : value;
                }
                results.Add(row);
            }
            return JsonSerializer.Serialize(results);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n--- SQL Execution Error ---\nQuery: {sqlQuery}\nError: {ex.Message}\n---------------------------\n");
            return null;
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }
    }

    // --- UNCHANGED HELPER: GetConversationalAnswerFromGemini ---
    private async Task<string> GetConversationalAnswerFromGemini(string userQuestion, string sqlQuery, string rawJsonResult)
    {
        string prompt = $"""
        The user asked: '{userQuestion}'.
        The database returned the following raw JSON data: '{rawJsonResult}'.

        Your task is to provide a concise, conversational answer.
        - If the JSON contains a single value (e.g., `[{"Count": 10}]`), answer the question directly.
        - If the JSON is an empty array `[]`, state that no results were found (e.g., "I couldn't find any results for that request.").
        - If the JSON contains a list of items, format the result as a simple and clean summary or a markdown table.
        - Format all currency amounts as LKR (Sri Lankan Rupees).
        """;

        var request = new GeminiRequest { Contents = new List<Content> { new() { Parts = new List<Part> { new() { Text = prompt } } } } };
        string finalAnswer = await SendRequestToGemini(request, "You are a friendly, helpful customer service assistant for a car service center.");
        return finalAnswer.Trim().Replace("\"", "");
    }

    // --- UNCHANGED HELPER: SendRequestToGemini ---
    private async Task<string> SendRequestToGemini(GeminiRequest request, string systemInstruction = null)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_geminiEndpoint}?key={_apiKey}");
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"\n--- Gemini API Error ---\nStatus: {response.StatusCode}\nContent: {errorContent}\n---------------------------\n");
            return string.Empty;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return geminiResponse?.Candidates?.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text?.Trim() ?? string.Empty;
    }
}