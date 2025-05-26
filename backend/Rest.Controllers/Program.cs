using System.Reflection;
using System.Text.Json.Serialization;
using Data;
using Data.Repositories;
using Domain.AccountingPeriods;
using Domain.Accounts;
using Domain.Actions;
using Domain.BalanceEvents;
using Domain.Funds;
using Domain.Services;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Rest.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure the JSON serializer to serialize enums as their string values
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure the DbContext to connect to the database
builder.Services.AddDbContext<DatabaseContext>();

// Add application DI services
builder.Services.AddScoped<UnitOfWork>();

// Add domain DI services
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
builder.Services.AddScoped<IFundRepository, FundRepository>();
builder.Services.AddScoped<IBalanceEventRepository, BalanceEventRepository>();
builder.Services.AddScoped<AddAccountAction>();
builder.Services.AddScoped<AddAccountingPeriodAction>();
builder.Services.AddScoped<CloseAccountingPeriodAction>();
builder.Services.AddScoped<AddTransactionAction>();
builder.Services.AddScoped<PostTransactionAction>();
builder.Services.AddScoped<AddFundConversionAction>();
builder.Services.AddScoped<AddChangeInValueAction>();
builder.Services.AddScoped<AccountBalanceService>();
builder.Services.AddScoped<AddFundAction>();
builder.Services.AddScoped<BalanceEventDateValidator>();
builder.Services.AddScoped<BalanceEventFutureEventValidator>();

// Configure CORS to allow requests from select origins
string corsPolicyName = "CORS";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName,
        policy =>
        {
            _ = policy.WithOrigins("http://localhost:5000").AllowAnyHeader().AllowAnyMethod();
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
    options.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Example = new OpenApiString("yyyy-MM-dd")
    });
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();