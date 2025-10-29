namespace automobile_backend.InterFaces.IServices
{
    public interface IChatbotService
    {
        // --- UPDATED SIGNATURE (added userName) ---
        Task<string> AnswerQuestionAsync(string userQuestion, int userId, string userRole, string? userName);
    }
}