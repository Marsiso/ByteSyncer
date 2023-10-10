using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using OpenIddict.Abstractions;

namespace ByteSyncer.Application.Services
{
    public class IdentityResolver
    {
        public IDictionary<string, StringValues> ParseOAuthParameters(HttpContext httpContext, List<string>? excluding = null)
        {
            excluding ??= new List<string>();

            Dictionary<string, StringValues> parameters = httpContext.Request.HasFormContentType
                ? httpContext.Request.Form
                    .Where(keyValuePair => !excluding.Contains(keyValuePair.Key))
                    .ToDictionary(
                        keyValuePair => keyValuePair.Key,
                        keyValuePair => keyValuePair.Value)
                : httpContext.Request.Query
                    .Where(keyValuePair => !excluding.Contains(keyValuePair.Key))
                    .ToDictionary(
                        keyValuePair => keyValuePair.Key,
                        keyValuePair => keyValuePair.Value);

            return parameters;
        }

        public string BuildRedirectUrl(HttpRequest request, IDictionary<string, StringValues> oAuthParameters)
        {
            string url = request.PathBase + request.Path + QueryString.Create(oAuthParameters);
            return url;
        }

        public bool IsAuthenticated(AuthenticateResult authenticateResult, OpenIddictRequest request)
        {
            if (!authenticateResult.Succeeded)
            {
                return false;
            }

            if (request.MaxAge.HasValue && authenticateResult.Properties != null)
            {
                TimeSpan maxAgeSeconds = TimeSpan.FromSeconds(request.MaxAge.Value);

                bool expired = !authenticateResult.Properties.IssuedUtc.HasValue || DateTimeOffset.UtcNow - authenticateResult.Properties.IssuedUtc > maxAgeSeconds;

                if (expired)
                {
                    return false;
                }
            }

            return true;
        }

        public static List<string> GetDestinations(ClaimsIdentity identity, Claim claim)
        {
            List<string> destinations = new List<string>();

            if (claim.Type is OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.Email)
            {
                destinations.Add(OpenIddictConstants.Destinations.AccessToken);
            }

            return destinations;
        }
    }
}
