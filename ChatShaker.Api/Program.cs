using ChatShaker.Api.Configuration;
using ChatShaker.Api.Filters.Hangfire;
using ChatShaker.Api.Hubs;
using ChatShaker.Api.Middlewares;
using ChatShaker.Api.SignalR;
using ChatShaker.Application;
using ChatShaker.Application.Common.Behaviors;
using ChatShaker.Application.Interfaces;
using ChatShaker.Application.Mapping;
using ChatShaker.Infrastructure;
using ChatShaker.Infrastructure.Data;
using ChatShaker.Infrastructure.Jobs;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore;

namespace ChatShaker.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var quizAppSpecificOrigins = "_quizAppSpecificOrigins";

            builder.Configuration.AddMainConfiguration();

            builder.Services
                .AddApplication()
                .AddInfrastructure(builder.Configuration, builder.Environment);

            builder.Services.AddValidatorsFromAssemblyContaining<MappingProfile>();
            #region CORS
            builder.Services.AddCors(options =>
            {
                var urls = builder.Configuration.GetSection("CORS:CorsUrls").Get<List<string>>();
                options.AddPolicy(name: quizAppSpecificOrigins, policy =>
                {
                    policy.WithOrigins(urls.ToArray());
                });
            });
            #endregion

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddScoped<DataSeeder>();

            if (!builder.Environment.IsEnvironment("IntegrationTests"))
            {
                builder.Services.AddSignalR()
                    .AddStackExchangeRedis(builder.Configuration.GetConnectionString("RedisConnection"), options =>
                    {
                        options.Configuration.ChannelPrefix = "ChatShaker_App";
                    });
            }
            else
            {
                builder.Services.AddSignalR();
            }

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<RequestExceptionMiddleware>();
            builder.Services.AddScoped<IChatNotifier, ChatNotifier>();

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            var app = builder.Build();

            app.UseMiddleware<RequestExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chat-Shaker API v1");
                });
            }

            app.UseHttpsRedirection();

            if (!app.Environment.IsEnvironment("IntegrationTests"))
            {
                using var scope = app.Services.CreateScope();
                var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                await seeder.Seed();
                
                var registrar = scope.ServiceProvider.GetRequiredService<RecurringJobRegistrar>();
                registrar.Register();
            }
            
            app.UseAuthentication();
            app.UseAuthorization();
            
            var hangfireCredentials =
                builder.Configuration.GetSection("Hangfire:Dashboard").Get<HangfireDashboardSettings>();

            if (hangfireCredentials is not null)
            {
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[]
                        { new HangfireAuthFilter(hangfireCredentials.Username, hangfireCredentials.Password) },
                });
            }


            app.MapControllers();
            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }


    }
}
