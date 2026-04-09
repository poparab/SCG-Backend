using MediatR;

namespace SCG.SharedKernel;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn => DateTime.UtcNow;
}
