using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain.Repositories;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toka.Shared.Auth;
using Toka.Shared.Extensions;
using Toka.Shared.Messaging;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AuthDb")));

        services.AddScoped<IAuthUserRepository, AuthUserRepository>();
        services.AddScoped<IAuthService, AuthAppService>();
        services.AddTokaMessaging(configuration);
        services.AddTokaJwtAuth(configuration);

        return services;
    }

    public static WebApplication ConfigureAuthApi(this WebApplication app)
    {
        app.UseTokaSwagger("Auth Service");
        app.ConfigureTokaApi("AuthService");
        return app;
    }

    public static async Task MigrateAuthDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
