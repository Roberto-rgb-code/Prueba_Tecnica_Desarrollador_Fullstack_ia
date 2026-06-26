using AuthService.Infrastructure;
using Serilog;
using Toka.Shared.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "AuthService")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Services.AddControllers();
    builder.Services.AddTokaSwagger("Auth Service");
    builder.Services.AddAuthInfrastructure(builder.Configuration);

    var app = builder.Build();
    app.ConfigureAuthApi();
    app.MapControllers();
    await app.MigrateAuthDatabaseAsync();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Auth service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
