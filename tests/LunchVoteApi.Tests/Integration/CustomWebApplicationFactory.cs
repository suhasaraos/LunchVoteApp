using LunchVoteApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LunchVoteApi.Tests.Integration;

public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    // Use a consistent database name so all tests share the same InMemory database within a test run
    private const string DatabaseName = "InMemoryTestDb_Integration";
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set Testing environment BEFORE services are configured
        // This prevents Program.cs from registering SQL Server
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // Add in-memory database for testing
            services.AddDbContext<LunchVoteDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseName);
            });
        });
    }
}
