using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace CardPassportApi.Api.Middleware;

public class TracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TracingMiddleware> _logger;
    private static readonly ActivitySource ActivitySource = new("CardPassportApi");
    private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

    public TracingMiddleware(RequestDelegate next, ILogger<TracingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Request.Headers["X-Request-ID"].ToString() ?? Guid.NewGuid().ToString();
        context.Items["RequestId"] = requestId;

        using var activity = ActivitySource.StartActivity(
            $"{context.Request.Method} {context.Request.Path}",
            ActivityKind.Server);

        try
        {
            // Add request details to the activity
            activity?.SetTag("http.method", context.Request.Method);
            activity?.SetTag("http.url", context.Request.Path);
            activity?.SetTag("http.request_id", requestId);
            activity?.SetTag("http.client_ip", context.Connection.RemoteIpAddress?.ToString());

            // Track middleware execution time
            using var middlewareActivity = ActivitySource.StartActivity("Middleware Processing");
            var middlewareStartTime = Stopwatch.GetTimestamp();

            // Track authentication time
            using var authActivity = ActivitySource.StartActivity("Authentication");
            var authStartTime = Stopwatch.GetTimestamp();

            // Track authorization time
            using var authzActivity = ActivitySource.StartActivity("Authorization");
            var authzStartTime = Stopwatch.GetTimestamp();

            // Track request processing time
            using var processingActivity = ActivitySource.StartActivity("Request Processing");
            var processingStartTime = Stopwatch.GetTimestamp();

            // Track database operations
            using var dbActivity = ActivitySource.StartActivity("Database Operations");
            var dbStartTime = Stopwatch.GetTimestamp();

            // Track response time
            using var responseActivity = ActivitySource.StartActivity("Response Processing");
            var responseStartTime = Stopwatch.GetTimestamp();

            // Process the request
            await _next(context);

            // Record timing information
            var middlewareDuration = Stopwatch.GetElapsedTime(middlewareStartTime, Stopwatch.GetTimestamp());
            var authDuration = Stopwatch.GetElapsedTime(authStartTime, Stopwatch.GetTimestamp());
            var authzDuration = Stopwatch.GetElapsedTime(authzStartTime, Stopwatch.GetTimestamp());
            var processingDuration = Stopwatch.GetElapsedTime(processingStartTime, Stopwatch.GetTimestamp());
            var dbDuration = Stopwatch.GetElapsedTime(dbStartTime, Stopwatch.GetTimestamp());
            var responseDuration = Stopwatch.GetElapsedTime(responseStartTime, Stopwatch.GetTimestamp());

            // Add timing metrics to the activity
            activity?.SetTag("middleware.duration_ms", middlewareDuration.TotalMilliseconds);
            activity?.SetTag("auth.duration_ms", authDuration.TotalMilliseconds);
            activity?.SetTag("authz.duration_ms", authzDuration.TotalMilliseconds);
            activity?.SetTag("processing.duration_ms", processingDuration.TotalMilliseconds);
            activity?.SetTag("db.duration_ms", dbDuration.TotalMilliseconds);
            activity?.SetTag("response.duration_ms", responseDuration.TotalMilliseconds);
            activity?.SetTag("http.status_code", context.Response.StatusCode);

            // Log timing information
            _logger.LogInformation(
                "Request {RequestId} completed in {TotalDuration}ms: Auth={AuthDuration}ms, AuthZ={AuthZDuration}ms, Processing={ProcessingDuration}ms, DB={DBDuration}ms, Response={ResponseDuration}ms",
                requestId,
                middlewareDuration.TotalMilliseconds,
                authDuration.TotalMilliseconds,
                authzDuration.TotalMilliseconds,
                processingDuration.TotalMilliseconds,
                dbDuration.TotalMilliseconds,
                responseDuration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            activity?.SetTag("error.stack_trace", ex.StackTrace);
            throw;
        }
        finally
        {
            activity?.Stop();
        }
    }
} 