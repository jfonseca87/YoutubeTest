using System.Runtime.CompilerServices;
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
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    private readonly AppSettings _settings = options.Value;
    private readonly int _batchSize = options.Value.BatchSize > 0 ? options.Value.BatchSize : 50;

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        var videoIds = await LoadVideoIdsAsync(_settings.InputPath, cancellationToken);
        logger.LogInformation("Input: {Count} video IDs from {Path}", videoIds.Count, _settings.InputPath);

        var batchCount = (videoIds.Count + _batchSize - 1) / _batchSize;
        logger.LogInformation("Processing {BatchCount} batches of up to {BatchSize}", batchCount, _batchSize);

        var missingIds = new HashSet<string>(StringComparer.Ordinal);
        var batches = FetchBatchesAsync(videoIds, batchCount, missingIds, cancellationToken);
        var totalVideos = await writer.WriteBatchesAsync(_settings.OutputPath, batches, cancellationToken);

        await SaveMissingVideosAsync(missingIds, cancellationToken);

        logger.LogInformation("Process completed: {Total} final videos", totalVideos);
    }

    private async IAsyncEnumerable<List<Video>> FetchBatchesAsync(
        IReadOnlyList<string> videoIds,
        int batchCount,
        HashSet<string> missingIds,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var current = 0;

        foreach (var batch in videoIds.Chunk(_batchSize))
        {
            current++;
            cancellationToken.ThrowIfCancellationRequested();

            logger.LogInformation("Batch {Current}/{Total} ({Count} IDs)", current, batchCount, batch.Length);

            List<Video> videos;

            try
            {
                videos = await fetcher.FetchVideosAsync(batch, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Batch {Current}/{Total} failed, continuing with the next one", current, batchCount);
                missingIds.UnionWith(batch);
                continue;
            }

            if (videos.Count < batch.Length)
            {
                var receivedIds = videos
                    .Select(v => v.Id)
                    .ToHashSet(StringComparer.Ordinal);

                missingIds.UnionWith(batch.Where(id => !receivedIds.Contains(id)));
            }

            yield return videos;
        }
    }

    private async Task SaveMissingVideosAsync(HashSet<string> missingIds, CancellationToken cancellationToken)
    {
        if (missingIds.Count == 0)
            return;

        var inputDirectory = Path.GetDirectoryName(_settings.InputPath);
        if (string.IsNullOrEmpty(inputDirectory))
            return;

        Directory.CreateDirectory(inputDirectory);

        var missingPath = Path.Combine(inputDirectory, "missing-videos.json");
        await using var stream = File.Create(missingPath);
        await JsonSerializer.SerializeAsync(stream, missingIds.ToList(), WriteOptions, cancellationToken);

        logger.LogWarning(
            "Missing videos: {MissingCount} IDs not returned. Saved to {Path}",
            missingIds.Count,
            missingPath);
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

        return ids;
    }

    private static void AddId(List<string> ids, JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var id = element.GetString();
            if (!string.IsNullOrWhiteSpace(id))
                ids.Add(id.Trim());
        }
    }
}
