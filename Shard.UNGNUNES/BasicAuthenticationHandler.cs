using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Shard.UNGNUNES
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder, 
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if(!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out AuthenticationHeaderValue headerValue))
            {
                return AuthenticateResult.NoResult();
            }
            
            byte[] headerValueBytes = Convert.FromBase64String(headerValue.Parameter);
            string userAndPassword = Encoding.UTF8.GetString(headerValueBytes);
            string user = userAndPassword.Split(":")[0];
            string password = userAndPassword.Split(":")[1];
            
            if (user.Equals("admin") && password == "password")
            {
                var claims = new[] {new Claim(ClaimTypes.Role, "admin")};
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }

            if (user.StartsWith("shard-") && password == "caramba")
            {
                var claims = new[] {new Claim(ClaimTypes.Role, "shard")};
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            
            
            return null;
        }
    }
}