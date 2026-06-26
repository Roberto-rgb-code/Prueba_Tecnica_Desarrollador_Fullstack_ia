using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Toka.Shared.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureTokaApi(this WebApplication app, string serviceName)
    {
        app.UseSerilogRequestLogging();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = serviceName }));
        return app;
    }

    public static IServiceCollection AddTokaSwagger(this IServiceCollection services, string title)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = title, Version = "v1" }));
        return services;
    }

    public static WebApplication UseTokaSwagger(this WebApplication app, string title)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", title));
        return app;
    }
}
