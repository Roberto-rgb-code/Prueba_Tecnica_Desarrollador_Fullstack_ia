using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoleService.Application.Interfaces;
using RoleService.Application.Services;
using RoleService.Domain.Repositories;
using RoleService.Infrastructure.Persistence;
using RoleService.Infrastructure.Repositories;
using Toka.Shared.Auth;
using Toka.Shared.Extensions;
using Toka.Shared.Messaging;

namespace RoleService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRoleInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

        services.AddDbContext<RoleDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("RoleDb")));

        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRoleService, RoleAppService>();
        services.AddTokaMessaging(configuration);
        services.AddTokaJwtAuth(configuration);

        return services;
    }

    public static WebApplication ConfigureRoleApi(this WebApplication app)
    {
        app.UseTokaSwagger("Role Service");
        app.ConfigureTokaApi("RoleService");
        return app;
    }

    public static async Task MigrateRoleDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RoleDbContext>();
        await db.Database.EnsureCreatedAsync();

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Domain.Entities.Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Full system access", IsActive = true },
                new Domain.Entities.Role { Id = Guid.NewGuid(), Name = "User", Description = "Standard user access", IsActive = true });
            await db.SaveChangesAsync();
        }
    }
}
