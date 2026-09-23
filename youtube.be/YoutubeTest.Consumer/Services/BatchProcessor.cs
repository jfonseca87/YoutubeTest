using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YoutubeTest.Consumer.Models;
using YoutubeTest.Shared.Models;

namespace YoutubeTest.Consumer.Services;

public class BatchProcessor(
    YouTubeFetcher fetcher,
    JsonOutputWriter writer,
    ILogger<BatchProcessor> logger,
    IOptions<AppSettings> options)
{
    private readonly AppSettings _settings = options.Value;
    private readonly int _batchSize = options.Value.BatchSize > 0 ? options.Value.BatchSize : 50;

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        var videoIds = await LoadVideoIdsAsync(_settings.InputPath, cancellationToken);
        logger.LogInformation("Input: {Count} video IDs from {Path}", videoIds.Count, _settings.InputPath);

        var allVideos = new List<Video>();
        var batches = videoIds.Chunk(_batchSize).ToList();
        logger.LogInformation("Processing {BatchCount} batches of up to {BatchSize}", batches.Count, _batchSize);

        for (var i = 0; i < batches.Count; i++)
        {
            var batch = batches[i];
            logger.LogInformation("Batch {Current}/{Total} ({Count} IDs)", i + 1, batches.Count, batch.Length);

            try
            {
                var videos = await fetcher.FetchVideosAsync(batch, cancellationToken);
                allVideos.AddRange(videos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Batch {Current}/{Total} failed, continuing with the next one", i + 1, batches.Count);
            }
        }

        await writer.WriteAsync(_settings.OutputPath, allVideos, cancellationToken);
        logger.LogInformation("Process completed: {Total} final videos", allVideos.Count);
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
