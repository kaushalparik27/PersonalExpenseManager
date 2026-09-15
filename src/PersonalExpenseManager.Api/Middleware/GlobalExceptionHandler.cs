using Microsoft.AspNetCore.Diagnostics;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.Api.Middleware;

// Catches anything that escapes controllers/filters so the client always gets the
// app's consistent ApiResponse envelope (spec §53) instead of the framework's bare
// error page or an empty body — and never a stack trace (spec §38).
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail("An unexpected error occurred. Please try again."),
            cancellationToken);

        return true;
    }
}
