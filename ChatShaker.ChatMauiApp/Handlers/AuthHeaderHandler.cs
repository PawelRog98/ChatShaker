using ChatShaker.ChatMauiApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.Handlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IAuthTokenProvider _authTokenProvider;

        public AuthHeaderHandler(IAuthTokenProvider authTokenProvider)
        {
            _authTokenProvider = authTokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _authTokenProvider.GetAuthToken();

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

}
