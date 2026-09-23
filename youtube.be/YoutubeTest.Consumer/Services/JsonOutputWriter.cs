using System.Text.Json;
using Microsoft.Extensions.Logging;
using YoutubeTest.Shared.Models;

namespace YoutubeTest.Consumer.Services;

public class JsonOutputWriter(ILogger<JsonOutputWriter> logger)
{
    private static readonly JsonWriterOptions WriterOptions = new()
    {
        Indented = true
    };

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<int> WriteBatchesAsync(
        string outputPath,
        IAsyncEnumerable<List<Video>> batches,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var totalVideos = 0;

        await using var file = File.Create(fullPath);
        await using var jsonWriter = new Utf8JsonWriter(file, WriterOptions);

        jsonWriter.WriteStartArray();

        await foreach (var videos in batches.WithCancellation(cancellationToken))
        {
            foreach (var video in videos)
            {
                JsonSerializer.Serialize(jsonWriter, video, SerializerOptions);
                totalVideos++;
            }

            jsonWriter.Flush();
        }

        jsonWriter.WriteEndArray();
        await jsonWriter.FlushAsync(cancellationToken);

        logger.LogInformation("Output written: {Path} ({Count} videos)", fullPath, totalVideos);

        return totalVideos;
    }
}
