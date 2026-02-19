using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ServiceAxis.Application.Behaviours;

namespace ServiceAxis.Application;

/// <summary>
/// Extension methods for registering Application-layer services with the DI container.
/// Call <c>services.AddApplicationServices()</c> from the API's <c>Program.cs</c>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Registers MediatR handlers, validators, and pipeline behaviours.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
