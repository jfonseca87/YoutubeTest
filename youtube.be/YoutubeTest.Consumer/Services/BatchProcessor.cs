using System.Text.Json;
using Microsoft.Extensions.Logging;
using YoutubeTest.Shared.Models;

namespace YoutubeTest.Consumer.Services;

public class BatchProcessor
{
    private readonly YouTubeFetcher _fetcher;
    private readonly JsonOutputWriter _writer;
    private readonly ILogger<BatchProcessor> _logger;
    private readonly int _batchSize;

    public BatchProcessor(YouTubeFetcher fetcher, JsonOutputWriter writer, ILogger<BatchProcessor> logger, int batchSize)
    {
        _fetcher = fetcher;
        _writer = writer;
        _logger = logger;
        _batchSize = batchSize > 0 ? batchSize : 50;
    }

    public async Task ProcessAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        var videoIds = await LoadVideoIdsAsync(inputPath, cancellationToken);
        _logger.LogInformation("Input: {Count} video IDs from {Path}", videoIds.Count, inputPath);

        var allVideos = new List<Video>();
        var batches = videoIds.Chunk(_batchSize).ToList();
        _logger.LogInformation("Processing {BatchCount} batches of up to {BatchSize}", batches.Count, _batchSize);

        for (var i = 0; i < batches.Count; i++)
        {
            var batch = batches[i];
            _logger.LogInformation("Batch {Current}/{Total} ({Count} IDs)", i + 1, batches.Count, batch.Length);

            try
            {
                var videos = await _fetcher.FetchVideosAsync(batch, cancellationToken);
                allVideos.AddRange(videos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch {Current}/{Total} failed, continuing with the next one", i + 1, batches.Count);
            }
        }

        await _writer.WriteAsync(outputPath, allVideos, cancellationToken);
        _logger.LogInformation("Process completed: {Total} final videos", allVideos.Count);
    }

    private static async Task<List<string>> LoadVideoIdsAsync(string inputPath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(inputPath);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var ids = new List<string>();

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in doc.RootElement.EnumerateArray())
                AddId(ids, element);
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            if (doc.RootElement.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in items.EnumerateArray())
                    AddId(ids, element);
            }
            else
            {
                AddId(ids, doc.RootElement);
            }
        }

        return ids;
    }

    private static void AddId(List<string> ids, JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var id = element.GetString();
            if (!string.IsNullOrWhiteSpace(id))
                ids.Add(id);
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("videoId", out var videoId) && videoId.ValueKind == JsonValueKind.String)
            {
                var id = videoId.GetString();
                if (!string.IsNullOrWhiteSpace(id))
                    ids.Add(id);
            }
            else if (element.TryGetProperty("id", out var idProp))
            {
                if (idProp.ValueKind == JsonValueKind.String)
                {
                    var id = idProp.GetString();
                    if (!string.IsNullOrWhiteSpace(id))
                        ids.Add(id);
                }
                else if (idProp.ValueKind == JsonValueKind.Object &&
                         idProp.TryGetProperty("videoId", out var nested) &&
                         nested.ValueKind == JsonValueKind.String)
                {
                    var id = nested.GetString();
                    if (!string.IsNullOrWhiteSpace(id))
                        ids.Add(id);
                }
            }
        }
    }
}
