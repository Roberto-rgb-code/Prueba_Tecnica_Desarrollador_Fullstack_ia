using Microsoft.AspNetCore.Builder;
using Serilog;
using Toka.Shared.Extensions;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Gateway")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
    builder.Services.AddTokaSwagger("Toka Gateway");

    var app = builder.Build();
    app.UseCors();
    app.UseTokaSwagger("Toka Gateway");
    app.UseSerilogRequestLogging();
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Gateway" }));
    app.MapReverseProxy();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
