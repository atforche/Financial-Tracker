using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Models;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
bool shouldLaunchAPI = Rest.EnvironmentManager.ShouldLaunchAPI();
bool isTesting = builder.Environment.IsEnvironment("Testing");
string authenticationMode = Environment.GetEnvironmentVariable("AUTH_MODE") ?? "google";
bool usesDevelopmentAuthentication = string.Equals(authenticationMode, "development", StringComparison.OrdinalIgnoreCase);
bool usesGoogleAuthentication = string.Equals(authenticationMode, "google", StringComparison.OrdinalIgnoreCase);
if (!usesDevelopmentAuthentication && !usesGoogleAuthentication)
{
    throw new InvalidOperationException("AUTH_MODE must be either 'development' or 'google'.");
}
if (usesDevelopmentAuthentication && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException("AUTH_MODE=development is allowed only when ASPNETCORE_ENVIRONMENT is Development.");
}
string? googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
string[] allowedGoogleSubjects = (Environment.GetEnvironmentVariable("GOOGLE_ALLOWED_SUBJECTS") ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
if (shouldLaunchAPI && usesGoogleAuthentication
    && (string.IsNullOrWhiteSpace(googleClientId) || allowedGoogleSubjects.Length == 0))
{
    throw new InvalidOperationException("GOOGLE_CLIENT_ID and GOOGLE_ALLOWED_SUBJECTS must be configured before launching the API.");
}

_ = builder.Services.AddHttpContextAccessor();
_ = builder.Services.AddHealthChecks()
    .AddCheck<Rest.Health.DatabaseHealthCheck>("database", tags: ["ready"]);
Microsoft.AspNetCore.Authentication.AuthenticationBuilder authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    string authenticationScheme = isTesting
        ? Rest.Authentication.TestAuthenticationDefaults.Scheme
        : usesDevelopmentAuthentication
            ? Rest.Authentication.DevelopmentAuthenticationDefaults.Scheme
            : JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = authenticationScheme;
    options.DefaultChallengeScheme = authenticationScheme;
});
if (usesGoogleAuthentication)
{
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
}
if (isTesting)
{
    _ = authenticationBuilder.AddScheme<Rest.Authentication.TestAuthenticationOptions, Rest.Authentication.TestAuthenticationHandler>(
        Rest.Authentication.TestAuthenticationDefaults.Scheme,
        _ => { });
}
else if (usesDevelopmentAuthentication)
{
    _ = authenticationBuilder.AddScheme<Rest.Authentication.DevelopmentAuthenticationOptions, Rest.Authentication.DevelopmentAuthenticationHandler>(
        Rest.Authentication.DevelopmentAuthenticationDefaults.Scheme,
        _ => { });
}
Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder authorizationPolicyBuilder =
    new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser();
if (!isTesting && usesGoogleAuthentication && allowedGoogleSubjects.Length > 0)
{
    _ = authorizationPolicyBuilder.RequireClaim("sub", allowedGoogleSubjects);
}
_ = builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(authorizationPolicyBuilder.Build());
_ = builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        bool isReadRequest = HttpMethods.IsGet(context.Request.Method)
            || HttpMethods.IsHead(context.Request.Method)
            || HttpMethods.IsOptions(context.Request.Method);
        int permitLimit = isReadRequest ? 120 : 30;
        string subject = context.User.FindFirst("sub")?.Value
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            $"{subject}:{permitLimit}",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
});

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
if (shouldLaunchAPI)
{
    // The backend is reachable only through Caddy, which supplies these headers.
    // Clearing the defaults permits Caddy's dynamically assigned Compose address.
    ForwardedHeadersOptions forwardedHeadersOptions = new()
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        ForwardLimit = 1
    };
    forwardedHeadersOptions.KnownIPNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
    _ = app.UseForwardedHeaders(forwardedHeadersOptions);
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseCors();
_ = app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
}).AllowAnonymous();
_ = app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
}).AllowAnonymous();
app.MapControllers();

if (shouldLaunchAPI)
{
    // Construct the validated environment configuration before accepting requests.
    _ = Data.EnvironmentManager.Instance;
    _ = Rest.EnvironmentManager.Instance;

    app.Run();
}
else if (isTesting)
{
    app.Run();
}