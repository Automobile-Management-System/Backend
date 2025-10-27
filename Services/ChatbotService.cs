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

    // --- NEW: KNOWLEDGE BASE FOR GENERAL QUESTIONS ---
    private const string KnowledgeBase = """
        You are a helpful assistant for "AutoServe 360".
        
        ## System Overview
        In today’s world, people expect quick and convenient services, and the automobile
        industry is no exception. Many service centers still depend on manual processes for
        booking appointments, tracking service progress, and handling payments, which often
        leads to delays, miscommunication, and customer dissatisfaction.
        
        Our project, the Automobile Service Time Logging & Appointment System (AutoServe 360), is
        developed to overcome these issues. The system brings customers, employees, and
        administrators into one digital platform where everything can be managed efficiently.
        Customers will be able to book services, request modifications, make payments, and
        check updates in real time. Employees can log their working hours, manage assigned
        tasks, and update progress, while administrators can monitor the entire process, manage
        users, and analyze reports.
        By making use of modern tools and practices, this project aims to create a secure, user-friendly, and scalable system that improves the overall experience of both customers
        and service providers.
        
        ## User Roles & Features
        
        ### Customer
        - **Dashboard:** View personal data and added vehicles.
        - **Service:** Book new appointments for services.
        - **Modification:** Request modifications for their vehicles.
        - **Payment:** Make payments for services.
        - **About Us & Home:** View landing page details.
        
        ### Employee
        - **Dashboard:** View personalized employee data.
        - **Work Progress:** Start and stop time logs for tasks.
        - **View Services/Modifications:** Can view all available services and modifications.
        
        ### Admin
        - **Dashboard:** See all revenue, user counts, and system metrics.
        - **User Management:** Add employees, manage user roles, edit user data, and set users as active/inactive.
        - **Service Management:** Create new services offered by the center.
        - **Payment Management:** Manually mark cash payments as complete and view all transactions.
        - **Manage Appointments:** View and manage all service and modification appointments.
        - **Analytics:** Generate reports and view system analytics.
        """;

    // --- UNCHANGED: DATABASE SCHEMA FOR SQL QUESTIONS ---
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
        - Users.Role:
            0 = Admin
            1 = Employee
            2 = Customer
        - CustomerVehicles.FuelType:
            0 = Petrol
            1 = Diesel
            2 = Electric
            3 = Hybrid
        - Appointments.Status (AppointmentStatus):
            0 = Pending
            1 = Upcoming
            2 = InProgress
            3 = Completed
            4 = Rejected
        - Appointments.Type:
            0 = Modifications
            1 = Service
        - Payments.Status (PaymentStatus):
            0 = Pending
            1 = Completed
            2 = Failed
            3 = Refunded
        - Payments.PaymentMethod:
            0 = CreditCard
            1 = DebitCard
            2 = Cash
            3 = BankTransfer
        
        ## TABLE RELATIONSHIPS (How to JOIN)
        - **Users & Appointments (Customer)**: Users.UserId (Role=2) -> Appointments.UserId
        - **Users & CustomerVehicles (Customer)**: Users.UserId (Role=2) -> CustomerVehicles.UserId
        - **CustomerVehicles & Appointments**: CustomerVehicles.VehicleId -> Appointments.VehicleId
        
        - **Users & TimeLogs (Employee)**: Users.UserId (Role=1) -> TimeLogs.UserId
        - **Appointments & TimeLogs**: Appointments.AppointmentId -> TimeLogs.AppointmentId
        
        - **Users & Reports (Admin)**: Users.UserId (Role=0) -> Reports.UserId

        - **Appointments & Payments (One-to-One)**: Appointments.AppointmentId -> Payments.AppointmentId
        - **Appointments & Reviews (One-to-One)**: Appointments.AppointmentId -> Reviews.AppointmentId
        - **Appointments & ModificationRequests (One-to-Many)**: Appointments.AppointmentId -> ModificationRequests.AppointmentId

        - **Appointments <-> Services (Many-to-Many)**:
            - JOIN Appointments ON Appointments.AppointmentId = AppointmentServices.AppointmentId
            - JOIN Services ON Services.ServiceId = AppointmentServices.ServiceId
        
        - **Appointments <-> Users (Employee) (Many-to-Many)**:
            - JOIN Appointments ON Appointments.AppointmentId = EmployeeAppointments.AppointmentId
            - JOIN Users ON Users.UserId = EmployeeAppointments.UserId (WHERE Role=1)

        --- CRITICAL SQL RULES ---
        1. **Strictly use the table and column names listed above.**
        2. **ALWAYS USE ISNULL(AGGREGATE_FUNCTION, 0)** for all SUM, AVG, and COUNT.
        3. **Role-Based Joins**: Filter `Users.Role = 1` for employees, `Users.Role = 2` for customers, `Users.Role = 0` for admins.
        4. **String Matching**: Use `LIKE '%value%'` for partial string matching.
        5. **Use ENUM values**: Always use the numerical values provided.
        """;

    public ChatbotService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext dbContext)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _dbContext = dbContext;
        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini:ApiKey not found in configuration.");
    }

    // --- UPDATED: This is now a "Router" method ---
    public async Task<string> AnswerQuestionAsync(string userQuestion)
    {
        // Step 1: Classify the user's intent
        string category = await ClassifyQuestionAsync(userQuestion);

        // Step 2: Route to the correct answering pipeline
        switch (category)
        {
            case "DATABASE":
                return await AnswerDatabaseQuestionAsync(userQuestion);
            case "GENERAL":
                return await AnswerGeneralQuestionAsync(userQuestion);
            default:
                // Fallback if classification fails
                return await AnswerGeneralQuestionAsync(userQuestion);
        }
    }

    // --- NEW: Pipeline for General Knowledge Questions ---
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

        var request = new GeminiRequest
        {
            Contents = new List<Content>
            {
                new() { Parts = new List<Part> { new() { Text = prompt } } }
            }
        };

        // Use a friendly persona for this type of answer
        string finalAnswer = await SendRequestToGemini(request, "You are a friendly, helpful customer service assistant for AutoServe 360.");
        return finalAnswer.Trim().Replace("\"", "");
    }

    // --- NEW: Pipeline for Database/SQL Questions ---
    // (This is the logic from your old AnswerQuestionAsync method)
    private async Task<string> AnswerDatabaseQuestionAsync(string userQuestion)
    {
        // 1. Generate SQL
        string sqlQuery = await GetSqlQueryFromGemini(userQuestion);
        if (string.IsNullOrWhiteSpace(sqlQuery))
        {
            return "I couldn't generate a valid query for that question. Please try rephrasing.";
        }

        // 2. Execute SQL
        string? jsonResult = await ExecuteSqlQuery(sqlQuery);
        if (jsonResult == null)
        {
            return "I executed the query but found no data matching your request, or the query failed structurally. Check backend logs.";
        }

        // 3. Convert JSON result to conversational answer
        return await GetConversationalAnswerFromGemini(userQuestion, sqlQuery, jsonResult);
    }


    // --- NEW: Classifier to route requests ---
    private async Task<string> ClassifyQuestionAsync(string userQuestion)
    {
        string prompt = $"""
            You are a query classifier. Your only job is to return a single word: DATABASE or GENERAL.
            - Return DATABASE for questions about specific data (counts, lists, sums, averages, e.g., "how many cars", "list all appointments", "total revenue for John").
            - Return GENERAL for all other questions (greetings, how-to questions, system features, e.g., "hi", "how do I book an appointment", "what is AutoServe 360", "how to use this system").

            User Question: "{userQuestion}"
            Classification:
            """;

        var request = new GeminiRequest
        {
            Contents = new List<Content> { new() { Parts = new List<Part> { new() { Text = prompt } } } }
        };

        string classification = await SendRequestToGemini(request);

        // Clean and validate the response
        classification = classification.Trim().ToUpper();
        if (classification == "DATABASE") return "DATABASE";
        if (classification == "GENERAL") return "GENERAL";

        // Fallback logic for safety
        var lowerQuestion = userQuestion.ToLower();
        if (lowerQuestion.StartsWith("hi") || lowerQuestion.StartsWith("hello") || lowerQuestion.Contains("how to") || lowerQuestion.Contains("what is"))
        {
            return "GENERAL";
        }

        // If unsure, default to trying it as a database query
        return "DATABASE";
    }


    // --- UNCHANGED HELPER: GetSqlQueryFromGemini ---
    private async Task<string> GetSqlQueryFromGemini(string userQuestion)
    {
        string fullPrompt = $"{DatabaseSchema}\n\nUser Question: {userQuestion}\n\nSQL Query:";
        var request = new GeminiRequest
        {
            Contents = new List<Content> { new() { Parts = new List<Part> { new() { Text = fullPrompt } } } }
        };
        return await SendRequestToGemini(request);
    }

    // --- UNCHANGED HELPER: ExecuteSqlQuery ---
    private async Task<string?> ExecuteSqlQuery(string sqlQuery)
    {
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
        - If the JSON contains a single value (e.g., `[{"Count": 10}]`), answer the question directly. Example: "There are a total of 10 completed appointments."
        - If the JSON is an empty array `[]`, state that no results were found. Example: "I couldn't find any vehicles with that brand."
        - If the JSON contains a list of items, format the result as a simple and clean summary or a markdown table.
        - Format all currency amounts as LKR (Sri Lankan Rupees).
        """;

        var request = new GeminiRequest
        {
            Contents = new List<Content> { new() { Parts = new List<Part> { new() { Text = prompt } } } }
        };

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