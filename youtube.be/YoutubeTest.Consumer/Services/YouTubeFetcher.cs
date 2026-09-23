using System.Text.Json;
using Microsoft.Extensions.Logging;
using YoutubeTest.Shared.Models;

namespace YoutubeTest.Consumer.Services;

public class YouTubeFetcher(
    HttpClient httpClient,
    ILogger<YouTubeFetcher> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<Video>> FetchVideosAsync(IReadOnlyList<string> videoIds, CancellationToken cancellationToken = default)
    {
        var results = new List<Video>();

        if (videoIds.Count == 0)
            return results;

        var idParam = string.Join(",", videoIds);
        var url = $"videos?part=snippet,contentDetails,status,statistics&id={Uri.EscapeDataString(idParam)}";

        logger.LogInformation("YouTube fetch: {Count} videos", videoIds.Count);

        var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<YouTubeVideoListResponse>(stream, JsonOptions, cancellationToken);

        if (result?.Items is { Count: > 0 })
        {
            results.AddRange(result.Items);
            logger.LogInformation(
                "YouTube fetch OK: {Count} videos received (pageInfo totalResults: {TotalResults})",
                result.Items.Count,
                result.PageInfo?.TotalResults ?? 0);
        }
        else
        {
            logger.LogWarning(
                "YouTube fetch: empty items (pageInfo totalResults: {TotalResults})",
                result?.PageInfo?.TotalResults ?? 0);
        }

        return results;
    }
}
