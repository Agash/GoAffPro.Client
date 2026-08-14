using System.Net;
using System.Text;

namespace GoAffPro.Client.Tests;

internal sealed class TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responseFactory) : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _responseFactory = responseFactory;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = _responseFactory(request, cancellationToken);
        return Task.FromResult(response);
    }

    public static HttpResponseMessage JsonResponse(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json"),
        };
    }
}
