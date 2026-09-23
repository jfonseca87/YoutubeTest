using System.Text.Json.Serialization;

namespace YoutubeTest.Shared.Models;

public class VideoSnippet
{
    [JsonPropertyName("publishedAt")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("channelId")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("thumbnails")]
    public ThumbnailSet? Thumbnails { get; set; }

    [JsonPropertyName("channelTitle")]
    public string ChannelTitle { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public string[] Tags { get; set; } = [];

    [JsonPropertyName("categoryId")]
    public string CategoryId { get; set; } = string.Empty;

    [JsonPropertyName("liveBroadcastContent")]
    public string LiveBroadcastContent { get; set; } = string.Empty;

    [JsonPropertyName("defaultLanguage")]
    public string? DefaultLanguage { get; set; }

    [JsonPropertyName("localized")]
    public Localized? Localized { get; set; }

    [JsonPropertyName("defaultAudioLanguage")]
    public string? DefaultAudioLanguage { get; set; }
}
