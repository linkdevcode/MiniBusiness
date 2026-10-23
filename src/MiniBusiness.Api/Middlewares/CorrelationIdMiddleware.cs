using System.Diagnostics;
using Microsoft.Extensions.Primitives;

namespace MiniBusiness.Api.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrGenerateCorrelationId(context);

        // Save to HttpContext.Items
        context.Items["CorrelationId"] = correlationId;

        // Synchronize with Activity (supports Distributed Tracing if OpenTelemetry is used later)
        Activity.Current?.SetTag("correlation.id", correlationId);
        
        // Ensure the header is always included in the response, even if the request fails
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
            {
                context.Response.Headers[CorrelationIdHeaderName] = new StringValues(correlationId);
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId)
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}