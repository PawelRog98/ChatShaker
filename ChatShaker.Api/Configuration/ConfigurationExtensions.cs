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
            { "EmailConfiguration:EmailSmtp", Environment.GetEnvironmentVariable("EMAIL_SMTP") },
            { "EmailConfiguration:Email", Environment.GetEnvironmentVariable("EMAIL") },
            { "EmailConfiguration:EmailPassword", Environment.GetEnvironmentVariable("EMAIL_PASSWORD") },
            { "EmailConfiguration:Port", Environment.GetEnvironmentVariable("EMAIL_PORT") },
            { "FileStorage:RootPath", Environment.GetEnvironmentVariable("FILES_STORAGE") },
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
