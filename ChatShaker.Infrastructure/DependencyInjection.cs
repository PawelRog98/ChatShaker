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
using ChatShaker.Application.Interfaces;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Repositories;
using ChatShaker.Domain.Serivces;
using ChatShaker.Domain.Services;
using ChatShaker.Infrastructure.ChatRoomServices;
using ChatShaker.Infrastructure.FileManagement;
using ChatShaker.Infrastructure.Jobs;
using ChatShaker.Application.Jobs.Abstraction;
using ChatShaker.Infrastructure.Email.Smtp;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ChatShaker.Domain.Models.Email;

namespace ChatShaker.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            var emailSettings = new EmailSettings();
            configuration.GetSection("EmailConfiguration").Bind(emailSettings);
            services.AddSingleton(emailSettings);

            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFileManager, FileManager>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<ICodeGenerationService, CodeGenerationService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IChatRoomMembershipRepository, ChatRoomMembershipRepository>();
            services.AddScoped<IChatRoomKeyBlobRepository, ChatRoomKeyBlobRepository>();
            services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IMessageStatusRepository, MessageStatusRepository>();
            services.AddScoped<IUserPublicKeyRepository, UserPublicKeyRepository>();
            services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
            services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            services.AddScoped<IFileResourceRepository, FileResourceRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            #region JWT
            var jwtSettings = new JwtSettings();
            if (!environment.IsEnvironment("IntegrationTests"))
            {
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
                    Console.WriteLine($"[DEBUG-JWT] JWT Issuer: '{jwtSettings.JwtIssuer}'");
                    conf.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtSettings.JwtIssuer,
                        ValidAudience = jwtSettings.JwtIssuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.JwtKey)),
                    };

                    conf.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                                context.Token = accessToken;

                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"Auth failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                        }
                    };
                });
            }
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

                /*services.AddHangfire(conf =>
                    conf.UseRedisStorage(redis, new RedisStorageOptions
                    {
                        Prefix = "app_hangfire:",
                        InvisibilityTimeout = TimeSpan.FromMinutes(10)
                    }));*/
                
                var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                services.AddHangfire(conf =>
                    conf.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                        {
                            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                            QueuePollInterval = TimeSpan.Zero,
                            UseRecommendedIsolationLevel = true,
                            DisableGlobalLocks = true
                        }));
                
                services.AddHangfireServer(options =>
                    {
                        options.WorkerCount = Environment.ProcessorCount * 5;
                    });
                    
                services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString).LogTo(Console.WriteLine));
                services.AddDatabaseDeveloperPageExceptionFilter();
            }
            #endregion

            #region Jobs

            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IRecurringJob))
                .AddClasses(classes => classes.AssignableTo<IRecurringJob>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.AddSingleton<RecurringJobRegistrar>();

            #endregion

            return services;
        }
    }
}
