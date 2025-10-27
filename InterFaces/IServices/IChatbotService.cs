namespace automobile_backend.InterFaces.IServices
{
    public interface IChatbotService
    {
        Task<string> AnswerQuestionAsync(string userQuestion);
    }
}