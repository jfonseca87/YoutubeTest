using System.Text.Json;
using Microsoft.Extensions.Logging;
using YoutubeTest.Shared.Models;

namespace YoutubeTest.Consumer.Services;

public class JsonOutputWriter
{
    private readonly ILogger<JsonOutputWriter> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public JsonOutputWriter(ILogger<JsonOutputWriter> logger)
    {
        _logger = logger;
    }

    public async Task WriteAsync(string outputPath, IReadOnlyList<Video> videos, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(stream, videos, JsonOptions, cancellationToken);

        _logger.LogInformation("Output written: {Path} ({Count} videos)", outputPath, videos.Count);
    }
}
