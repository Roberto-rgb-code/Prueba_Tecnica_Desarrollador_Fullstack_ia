using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Toka.Shared.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddTokaMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(configuration.GetSection(RabbitMqSettings.SectionName));

        if (configuration.GetValue("Messaging:UseRabbitMq", false))
            services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        else
            services.AddSingleton<IEventPublisher, NoOpEventPublisher>();

        return services;
    }
}
