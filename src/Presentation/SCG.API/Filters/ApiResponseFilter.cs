using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SCG.Application.Abstractions;

namespace SCG.API.Filters;

public sealed class ApiResponseFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var correlationId = context.HttpContext.Response.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? context.HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault();

        if (context.Result is ObjectResult objectResult)
        {
            var statusCode = objectResult.StatusCode ?? 200;

            // Don't wrap file results or already-wrapped responses
            if (objectResult.Value is FileContentResult or ApiResponse<object>)
            {
                await next();
                return;
            }

            if (statusCode >= 200 && statusCode < 300)
            {
                objectResult.Value = new ApiResponse<object>(true, objectResult.Value, null, correlationId);
            }
            else
            {
                // Extract error from anonymous { error = "..." } objects
                var error = ExtractError(objectResult.Value);
                objectResult.Value = new ApiResponse<object>(false, null, error, correlationId);
            }
        }

        await next();
    }

    private static string? ExtractError(object? value)
    {
        if (value is null) return null;

        var errorProp = value.GetType().GetProperty("error") ?? value.GetType().GetProperty("Error");
        return errorProp?.GetValue(value)?.ToString();
    }
}
