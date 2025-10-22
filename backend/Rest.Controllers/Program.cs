using System.Globalization;
using System.Text.Json.Serialization;
using Rest.Models;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure the JSON serializer to serialize enums as their string values
_ = builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

if (Rest.Controllers.EnvironmentManager.ShouldLaunchAPI())
{
    // Register needed DI services
    Data.ServiceManager.Register(builder.Services);
    Domain.ServiceManager.Register(builder.Services);

    // Configure CORS to allow requests from select origins
    _ = builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
            policy =>
            {
                _ = policy.WithOrigins(Rest.Controllers.EnvironmentManager.Instance.FrontendOrigin)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });

    // Configure logging
    _ = builder.Host.UseSerilog((context, configuration) =>
    {
        _ = configuration.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
            .WriteTo.File(Rest.Controllers.EnvironmentManager.Instance.LogDirectory + "/api-log-.log",
                rollingInterval: RollingInterval.Day,
                formatProvider: CultureInfo.InvariantCulture);
    });
}

// Configure OpenAPI document generation
builder.Services.AddOpenApi();
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    // Enable the Swagger UI
    _ = app.MapOpenApi();
    _ = app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Financial Tracker API"));
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors();
app.MapControllers();

if (Rest.Controllers.EnvironmentManager.ShouldLaunchAPI())
{
    // Ensure the database is healthy and the environment variables are all defined
    using IServiceScope serviceScope = app.Services.CreateScope();
    IServiceProvider services = serviceScope.ServiceProvider;
    services.GetRequiredService<Data.DatabaseContext>()?.RunHealthCheck();
    Data.EnvironmentManager.Instance.PrintEnvironment();
    Rest.Controllers.EnvironmentManager.Instance.PrintEnvironment();

    app.Run();
}