using automobile_backend.InterFaces.IServices;
using automobile_backend.Models.Ai;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        You are a helpful assistant for "AutoServe 30".
        
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
        - Users (Role=2) JOIN CustomerVehicles ON Users.UserId = CustomerVehicles.UserId
        - Users (Role=2) JOIN Appointments ON Users.UserId = Appointments.UserId
        - CustomerVehicles JOIN Appointments ON CustomerVehicles.VehicleId = Appointments.VehicleId
        - Users (Role=1) JOIN TimeLogs ON Users.UserId = TimeLogs.UserId
        - Appointments JOIN TimeLogs ON Appointments.AppointmentId = TimeLogs.AppointmentId
        - Users (Role=0) JOIN Reports ON Users.UserId = Reports.UserId
        - Appointments JOIN Payments ON Appointments.AppointmentId = Payments.AppointmentId (One-to-One)
        - Appointments JOIN Reviews ON Appointments.AppointmentId = Reviews.AppointmentId (One-to-One)
        - Appointments JOIN ModificationRequests ON Appointments.AppointmentId = ModificationRequests.AppointmentId (One-to-Many)
        - Appointments JOIN AppointmentServices ON Appointments.AppointmentId = AppointmentServices.AppointmentId
        - AppointmentServices JOIN Services ON AppointmentServices.ServiceId = Services.ServiceId
        - Appointments JOIN EmployeeAppointments ON Appointments.AppointmentId = EmployeeAppointments.AppointmentId
        - EmployeeAppointments JOIN Users (Role=1) ON EmployeeAppointments.UserId = Users.UserId

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

    // --- ***UPDATED***: Main Router Method (Handles Guests) ---
    public async Task<string> AnswerQuestionAsync(string userQuestion, int userId, string userRole, string? userName)
    {
        // *** Force Guests to General Knowledge ***
        if (userRole == "Guest")
        {
            Console.WriteLine("User is Guest, routing to GENERAL knowledge.");
            return await AnswerGeneralQuestionAsync(userQuestion, userRole, userName); // Pass Guest role/name
        }
        // ****************************************

        // Authenticated user logic: Classify and check sensitivity
        (string category, bool isSensitive) = await ClassifyQuestionAsync(userQuestion);

        if (isSensitive && userRole != "Admin")
        {
            return GetPoliteRefusal(userQuestion, userRole, userName); // Pass userName
        }

        switch (category)
        {
            case "DATABASE":
                return await AnswerDatabaseQuestionAsync(userQuestion, userId, userRole, userName); // Pass userName
            case "GENERAL":
            default:
                return await AnswerGeneralQuestionAsync(userQuestion, userRole, userName); // Pass userName
        }
    }

    // --- Polite Refusal Logic (Added userName) ---
    private string GetPoliteRefusal(string userQuestion, string userRole, string? userName)
    {
        string greeting = GetPersonalizedGreeting(userRole, userName);
        var lowerQuestion = userQuestion.ToLower();

        if (lowerQuestion.Contains("how many users") || lowerQuestion.Contains("total users") || lowerQuestion.Contains("all users"))
        {
            return $"{greeting} I can help with your specific account details, but I don't have access to the total number of users in the system.";
        }
        if (lowerQuestion.Contains("revenue") || lowerQuestion.Contains("total amount") || lowerQuestion.Contains("system earnings"))
        {
            return userRole == "Customer"
                ? $"{greeting} I can certainly help you with your payment history or show you how to make payments for your appointments. Would you like assistance with that?"
                : $"{greeting} I don't have access to overall system revenue figures. I can help you view services or log your work time if needed.";
        }
        return $"{greeting} I'm sorry, but I can only provide information directly related to your account or general system features.";
    }


    // --- ***UPDATED***: General Knowledge Pipeline (Personalized Greeting) ---
    private async Task<string> AnswerGeneralQuestionAsync(string userQuestion, string userRole, string? userName)
    {
        string greeting = GetPersonalizedGreeting(userRole, userName); // Get greeting

        string prompt = $"""
            {KnowledgeBase}
            ---
            Based ONLY on the knowledge base provided above, answer the user's question.
            Start your response with the provided personalized greeting: "{greeting}"
            Be friendly, concise, and helpful.
            If the question is just a simple greeting like "Hi" or "Hello", respond with the greeting and a brief welcome to AutoServe 360, asking how you can help.
            Always sound polite, professional, and responsive.
            User Question: "{userQuestion}"
            Answer:
            """;

        var request = new GeminiRequest { Contents = new List<Content> { new() { Parts = new List<Part> { new() { Text = prompt } } } } };
        string finalAnswer = await SendRequestToGemini(request, "You are a friendly and helpful AI agent for AutoServe 360.");

        // Ensure the answer starts with the greeting, handle potential AI inconsistencies
        if (!finalAnswer.Trim().StartsWith(greeting.Trim()))
        {
            return $"{greeting} {finalAnswer.Trim()}";
        }
        return finalAnswer.Trim().Replace("\"", "");
    }

    // --- ***UPDATED***: Database Pipeline (Accepts userName) ---
    private async Task<string> AnswerDatabaseQuestionAsync(string userQuestion, int userId, string userRole, string? userName)
    {
        string sqlQuery = await GetSqlQueryFromGemini(userQuestion, userId, userRole);
        if (string.IsNullOrWhiteSpace(sqlQuery) || sqlQuery.Contains("Access Denied"))
        {
            // Use polite refusal for access denied
            return GetPoliteRefusal("Access Denied Scenario", userRole, userName);
            // return "I'm sorry, I can only provide information related to your own account.";
        }

        string? jsonResult = await ExecuteSqlQuery(sqlQuery);
        if (jsonResult == null)
        {
            return "I executed the query but found no data matching your request, or the query failed structurally. Check backend logs.";
        }

        return await GetConversationalAnswerFromGemini(userQuestion, jsonResult, userRole, userName); // Pass userName
    }


    // --- Classifier (Unchanged) ---
    private async Task<(string Category, bool IsSensitive)> ClassifyQuestionAsync(string userQuestion)
    {
        string prompt = $"""
            You are a query classifier. Your only job is to return a classification in the format: CATEGORY | SENSITIVE_FLAG
            - CATEGORY can be: DATABASE or GENERAL.
            - SENSITIVE_FLAG can be: TRUE or FALSE.

            Classification Rules:
            - GENERAL: Greetings, how-to questions, system features (e.g., "hi", "how do I book?", "what is AutoServe 360?"). Always SENSITIVE_FLAG = FALSE.
            - DATABASE: Questions about specific data (counts, lists, sums, averages).
                - SENSITIVE_FLAG = TRUE if the question asks for:
                    - System-wide totals/aggregates (e.g., "total users", "total revenue", "how many employees?").
                    - Data explicitly about *another* specific user (e.g., "show Sara Smith's car", "appointments for Mike Johnson").
                - SENSITIVE_FLAG = FALSE if the question is about:
                    - The *current* user's own data (implicitly, e.g., "show my cars", "my appointments").
                    - General data not tied to specific users (e.g., "list all services", "Toyota cars").

            Examples:
            User Question: "Hi" -> GENERAL | FALSE
            User Question: "How do I pay?" -> GENERAL | FALSE
            User Question: "Show my cars" -> DATABASE | FALSE
            User Question: "How many cars do I have?" -> DATABASE | FALSE
            User Question: "List all services" -> DATABASE | FALSE
            User Question: "Show me Toyota cars" -> DATABASE | FALSE
            User Question: "How many users are in the system?" -> DATABASE | TRUE
            User Question: "What is the total system revenue?" -> DATABASE | TRUE
            User Question: "Show appointments for Sara Smith" -> DATABASE | TRUE
            User Question: "How many employees are there?" -> DATABASE | TRUE

            User Question: "{userQuestion}"
            Classification:
            """;

        var request = new GeminiRequest { Contents = new List<Content> { new() { Parts = new List<Part> { new() { Text = prompt } } } } };
        string response = await SendRequestToGemini(request);
        response = response.Trim().ToUpper();

        string category = "GENERAL";
        bool isSensitive = false;

        var parts = response.Split('|');
        if (parts.Length == 2)
        {
            string parsedCategory = parts[0].Trim();
            string parsedSensitive = parts[1].Trim();
            if (parsedCategory == "DATABASE") category = "DATABASE";
            if (parsedSensitive == "TRUE") isSensitive = true;
        }
        else
        {
            var lowerQuestion = userQuestion.ToLower();
            if (lowerQuestion.Contains("how many users") || lowerQuestion.Contains("total users") || lowerQuestion.Contains("revenue") || lowerQuestion.Contains("earnings") || Regex.IsMatch(lowerQuestion, @"(for|of|belonging to) [A-Z][a-z]+ [A-Z][a-z]+"))
            { category = "DATABASE"; isSensitive = true; }
            else if (!lowerQuestion.StartsWith("hi") && !lowerQuestion.StartsWith("hello") && !lowerQuestion.Contains("how to") && !lowerQuestion.Contains("what is"))
            { category = "DATABASE"; }
        }

        Console.WriteLine($"Question: '{userQuestion}' -> Classified as: {category}, Sensitive: {isSensitive}"); // Debug log
        return (category, isSensitive);
    }


    // --- ***UPDATED***: Security-Enhanced SQL Generation (Guest Check Added) ---
    private async Task<string> GetSqlQueryFromGemini(string userQuestion, int userId, string userRole)
    {
        // *** NEW: Explicitly block Guests from SQL generation ***
        if (userRole == "Guest")
        {
            Console.WriteLine("Guest user attempted database query. Blocking SQL generation.");
            return "Access Denied"; // Or return empty string
        }
        // *******************************************************

        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine(DatabaseSchema); // Base schema

        // Existing Role-based rules (unchanged)
        if (userRole == "Customer")
        {
            promptBuilder.AppendLine("### SECURITY CONTEXT (MANDATORY) ###");
            promptBuilder.AppendLine($"- You are generating this query on behalf of a CUSTOMER with UserId = {userId}.");
            promptBuilder.AppendLine($"- **PRIMARY RULE:** All queries on customer-related tables (Appointments, CustomerVehicles, Payments, Reviews, Users) MUST contain a `WHERE` clause filtering for this user (e.g., `... WHERE UserId = {userId}`).");
            promptBuilder.AppendLine($"- **CONFLICT RULE:** If the user's question (e.g., 'show me Sara Smith's car') *conflicts* with your PRIMARY RULE, the PRIMARY RULE **ALWAYS WINS**.");
            promptBuilder.AppendLine("- In case of a conflict, you MUST generate a query that includes *both* the user's request AND the security filter. This will correctly return no results.");
        }
        else if (userRole == "Employee")
        {
            promptBuilder.AppendLine("### SECURITY CONTEXT (MANDATORY) ###");
            promptBuilder.AppendLine($"- You are generating this query on behalf of an EMPLOYEE with UserId = {userId}.");
            promptBuilder.AppendLine($"- **ALLOWED (PERSONAL):** You can query your *own* personal data (`TimeLogs`, `EmployeeAppointments`).");
            promptBuilder.AppendLine($"- **ALLOWED (ASSIGNED APPOINTMENT DETAILS):** You can query details (Customer name, Vehicle info, Services, Modifications) for appointments *only if* assigned to you (requires JOIN through `EmployeeAppointments` filtered by `ea.UserId = {userId}`).");
            promptBuilder.AppendLine($"- **ALLOWED (GENERAL):** You can query *all* data from general tables like `Services`.");
            promptBuilder.AppendLine($"- **DENIED (UNASSIGNED/SYSTEM DATA):** You MUST NOT generate queries for: Appointments not assigned to you, Direct queries on CustomerVehicles/Payments/Reviews (unless joined via YOUR assigned appointment), Direct queries on Users (except customer name for YOUR assigned appointment), System-wide aggregates.");
            promptBuilder.AppendLine($"- **CONFLICT RULE:** If the user asks for *any denied* data, return the single string: `Access Denied`");
        }
        else if (userRole == "Admin")
        {
            promptBuilder.AppendLine("### SECURITY CONTEXT (ADMIN) ###");
            promptBuilder.AppendLine("- You are generating this query on behalf of an ADMIN.");
            promptBuilder.AppendLine("- You have full access to all tables and all user data.");
        }

        promptBuilder.AppendLine("\n--- END OF RULES ---");
        promptBuilder.AppendLine($"\nUser Question: {userQuestion}\n\nSQL Query:");

        string fullPrompt = promptBuilder.ToString();

        var request = new GeminiRequest { Contents = new List<Content> { new() { Parts = new List<Part> { new() { Text = fullPrompt } } } } };
        return await SendRequestToGemini(request);
    }

    // --- SQL Execution (Unchanged) ---
    private async Task<string?> ExecuteSqlQuery(string sqlQuery)
    {
        if (sqlQuery.Trim() == "Access Denied")
        {
            Console.WriteLine("\n--- SQL Execution Halted: Access Denied by prompt rule. ---");
            return "[]"; // Return empty JSON
        }

        DbConnection connection = _dbContext.Database.GetDbConnection();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sqlQuery; command.CommandTimeout = 30;
            if (connection.State != ConnectionState.Open) { await connection.OpenAsync(); }
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.HasRows) { return "[]"; }
            var results = new List<Dictionary<string, object?>>();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i) == DBNull.Value ? null : reader.GetValue(i);
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
            if (connection.State == ConnectionState.Open) { await connection.CloseAsync(); }
        }
    }

    // --- ***UPDATED***: Conversational Prompt (Includes Greeting Logic) ---
    private async Task<string> GetConversationalAnswerFromGemini(string userQuestion, string rawJsonResult, string userRole, string? userName)
    {
        string greeting = GetPersonalizedGreeting(userRole, userName); // Get greeting

        string prompt = $"""
        A user asked: '{userQuestion}'.
        You have queried the database and received the following raw JSON data: '{rawJsonResult}'.

        Your task is to act as a helpful "AutoServe 360" agent and provide a user-friendly, responsive answer based *only* on this data.
        - **Start:** Begin your response with the provided personalized greeting: "{greeting}"
        - **Tone:** Be polite, professional, and helpful.
        - **Currency:** Format all monetary values as LKR (e.g., LKR 1,500.00).
        - **Single Value:** If the JSON contains a single value, answer directly and clearly after the greeting.
        - **List of Items:** If the JSON contains multiple items, introduce the table politely after the greeting (e.g., "{greeting} Of course! Here is the information you requested:") followed by the markdown table.
        - **Empty Result:** If the JSON is an empty array `[]`, state politely after the greeting that you couldn't find a match.
            - *Example:* "{greeting} I've checked our records for you, but I couldn't find any information matching your request."
        - **Access Denied Scenario:** If the JSON is `[]` because access was appropriately denied, the "empty result" response is correct.
        
        Please provide only the final, clean answer to the user, starting with the greeting.
        """;

        var request = new GeminiRequest { Contents = new List<Content> { new() { Parts = new List<Part> { new() { Text = prompt } } } } };
        string finalAnswer = await SendRequestToGemini(request, "You are a helpful and polite AI agent for AutoServe 360. You are speaking directly to the user.");

        // Ensure the answer starts with the greeting
        if (!finalAnswer.Trim().StartsWith(greeting.Trim()))
        {
            return $"{greeting} {finalAnswer.Trim()}";
        }
        return finalAnswer.Trim().Replace("\"", "");
    }

    // --- ***NEW***: Helper to generate greeting ---
    private string GetPersonalizedGreeting(string userRole, string? userName)
    {
        // Use the actual name if available, otherwise use the role
        string namePart = !string.IsNullOrWhiteSpace(userName) ? userName.Split(' ')[0] : userRole; // Use first name or role

        switch (userRole)
        {
            case "Admin":
                return $"Hello Admin {namePart},";
            case "Employee":
                return $"Hello {namePart},"; // Employee greeting is simpler
            case "Customer":
                return $"Hello {namePart},"; // Customer greeting is simpler
            case "Guest":
            default:
                return "Hello, welcome to AutoServe 360!"; // Generic greeting for guests
        }
    }

    // --- Send Request (Unchanged) ---
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