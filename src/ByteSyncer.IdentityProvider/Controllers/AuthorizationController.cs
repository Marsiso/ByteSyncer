using System.Collections.Immutable;
using System.Security.Claims;
using System.Web;
using ByteSyncer.Application.Services;
using ByteSyncer.Domain.Constants;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ByteSyncer.IdentityProvider.Controllers
{
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;
        private readonly OAuthProvider _identityResolver;

        public AuthorizationController(
           IOpenIddictApplicationManager applicationManager,
           IOpenIddictAuthorizationManager authorizationManager,
           IOpenIddictScopeManager scopeManager,
           OAuthProvider identityResolver)
        {
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
            _identityResolver = identityResolver;
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        public async Task<IActionResult> Authorize()
        {
            OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            IDictionary<string, Microsoft.Extensions.Primitives.StringValues> parameters = _identityResolver.ParseOAuthParameters(HttpContext, new List<string> { Parameters.Prompt });

            AuthenticateResult result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!_identityResolver.IsAuthenticated(result, request))
            {
                return Challenge(properties: new AuthenticationProperties
                {
                    RedirectUri = _identityResolver.BuildRedirectUrl(HttpContext.Request, parameters)
                }, new[] { CookieAuthenticationDefaults.AuthenticationScheme });
            }

            object application = await _applicationManager.FindByClientIdAsync(request.ClientId) ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            string? consentType = await _applicationManager.GetConsentTypeAsync(application);

            // We just ignore other consent types, because they are not compliant with OAuth and OpenId Connect docs, that state that Resource Owner should grant the Client access
            // you might also support Implicit ConsentType - where you do not require consent screen even if `prompt=consent` provided. In that case just drop this if.
            // you might want to support External ConsentType - where you need to get created authorization first by admin to be able to log in.
            if (consentType != ConsentTypes.Explicit)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Only explicit consent clients are supported."
                    }));
            }

            string consentClaim = result.Principal.GetClaim(AuthorizationDefaults.ConsentNaming);

            // It might be extended in a way that consent claim will contain list of allowed client IDs.
            if (consentClaim != AuthorizationDefaults.GrantAccessValue)
            {
                string returnUrl = HttpUtility.UrlEncode(_identityResolver.BuildRedirectUrl(HttpContext.Request, parameters));
                string consentRedirectUrl = $"/Consent?returnUrl={returnUrl}";

                return Redirect(consentRedirectUrl);
            }

            string userId = result.Principal.FindFirst(ClaimTypes.Email)!.Value;

            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, userId)
                    .SetClaim(Claims.Email, userId)
                    .SetClaim(Claims.Name, userId)
                    .SetClaims(Claims.Role, new List<string> { "user", "admin" }.ToImmutableArray());

            identity.SetScopes(request.GetScopes());
            identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

            identity.SetDestinations(claim => OAuthProvider.GetDestinations(identity, claim));

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            {
                throw new InvalidOperationException("The specified grant type is not supported.");
            }

            AuthenticateResult result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            string? userID = result.Principal.GetClaim(Claims.Subject);

            if (string.IsNullOrEmpty(userID))
            {
                return Forbid(
                   authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                   properties: new AuthenticationProperties(new Dictionary<string, string>
                   {
                       [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                       [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Cannot find user from the token."
                   }));
            }

            var identity = new ClaimsIdentity(result.Principal.Claims,
                  authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                  nameType: Claims.Name,
                  roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, userID)
                    .SetClaim(Claims.Email, userID)
                    .SetClaim(Claims.Name, userID)
                    .SetClaims(Claims.Role, new List<string> { "user", "admin" }.ToImmutableArray());

            identity.SetDestinations(claim => OAuthProvider.GetDestinations(identity, claim));

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("~/connect/logout")]
        public async Task<IActionResult> LogoutPost()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return SignOut(
                  authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                  properties: new AuthenticationProperties
                  {
                      RedirectUri = "/"
                  });
        }
    }
}
