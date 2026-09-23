using System.Text.Json.Serialization;

namespace YoutubeTest.Shared.Models;

public class VideoStatistics
{
    [JsonPropertyName("viewCount")]
    public string ViewCount { get; set; } = string.Empty;

    [JsonPropertyName("likeCount")]
    public string LikeCount { get; set; } = string.Empty;

    [JsonPropertyName("favoriteCount")]
    public string FavoriteCount { get; set; } = string.Empty;

    [JsonPropertyName("commentCount")]
    public string CommentCount { get; set; } = string.Empty;
}
