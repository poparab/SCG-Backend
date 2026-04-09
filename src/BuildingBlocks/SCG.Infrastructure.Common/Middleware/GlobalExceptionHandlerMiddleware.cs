using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SCG.SharedKernel;
using System.Net;
using System.Text.Json;

namespace SCG.Infrastructure.Common.Middleware;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred. CorrelationId: {CorrelationId}",
                context.Items["CorrelationId"]);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                status = 500,
                title = "An unexpected error occurred.",
                correlationId = context.Items["CorrelationId"]?.ToString()
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
