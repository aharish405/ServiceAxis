using ServiceAxis.Application.Common.Models;
using ServiceAxis.Application.Contracts.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceAxis.Infrastructure.Services;

public class PlatformEventPublisher : IPlatformEventPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public PlatformEventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<T>(T @event) where T : PlatformEvent
    {
        // For monolith, we directly invoke the engine. 
        // We use a scope to ensure fresh DbContext if needed, 
        // though the engine itself will likely handle its own scoping/units of work.
        
        using var scope = _serviceProvider.CreateScope();
        var engine = scope.ServiceProvider.GetRequiredService<IAutomationEngine>();
        await engine.ProcessEventAsync(@event);
    }
}
