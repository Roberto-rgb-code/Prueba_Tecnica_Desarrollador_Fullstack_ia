using RoleService.Infrastructure;
using Serilog;
using Toka.Shared.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "RoleService")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Services.AddControllers();
    builder.Services.AddTokaSwagger("Role Service");
    builder.Services.AddRoleInfrastructure(builder.Configuration);

    var app = builder.Build();
    app.ConfigureRoleApi();
    app.MapControllers();
    await app.MigrateRoleDatabaseAsync();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Role service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
