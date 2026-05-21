using Hangfire.Dashboard;
using Hangfire.Dashboard.BasicAuthorization;

namespace ChatShaker.Api.Filters.Hangfire;

public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    private readonly string _username;
    private readonly string _password;

    public HangfireAuthFilter(string username, string password)
    {
        _username = username;
        _password = password;
    }

    public bool Authorize(DashboardContext context)
    {
        var filter = new BasicAuthAuthorizationFilter(
            new BasicAuthAuthorizationFilterOptions
            {
                RequireSsl = false,
                SslRedirect = false,
                LoginCaseSensitive = true,
                Users = new[]
                {
                    new BasicAuthAuthorizationUser
                    {
                        Login = _username,
                        PasswordClear = _password
                    }
                }
            });

        return filter.Authorize(context);
    }
}