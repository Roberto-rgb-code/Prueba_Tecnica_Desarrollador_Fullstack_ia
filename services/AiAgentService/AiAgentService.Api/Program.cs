using AiAgentService.Infrastructure;
using Serilog;
using Toka.Shared.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "AiAgentService")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Services.AddControllers();
    builder.Services.AddTokaSwagger("AI Agent Service");
    builder.Services.AddAgentInfrastructure(builder.Configuration);

    var app = builder.Build();
    app.ConfigureAgentApi();
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "AiAgentService" }));
    app.MapControllers();
    await app.SeedAgentKnowledgeAsync();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AI Agent service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
