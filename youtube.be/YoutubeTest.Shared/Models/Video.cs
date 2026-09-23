using System.Text.Json.Serialization;

namespace YoutubeTest.Shared.Models;

public class Video
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("etag")]
    public string Etag { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("snippet")]
    public VideoSnippet? Snippet { get; set; }

    [JsonPropertyName("contentDetails")]
    public ContentDetails? ContentDetails { get; set; }

    [JsonPropertyName("status")]
    public VideoStatus? Status { get; set; }

    [JsonPropertyName("statistics")]
    public VideoStatistics? Statistics { get; set; }
}
