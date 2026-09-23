using System.Text.Json.Serialization;

namespace YoutubeTest.Shared.Models;

public class Localized
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
