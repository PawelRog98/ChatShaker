using Microsoft.AspNetCore.Mvc.Testing;
using ChatShaker.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using ChatShaker.Infrastructure.Data;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;
using ChatShaker.IntegrationTests.Helpers;
using ChatShaker.Infrastructure.Configuration;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace ChatShaker.IntegrationTests.Fixtures;

public class IntegrationTestsWebAppFactory : WebApplicationFactory<Program>
{
    public DbConnection Connection { get; private set; }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();
            var projectDir = Directory.GetCurrentDirectory();
            var path = Path.Combine(projectDir, "appsettings.Test.json");

            config.AddJsonFile(path, optional: false);
        });

        builder.ConfigureServices((context, services) =>
        {
            var config = context.Configuration;
            var conn = config.GetConnectionString("DefaultConnection");

            var jwtDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(JwtSettings));
            if (jwtDescriptor != null)
                services.Remove(jwtDescriptor);

            var jwtSettings = new JwtSettings();
            config.GetSection("JWTAuth").Bind(jwtSettings);
            services.AddSingleton(jwtSettings);

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = "Bearer";
                option.DefaultScheme = "Bearer";
                option.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer(conf =>
            {
                conf.RequireHttpsMetadata = false;
                conf.SaveToken = true;
                conf.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtSettings.JwtIssuer,
                    ValidAudience = jwtSettings.JwtIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.JwtKey)),
                };
            });

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            var redisDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));

            if (redisDescriptor != null)
                services.Remove(redisDescriptor);

            var cacheDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDistributedCache));
            if (cacheDescriptor != null)
                services.Remove(cacheDescriptor);

            Connection = new SqlConnection(conn);
            Connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer((SqlConnection)Connection);
            });

            services.AddDistributedMemoryCache();

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            //TestDataSeeder.SeedUser(db).GetAwaiter().GetResult();
        });

        builder.UseEnvironment("IntegrationTests");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Connection?.Dispose();
    }
}
