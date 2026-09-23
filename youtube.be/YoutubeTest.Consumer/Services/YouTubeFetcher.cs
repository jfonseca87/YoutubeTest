using System.Text.Json;
using Microsoft.Extensions.Logging;
using YoutubeTest.Shared.Models;

namespace YoutubeTest.Consumer.Services;

public class YouTubeFetcher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YouTubeFetcher> _logger;
    private readonly string _token;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public YouTubeFetcher(HttpClient httpClient, ILogger<YouTubeFetcher> logger, string token)
    {
        _httpClient = httpClient;
        _logger = logger;
        _token = token;
    }

    public async Task<List<Video>> FetchVideosAsync(IReadOnlyList<string> videoIds, CancellationToken cancellationToken = default)
    {
        var results = new List<Video>();

        if (videoIds.Count == 0)
            return results;

        var idParam = string.Join(",", videoIds);
        var url = $"videos?part=snippet,contentDetails,status,statistics&id={Uri.EscapeDataString(idParam)}&key={Uri.EscapeDataString(_token)}";

        _logger.LogInformation("YouTube fetch: {Count} videos", videoIds.Count);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var result = await JsonSerializer.DeserializeAsync<YouTubeVideoListResponse>(stream, JsonOptions, cancellationToken);

        if (result?.Items is { Count: > 0 })
        {
            results.AddRange(result.Items);
            _logger.LogInformation("YouTube fetch OK: {Count} videos recibidos", result.Items.Count);
        }
        else
        {
            _logger.LogWarning("YouTube fetch: respuesta vacía");
        }

        return results;
    }

    private class YouTubeVideoListResponse
    {
        public List<Video>? Items { get; set; }
    }
}
