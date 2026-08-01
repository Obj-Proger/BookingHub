using System.Reflection;
using BookingHub.Application.Common.Behaviors;
using BookingHub.Application.Common.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BookingHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDispatcher, Dispatcher>();

        RegisterOpenGenericImplementations(services, typeof(IRequestHandler<,>));
        RegisterOpenGenericImplementations(services, typeof(IValidator<>));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    private static void RegisterOpenGenericImplementations(IServiceCollection services, Type openGenericInterface)
    {
        var applicationAssembly = typeof(DependencyInjection).Assembly;

        foreach (var implementationType in applicationAssembly.GetTypes())
        {
            if (implementationType.IsAbstract || implementationType.IsInterface)
                continue;

            foreach (var implementedInterface in implementationType.GetInterfaces())
            {
                if (implementedInterface.IsGenericType && implementedInterface.GetGenericTypeDefinition() == openGenericInterface)
                    services.AddScoped(implementedInterface, implementationType);
            }
        }
    }
}