using MediatR;

namespace ServiceAxis.Domain.Common;

/// <summary>
/// Marker interface for all domain events.
/// Implements INotification so MediatR can dispatch them.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>Gets the timestamp when the event occurred.</summary>
    DateTime OccurredAt { get; }
}

/// <summary>
/// Base record for domain events.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Aggregate root that can raise domain events.
/// </summary>
public abstract class AggregateRoot : BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Gets the raised domain events (read-only).</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>Adds a domain event to be dispatched.</summary>
    protected void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>Clears all raised domain events.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
