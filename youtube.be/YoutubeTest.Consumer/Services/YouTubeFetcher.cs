using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YoutubeTest.Consumer.Models;
using YoutubeTest.Shared.Models;

namespace YoutubeTest.Consumer.Services;

public class YouTubeFetcher(
    HttpClient httpClient,
    ILogger<YouTubeFetcher> logger,
    IOptions<AppSettings> options)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
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

        await SaveMissingVideosAsync(videoIds, results, cancellationToken);

        return results;
    }

    private async Task SaveMissingVideosAsync(
        IReadOnlyList<string> sentIds,
        List<Video> receivedVideos,
        CancellationToken cancellationToken)
    {
        if (receivedVideos.Count >= sentIds.Count)
            return;

        var receivedIds = receivedVideos
            .Select(v => v.Id)
            .ToHashSet(StringComparer.Ordinal);

        var missingIds = sentIds
            .Where(id => !receivedIds.Contains(id))
            .ToList();

        if (missingIds.Count == 0)
            return;

        var inputDirectory = Path.GetDirectoryName(options.Value.InputPath);
        if (string.IsNullOrEmpty(inputDirectory))
            return;

        Directory.CreateDirectory(inputDirectory);

        var missingPath = Path.Combine(inputDirectory, "missing-videos.json");
        await using var stream = File.Create(missingPath);
        await JsonSerializer.SerializeAsync(stream, missingIds, WriteOptions, cancellationToken);

        logger.LogWarning(
            "YouTube fetch: {MissingCount} of {SentCount} IDs not returned. Missing IDs saved to {Path}",
            missingIds.Count,
            sentIds.Count,
            missingPath);
    }
}
