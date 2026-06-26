using AuditService.Infrastructure;
using Serilog;
using Toka.Shared.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "AuditService")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Services.AddControllers();
    builder.Services.AddTokaSwagger("Audit Service");
    builder.Services.AddAuditInfrastructure(builder.Configuration);

    var app = builder.Build();
    app.ConfigureAuditApi();
    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Audit service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
