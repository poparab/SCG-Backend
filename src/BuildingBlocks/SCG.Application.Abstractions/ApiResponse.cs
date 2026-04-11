namespace SCG.Application.Abstractions;

public sealed record ApiResponse<T>(bool Success, T? Data, string? Error, string? CorrelationId);

public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data, string? correlationId = null) =>
        new(true, data, null, correlationId);

    public static ApiResponse<T> Fail<T>(string error, string? correlationId = null) =>
        new(false, default, error, correlationId);
}
