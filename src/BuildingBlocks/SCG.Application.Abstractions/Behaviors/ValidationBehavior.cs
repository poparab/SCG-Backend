using FluentValidation;
using MediatR;
using SCG.SharedKernel;

namespace SCG.Application.Abstractions.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errors = string.Join("; ", failures.Select(f => f.ErrorMessage));

        // Handle Result (no generic parameter)
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(errors);

        // Handle Result<T>
        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResponse);
            var failureMethod = resultType.GetMethod(nameof(Result<object>.Failure))!;
            return (TResponse)failureMethod.Invoke(null, [errors])!;
        }

        throw new ValidationException(failures);
    }
}
