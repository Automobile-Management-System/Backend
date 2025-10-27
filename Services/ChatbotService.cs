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

    // --- UPDATED: More robust schema and rules ---
    private const string DatabaseSchema = """
        You are a professional SQL generation AI for a car service management system.
        Your SOLE task is to convert natural language questions into a single, valid, optimized SQL Server SELECT query.
        You MUST ONLY return the SQL query, without any markdown (```), explanation, or commentary.
        DO NOT generate any INSERT, UPDATE, or DELETE queries. The connection is READ-ONLY.

        VALID TABLE AND COLUMN SCHEMA:
        - Appointments (AppointmentId, DateTime, Status, Amount, Type, UserId, VehicleId)
        - Users (UserId, FirstName, LastName, Role)
        - CustomerVehicles (VehicleId, Brand, Model, Year, RegistrationNumber, FuelType)
        - Services (ServiceId, ServiceName, BasePrice)
        - TimeLogs (LogId, HoursLogged, AppointmentId, UserId)
        - Reports (ReportId, UserId, GeneratedDate)
        - Reviews (ReviewId, Rating, AppointmentId)
        - AppointmentServices (AppointmentId, ServiceId) -- Join table
        - EmployeeAppointments (AppointmentId, UserId) -- Join table

        CRITICAL SQL Rules:
        1. **Strictly use the table and column names listed above.** Do not query tables that are not in the list (e.g., 'Parts', 'Suppliers').
        2. **STRICTLY USE NUMERICAL ENUM VALUES**:
            - Role: Admin=0, Employee=1, Customer=2
            - Status: Pending=0, InProgress=1, Completed=2, Cancelled=3, Rejected=4
            - FuelType: Petrol=0, Diesel=1, Electric=2, Hybrid=3
            - Type: Modifications=0, Service=1
        3. **ALWAYS USE ISNULL(AGGREGATE_FUNCTION, 0)** for all SUM, AVG, and COUNT to prevent NULL results on empty data sets.
        4. Use LIKE '%value%' for partial string matching on columns like Brand or Model.
        """;
    // -----------------------------------------------------------------

    public ChatbotService(HttpClient httpClient, IConfiguration configuration, ApplicationDbContext dbContext)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _dbContext = dbContext;
        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini:ApiKey not found in configuration.");
    }

    public async Task<string> AnswerQuestionAsync(string userQuestion)
    {
        // 1. Generate SQL from Question using Gemini
        string sqlQuery = await GetSqlQueryFromGemini(userQuestion);

        if (string.IsNullOrWhiteSpace(sqlQuery))
        {
            return "I couldn't generate a valid query for that question. Please try rephrasing.";
        }

        // 2. Execute SQL and get JSON result
        // UPDATED: Now expects a JSON string, not an object.
        string? jsonResult = await ExecuteSqlQuery(sqlQuery);

        if (jsonResult == null)
        {
            // This fallback now only triggers if the SQL query itself fails (e.g., syntax error)
            return "I executed the query but found no data matching your request, or the query failed structurally. Check backend logs.";
        }

        // 3. Convert JSON result into a conversational answer using Gemini
        return await GetConversationalAnswerFromGemini(userQuestion, sqlQuery, jsonResult);
    }

    // --- Helper Methods: SQL Generation ---

    private async Task<string> GetSqlQueryFromGemini(string userQuestion)
    {
        string fullPrompt = $"{DatabaseSchema}\n\nUser Question: {userQuestion}\n\nSQL Query:";

        var request = new GeminiRequest
        {
            Contents = new List<Content>
            {
                new() { Parts = new List<Part> { new() { Text = fullPrompt } } }
            }
        };

        return await SendRequestToGemini(request);
    }

    // --- Helper Methods: SQL Execution ---

    // UPDATED: This method now returns a JSON string, not a single object.
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

            // Use ExecuteReaderAsync to handle multiple rows and columns
            using var reader = await command.ExecuteReaderAsync();

            // If the query returns nothing, send back an empty array JSON.
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

            // Serialize the list of rows into a JSON string
            return JsonSerializer.Serialize(results);
        }
        catch (Exception ex)
        {
            // CRITICAL LOGGING: Check your console for the generated SQL and the exact error.
            Console.WriteLine($"\n--- SQL Execution Error ---\nQuery: {sqlQuery}\nError: {ex.Message}\n---------------------------\n");
            // Return null on failure to trigger the fallback message in AnswerQuestionAsync
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

    // --- Helper Methods: Conversational Output ---

    // UPDATED: This method now accepts a JSON string as the result.
    private async Task<string> GetConversationalAnswerFromGemini(string userQuestion, string sqlQuery, string rawJsonResult)
    {
        // UPDATED: The prompt now instructs the AI on how to read the JSON.
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
            Contents = new List<Content>
            {
                new() { Parts = new List<Part> { new() { Text = prompt } } }
            }
        };

        // Use a friendly system instruction for this final step
        string finalAnswer = await SendRequestToGemini(request, "You are a friendly, helpful customer service assistant for a car service center.");

        // Final cleanup for cleanliness
        return finalAnswer.Trim().Replace("\"", "");
    }

    // --- Helper Methods: Gemini API Communication ---

    private async Task<string> SendRequestToGemini(GeminiRequest request, string systemInstruction = null)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_geminiEndpoint}?key={_apiKey}");

        // Note: The 'systemInstruction' part might need adjustment
        // based on the exact GeminiRequest schema you are using.
        // This example assumes it's part of the 'Contents' or a separate property.
        // For simplicity, we'll keep your original structure.

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(httpRequest);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            // This logs any API key errors or model rejection errors
            Console.WriteLine($"\n--- Gemini API Error ---\nStatus: {response.StatusCode}\nContent: {errorContent}\n---------------------------\n");
            return string.Empty;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return geminiResponse?.Candidates?.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text?.Trim() ?? string.Empty;
    }
}