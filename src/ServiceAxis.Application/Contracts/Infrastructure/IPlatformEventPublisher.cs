using ServiceAxis.Application.Common.Models;

namespace ServiceAxis.Application.Contracts.Infrastructure;

public interface IPlatformEventPublisher
{
    Task PublishAsync<T>(T @event) where T : PlatformEvent;
}
