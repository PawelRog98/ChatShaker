using ChatShaker.Application.Common.Behaviors;
using ChatShaker.Application.Mapping;
using ChatShaker.Application.Users.Commands.Login;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ChatShaker.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => 
            {
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}
