namespace automobile_backend.Models.Ai;

public class Candidate
{
    public Content Content { get; set; } = new();
    // Simplified model, other properties (e.g., safety, index) are ignored for this use case
}

public class GeminiResponse
{
    public List<Candidate> Candidates { get; set; } = new();
}