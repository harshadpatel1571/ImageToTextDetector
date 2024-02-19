using Newtonsoft.Json;

namespace ImageToTextDetector.Dto;

public class RequestPayload
{
    [JsonProperty("contents")]
    public Content[] Contents { get; set; }
}

public class Content
{
    [JsonProperty("parts")]
    public Part[] Parts { get; set; }
}

public class Part
{
    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("inlineData")]
    public InlineData InlineData { get; set; }
}

public class InlineData
{
    [JsonProperty("mimeType")]
    public string MimeType { get; set; }

    [JsonProperty("data")]
    public string Data { get; set; }
}
