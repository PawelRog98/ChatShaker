using Microsoft.AspNetCore.Mvc.Testing;
using ChatShaker.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using ChatShaker.Infrastructure.Data;

namespace ChatShaker.IntegrationTests.Fixtures;

public class IntegrationTestsWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false);
        });

        builder.ConfigureServices((context, services) =>
        {
            var config = context.Configuration;
            var conn = config.GetConnectionString("DefaultConnection");

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(conn);
            });

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var dataBase = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dataBase.Database.EnsureCreated();
        });
    }
}
