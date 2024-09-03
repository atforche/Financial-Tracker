using System.Text.Json.Serialization;
using Application.Services;
using Data;
using Data.Repositories;
using Domain.Repositories;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure the JSON serializer to serialize enums as their string values
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Configure the DbContext to connect to the database
builder.Services.AddDbContext<DatabaseContext>();

// Add DI services
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>(
    service => new AccountService(service.GetRequiredService<IAccountRepository>()));

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
builder.Services.AddSwaggerGen();

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