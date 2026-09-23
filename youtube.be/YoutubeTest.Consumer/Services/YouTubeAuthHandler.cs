using System.Net.Http.Headers;

namespace YoutubeTest.Consumer.Services;

public class YouTubeAuthHandler(string token) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return base.SendAsync(request, cancellationToken);
    }
}
