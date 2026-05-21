using DotNetEnv;

namespace ChatShaker.Api.Configuration
{
    public static class ConfigurationExtensions
    {
        private static readonly Dictionary<string, string> _settingDictionary = new Dictionary<string, string>
        {
            { "ConnectionStrings:DefaultConnection", Environment.GetEnvironmentVariable("SQL_CONN") },
            { "ConnectionStrings:RedisConnection", Environment.GetEnvironmentVariable("REDIS_CONN") },
            { "ApiUrls:MainUrl", Environment.GetEnvironmentVariable("API_URL") },
            { "JWTAuth:JwtKey", Environment.GetEnvironmentVariable("JWT_KEY") },
            { "JWTAuth:JwtIssuer", Environment.GetEnvironmentVariable("JWT_ISSUER") },
            { "JWTAuth:JwtExpireDays", Environment.GetEnvironmentVariable("JWT_EXPIRE_DAYS") },
            { "FileStorage:RootPath", Environment.GetEnvironmentVariable("FILES_STORAGE") },
            { "Hangfire:Dashboard:Username", Environment.GetEnvironmentVariable("HANGFIRE_USERNAME") },
            { "Hangfire:Dashboard:Password", Environment.GetEnvironmentVariable("HANGFIRE_PASSWORD") },
            { "EmailConfiguration:Host",  Environment.GetEnvironmentVariable("EMAIL_HOST") },
            { "EmailConfiguration:Port",  Environment.GetEnvironmentVariable("EMAIL_PORT") },
            { "EmailConfiguration:FromEmail",  Environment.GetEnvironmentVariable("EMAIL") },
            { "EmailConfiguration:Username",  Environment.GetEnvironmentVariable("EMAIL_USERNAME") },
            { "EmailConfiguration:Password",  Environment.GetEnvironmentVariable("EMAIL_PASSWORD") },
            { "EmailConfiguration:FromName",  Environment.GetEnvironmentVariable("EMAIL_FROMNAME") },
        };

        public static IConfigurationBuilder AddMainConfiguration(this IConfigurationBuilder builder)
        {
            Env.Load();

            return builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddInMemoryCollection(_settingDictionary)
                .AddEnvironmentVariables();
        }
    }
}
