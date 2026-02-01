using LunchVoteApi.Data;
using LunchVoteApi.Middleware;
using LunchVoteApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext - check if already configured externally (e.g., by tests)
// In testing scenarios, skip SQL Server registration entirely
var isTestEnvironment = builder.Environment.EnvironmentName == "Testing";

if (!isTestEnvironment)
{
    builder.Services.AddDbContext<LunchVoteDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connectionString);
    });
}

// Register application services
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IVoteService, VoteService>();

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSPA", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Application Insights (optional)
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global exception handler
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowSPA");

app.UseAuthorization();

app.MapControllers();

// Apply migrations on startup in development (only for SQL Server, not for tests)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<LunchVoteDbContext>();
    
    try
    {
        // Check if we're using SQL Server before calling EnsureCreated
        if (dbContext.Database.IsSqlServer())
        {
            dbContext.Database.EnsureCreated();
        }
    }
    catch
    {
        // Ignore if database creation fails (e.g., SQL Server not available)
    }
}

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
