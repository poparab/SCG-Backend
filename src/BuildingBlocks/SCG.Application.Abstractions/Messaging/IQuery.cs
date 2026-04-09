using MediatR;
using SCG.SharedKernel;

namespace SCG.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
