using Microsoft.AspNetCore.Diagnostics;

namespace TaskTracker.Api.Errors;

public sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, body) = ErrorMapper.Map(exception);

        // 4xx are expected outcomes of bad input; only unexpected failures are logged as errors.
        if (status >= StatusCodes.Status500InternalServerError)
            LogUnhandledException(exception, httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(body, cancellationToken);
        return true;
    }

    // Source-generated logging (no EventId: the generator derives a stable one from the method name).
    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception processing {Method} {Path}")]
    private partial void LogUnhandledException(Exception exception, string method, string path);
}
