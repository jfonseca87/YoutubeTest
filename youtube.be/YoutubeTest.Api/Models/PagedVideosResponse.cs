using YoutubeTest.Shared.Models;

namespace YoutubeTest.Api.Models;

public record PagedVideosResponse(
    List<Video> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
