using System.Reflection;
using System.Text.Json.Serialization;
using Data;
using Data.Repositories;
using Domain.Entities;
using Domain.Events;
using Domain.Factories;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using RestApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure the JSON serializer to serialize enums as their string values
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure the DbContext to connect to the database
builder.Services.AddDbContext<DatabaseContext>();

// Configure MediatR to handler domain events
builder.Services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<IDomainEvent>());

// Add application DI services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add domain DI services
builder.Services.AddScoped<IAccountFactory, Account.AccountFactory>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountingPeriodFactory, AccountingPeriod.AccountingPeriodFactory>();
builder.Services.AddScoped<IAccountingPeriodRepository, AccountingPeriodRepository>();
builder.Services.AddScoped<ITransactionFactory, Transaction.TransactionFactory>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionDetailFactory, TransactionDetail.TransactionDetailFactory>();

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
    options.MapType(typeof(DateOnly), () => new OpenApiSchema
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