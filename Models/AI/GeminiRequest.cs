namespace automobile_backend.Models.Ai;

public class Part
{
    public string Text { get; set; } = string.Empty;
}

public class Content
{
    public string Role { get; set; } = "user"; // Always "user" for our prompt injection
    public List<Part> Parts { get; set; } = new();
}

public class GeminiRequest
{
    public List<Content> Contents { get; set; } = new();
}