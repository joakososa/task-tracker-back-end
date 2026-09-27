using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskTracker.Application.Common.Interfaces;
using TaskTracker.Application.Users;
using TaskTracker.Domain.Services;
using TaskTracker.Infrastructure.Persistence;
using TaskTracker.Infrastructure.Persistence.Interceptors;
using TaskTracker.Infrastructure.Persistence.Repositories;
using TaskTracker.Infrastructure.Security;

namespace TaskTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) => options
            .UseSqlServer(connectionString)
            .AddInterceptors(sp.GetRequiredService<AuditInterceptor>()));

        // Same scoped AppDbContext instance for repositories and the unit of work.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
