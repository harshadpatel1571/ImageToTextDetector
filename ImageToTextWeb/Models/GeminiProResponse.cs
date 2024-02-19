namespace ImageToTextWeb.Models;

public class GeminiProResponse
{
    public Content content { get; set; }
    public string finishReason { get; set; }
    public int index { get; set; }
    public List<SafetyRating> safetyRatings { get; set; }
}

public class Content
{
    public List<Part> parts { get; set; }
    public string role { get; set; }
}

public class Part
{
    public string text { get; set; }
}

public class PromptFeedback
{
    public List<SafetyRating> safetyRatings { get; set; }
}

public class Root
{
    public List<GeminiProResponse> candidates { get; set; }
    public PromptFeedback promptFeedback { get; set; }
}

public class SafetyRating
{
    public string category { get; set; }
    public string probability { get; set; }
}

