using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class DownstreamLoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;

    public DownstreamLoggingHandler(HttpMessageHandler innerHandler, ILogger logger)
        : base(innerHandler)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("log from downstream logging handler");
        _logger.LogInformation("[Proxy OUT] → {Method} {Url}", request.Method, request.RequestUri);

        if (request.Headers.Authorization != null)
        {
            _logger.LogInformation("[Proxy OUT] Auth Header: Bearer {TokenPart}...",
                request.Headers.Authorization.Parameter?.Substring(0, 10));
        }

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogInformation("[Proxy IN] ← Response: {StatusCode}", response.StatusCode);

        return response;
    }
}
