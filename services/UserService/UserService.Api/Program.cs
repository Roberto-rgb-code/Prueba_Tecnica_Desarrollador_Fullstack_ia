using UserService.Infrastructure;
using Serilog;
using Toka.Shared.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "UserService")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Services.AddControllers();
    builder.Services.AddTokaSwagger("User Service");
    builder.Services.AddUserInfrastructure(builder.Configuration);

    var app = builder.Build();
    app.ConfigureUserApi();
    app.MapControllers();
    await app.MigrateUserDatabaseAsync();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "User service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
