using AuditService.Application.Interfaces;
using AuditService.Domain.Repositories;
using AuditService.Infrastructure.Messaging;
using AuditService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toka.Shared.Auth;
using Toka.Shared.Extensions;
using Toka.Shared.Messaging;

namespace AuditService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoAuditSettings>(configuration.GetSection(MongoAuditSettings.SectionName));
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

        if (configuration.GetValue("MongoDb:UseMongo", false))
            services.AddSingleton<IAuditLogRepository, MongoAuditLogRepository>();
        else
            services.AddSingleton<IAuditLogRepository, InMemoryAuditLogRepository>();

        services.AddScoped<IAuditQueryService, AuditQueryService>();

        if (configuration.GetValue("Messaging:UseRabbitMq", false))
            services.AddHostedService<AuditEventConsumer>();

        services.AddTokaJwtAuth(configuration);
        return services;
    }

    public static WebApplication ConfigureAuditApi(this WebApplication app)
    {
        app.UseTokaSwagger("Audit Service");
        app.ConfigureTokaApi("AuditService");
        return app;
    }
}
