using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Models;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
bool shouldLaunchAPI = Rest.EnvironmentManager.ShouldLaunchAPI();
bool isTesting = builder.Environment.IsEnvironment("Testing");
string? googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
string[] allowedGoogleSubjects = (Environment.GetEnvironmentVariable("GOOGLE_ALLOWED_SUBJECTS") ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
if (shouldLaunchAPI && (string.IsNullOrWhiteSpace(googleClientId) || allowedGoogleSubjects.Length == 0))
{
    throw new InvalidOperationException("GOOGLE_CLIENT_ID and GOOGLE_ALLOWED_SUBJECTS must be configured before launching the API.");
}

_ = builder.Services.AddHttpContextAccessor();
Microsoft.AspNetCore.Authentication.AuthenticationBuilder authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = isTesting ? Rest.Authentication.TestAuthenticationDefaults.Scheme : JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = isTesting ? Rest.Authentication.TestAuthenticationDefaults.Scheme : JwtBearerDefaults.AuthenticationScheme;
});
_ = authenticationBuilder.AddJwtBearer(options =>
    {
        options.Authority = "https://accounts.google.com";
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = ["https://accounts.google.com", "accounts.google.com"],
            ValidateAudience = true,
            ValidAudience = googleClientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            NameClaimType = "sub",
        };
    });
if (isTesting)
{
    _ = authenticationBuilder.AddScheme<Rest.Authentication.TestAuthenticationOptions, Rest.Authentication.TestAuthenticationHandler>(
        Rest.Authentication.TestAuthenticationDefaults.Scheme,
        _ => { });
}
Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder authorizationPolicyBuilder =
    new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser();
if (!isTesting && allowedGoogleSubjects.Length > 0)
{
    _ = authorizationPolicyBuilder.RequireClaim("sub", allowedGoogleSubjects);
}
_ = builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(authorizationPolicyBuilder.Build());

// Configure the JSON serializer to serialize enums as their string values
_ = builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

if (shouldLaunchAPI || isTesting)
{
    // Register needed DI services
    Data.ServiceManager.Register(builder.Services);
    Domain.ServiceManager.Register(builder.Services);
    Rest.ServiceManager.Register(builder.Services);

    if (shouldLaunchAPI)
    {
        // Configure CORS to allow requests from select origins
        _ = builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    _ = policy.WithOrigins(Rest.EnvironmentManager.Instance.FrontendOrigin)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        // Configure logging
        _ = builder.Host.UseSerilog((context, configuration) =>
        {
            _ = configuration.ReadFrom.Configuration(context.Configuration)
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
                .WriteTo.File(Rest.EnvironmentManager.Instance.LogDirectory + "/api-log-.log",
                    rollingInterval: RollingInterval.Day,
                    formatProvider: CultureInfo.InvariantCulture);
        });
    }
    else
    {
        _ = builder.Services.AddCors();
    }
}

// Configure OpenAPI document generation
builder.Services.AddOpenApi(options =>
{
    // Manually add the FundAmountModel schema to the OpenAPI document since it's not directly used as a request or response model
    _ = options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components?.Schemas?.Add("FundAmountModel", new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["fundId"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" },
                ["fundName"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["amount"] = new OpenApiSchema { Type = JsonSchemaType.Number, Format = "decimal" }
            },
            Required = new HashSet<string> { "fundId", "fundName", "amount" }
        });
        return Task.CompletedTask;
    });
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    // Enable the Swagger UI
    _ = app.MapOpenApi();
    _ = app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Financial Tracker API"));
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors();
app.MapControllers();

if (shouldLaunchAPI)
{
    // Ensure the database is healthy and the environment variables are all defined
    using IServiceScope serviceScope = app.Services.CreateScope();
    IServiceProvider services = serviceScope.ServiceProvider;
    services.GetRequiredService<Data.DatabaseContext>()?.RunHealthCheck();
    Data.EnvironmentManager.Instance.PrintEnvironment();
    Rest.EnvironmentManager.Instance.PrintEnvironment();

    app.Run();
}
else if (isTesting)
{
    app.Run();
}