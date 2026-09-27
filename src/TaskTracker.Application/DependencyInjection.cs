using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskTracker.Application.Common.Behaviors;
using TaskTracker.Application.Common.Interfaces;

namespace TaskTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.Scan(s => s.FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
            .AsImplementedInterfaces().WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
            .AsImplementedInterfaces().WithScopedLifetime());
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator<,>));
        return services;
    }
}