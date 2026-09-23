using System.Text.Json.Serialization;

namespace YoutubeTest.Shared.Models;

public class ContentDetails
{
    [JsonPropertyName("duration")]
    public string Duration { get; set; } = string.Empty;

    [JsonPropertyName("dimension")]
    public string Dimension { get; set; } = string.Empty;

    [JsonPropertyName("definition")]
    public string Definition { get; set; } = string.Empty;

    [JsonConverter(typeof(FlexibleBoolConverter))]
    [JsonPropertyName("caption")]
    public bool Caption { get; set; }

    [JsonConverter(typeof(FlexibleBoolConverter))]
    [JsonPropertyName("licensedContent")]
    public bool LicensedContent { get; set; }

    [JsonPropertyName("projection")]
    public string Projection { get; set; } = string.Empty;
}
