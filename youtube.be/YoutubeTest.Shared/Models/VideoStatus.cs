using System.Text.Json.Serialization;

namespace YoutubeTest.Shared.Models;

public class VideoStatus
{
    [JsonPropertyName("uploadStatus")]
    public string UploadStatus { get; set; } = string.Empty;

    [JsonPropertyName("privacyStatus")]
    public string PrivacyStatus { get; set; } = string.Empty;

    [JsonPropertyName("license")]
    public string License { get; set; } = string.Empty;

    [JsonConverter(typeof(FlexibleBoolConverter))]
    [JsonPropertyName("embeddable")]
    public bool Embeddable { get; set; }

    [JsonConverter(typeof(FlexibleBoolConverter))]
    [JsonPropertyName("publicStatsViewable")]
    public bool PublicStatsViewable { get; set; }

    [JsonConverter(typeof(FlexibleBoolConverter))]
    [JsonPropertyName("madeForKids")]
    public bool MadeForKids { get; set; }
}
