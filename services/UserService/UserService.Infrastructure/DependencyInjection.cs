using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Caching;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Repositories;
using Toka.Shared.Auth;
using Toka.Shared.Extensions;
using Toka.Shared.Messaging;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUserInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

        services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("UserDb")));

        if (configuration.GetValue("Caching:UseRedis", false))
            services.AddStackExchangeRedisCache(options => options.Configuration = configuration.GetConnectionString("Redis"));
        else
            services.AddDistributedMemoryCache();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserAppService, UserAppService>();
        services.AddScoped<IUserCache, RedisUserCache>();
        services.AddTokaMessaging(configuration);
        services.AddTokaJwtAuth(configuration);

        return services;
    }

    public static WebApplication ConfigureUserApi(this WebApplication app)
    {
        app.UseTokaSwagger("User Service");
        app.ConfigureTokaApi("UserService");
        return app;
    }

    public static async Task MigrateUserDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
