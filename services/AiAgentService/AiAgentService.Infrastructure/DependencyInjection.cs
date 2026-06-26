using AiAgentService.Application.Interfaces;
using AiAgentService.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Toka.Shared.Extensions;

namespace AiAgentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAgentInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenAiSettings>(configuration.GetSection(OpenAiSettings.SectionName));
        services.Configure<QdrantSettings>(configuration.GetSection(QdrantSettings.SectionName));

        services.AddHttpClient<IOpenAiClient, OpenAiHttpClient>();
        services.AddHttpClient<IUserContextClient, HttpUserContextClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["Services:UserService"] ?? "http://localhost:5085/");
        });

        if (configuration.GetValue("Qdrant:Enabled", false))
            services.AddSingleton<IVectorStore, QdrantVectorStore>();
        else
            services.AddSingleton<IVectorStore, InMemoryVectorStore>();

        services.AddScoped<IAgentService, AgentAppService>();
        return services;
    }

    public static WebApplication ConfigureAgentApi(this WebApplication app)
    {
        app.UseTokaSwagger("AI Agent Service");
        app.UseSerilogRequestLogging();
        return app;
    }

    public static async Task SeedAgentKnowledgeAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var agent = scope.ServiceProvider.GetRequiredService<IAgentService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("AiAgentService.Seed");

        for (var i = 0; i < 20; i++)
        {
            try
            {
                await agent.SeedKnowledgeBaseAsync();
                logger.LogInformation("Knowledge base seeded successfully");
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Seed attempt {Attempt} failed, retrying...", i + 1);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        logger.LogError("Could not seed knowledge base after multiple attempts");
    }
}
