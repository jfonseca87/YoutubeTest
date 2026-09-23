using System.Text.Json.Serialization;

namespace YoutubeTest.Shared.Models;

public class YouTubeVideoListResponse
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("etag")]
    public string Etag { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<Video> Items { get; set; } = [];

    [JsonPropertyName("pageInfo")]
    public PageInfo? PageInfo { get; set; }
}
