using ChatShaker.Core.Interfaces.Authentication;
using ChatShaker.Domain.Entities;
using ChatShaker.Infrastructure.Authentication;
using ChatShaker.Infrastructure.Configuration;
using ChatShaker.Infrastructure.Data;
using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Repositories;

namespace ChatShaker.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();

            #region JWT
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JWTAuth").Bind(jwtSettings);

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
            #endregion

            #region Connections
            if (!environment.IsEnvironment("IntegrationTests"))
            {
                var redisConnection = configuration.GetConnectionString("RedisConnection");
                var redis = ConnectionMultiplexer.Connect(redisConnection);

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "AppCacheData_";
                });

                services.AddHangfire(conf =>
                    conf.UseRedisStorage(redis, new RedisStorageOptions
                    {
                        Prefix = "app_hangfire:",
                        InvisibilityTimeout = TimeSpan.FromMinutes(10)
                    }));
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));
            services.AddDatabaseDeveloperPageExceptionFilter();
            #endregion

            return services;
        }
    }
}
