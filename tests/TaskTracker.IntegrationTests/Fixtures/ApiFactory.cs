using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TaskTracker.IntegrationTests.Fixtures;

/// <summary>
/// Runs the real Api (its Program.cs, DI and middleware) in memory, against the Testcontainers database.
/// </summary>
public sealed class ApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Program reads the connection string right after CreateBuilder. UseSetting is applied by then;
        // ConfigureAppConfiguration is not, and the tests would silently run against the local docker-compose DB.
        builder.UseSetting("ConnectionStrings:Default", connectionString);
    }
}
