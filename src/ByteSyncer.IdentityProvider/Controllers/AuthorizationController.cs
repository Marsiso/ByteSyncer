using System.Collections.Immutable;
using System.Security.Claims;
using System.Web;
using ByteSyncer.Application.Services;
using ByteSyncer.Core.Application.Queries;
using ByteSyncer.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ByteSyncer.IdentityProvider.Controllers
{
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly AuthorizationProvider _authProvider;
        private readonly IMediator _mediator;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public AuthorizationController(
           AuthorizationProvider authProvider,
           IMediator mediator,
           IOpenIddictApplicationManager applicationManager,
           IOpenIddictAuthorizationManager authorizationManager,
           IOpenIddictScopeManager scopeManager)
        {
            _authProvider = authProvider;
            _mediator = mediator;
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        public async Task<IActionResult> Authorize()
        {
            OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            object application = await _applicationManager.FindByClientIdAsync(request.ClientId) ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            if (await _applicationManager.GetConsentTypeAsync(application) != ConsentTypes.Explicit)
            {
                Dictionary<string, string?> authenticationPropertiesItems = new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Only clients with explicit consent type are allowed."
                };

                AuthenticationProperties authenticationProperties = new AuthenticationProperties(authenticationPropertiesItems);

                return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties: authenticationProperties);
            }

            IDictionary<string, StringValues> parameters = _authProvider.ParseOAuthParameters(HttpContext, new List<string> { Parameters.Prompt });

            AuthenticateResult result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!_authProvider.IsAuthenticated(result, request))
            {
                return Challenge(properties: new AuthenticationProperties
                {
                    RedirectUri = _authProvider.BuildRedirectUrl(HttpContext.Request, parameters)
                }, new[] { CookieAuthenticationDefaults.AuthenticationScheme });
            }

            string? consentType = await _applicationManager.GetConsentTypeAsync(application);

            if (request.HasPrompt(Prompts.Login))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Challenge(properties: new AuthenticationProperties
                {
                    RedirectUri = _authProvider.BuildRedirectUrl(HttpContext.Request, parameters)
                }, new[] { CookieAuthenticationDefaults.AuthenticationScheme });
            }

            string consentClaim = result.Principal.GetClaim(AuthorizationDefaults.ConsentNaming);

            // It might be extended in a way that consent claim will contain list of allowed client IDs.
            if (consentClaim != AuthorizationDefaults.GrantAccessValue || request.HasPrompt(Prompts.Consent))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                string returnUrl = _authProvider.BuildRedirectUrl(HttpContext.Request, parameters);
                string returnUrlEncoded = HttpUtility.UrlEncode(returnUrl);
                string consentRedirectUrl = $"/Consent?returnUrl={returnUrlEncoded}";

                return Redirect(consentRedirectUrl);
            }

            string? userEmail = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

            ClaimsIdentity identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            ImmutableArray<string> roles = new List<string> { "user", "admin" }.ToImmutableArray();
            ImmutableArray<string> scopes = request.GetScopes();
            List<string> resources = await _scopeManager.ListResourcesAsync(scopes).ToListAsync();

            identity.SetClaim(Claims.Subject, userEmail)
                    .SetClaim(Claims.Email, userEmail)
                    .SetClaim(Claims.Name, userEmail)
                    .SetClaims(Claims.Role, roles)
                    .SetScopes(scopes)
                    .SetResources(resources)
                    .SetDestinations(claim => AuthorizationProvider.GetDestinations(identity, claim));

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
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

            string? userEmail = result.Principal.GetClaim(Claims.Subject);

            if (string.IsNullOrEmpty(userEmail))
            {
                Dictionary<string, string?> authenticationPropertiesItems = new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Cannot find user from the token."
                };

                AuthenticationProperties authenticationProperties = new AuthenticationProperties(authenticationPropertiesItems);

                return Forbid(
                   authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                   properties: authenticationProperties);
            }

            ClaimsIdentity identity = new ClaimsIdentity(result.Principal.Claims,
                  authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                  nameType: Claims.Name,
                  roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, userEmail)
                    .SetClaim(Claims.Email, userEmail)
                    .SetClaim(Claims.Name, userEmail)
                    .SetClaims(Claims.Role, new List<string> { "user", "admin" }.ToImmutableArray());

            identity.SetDestinations(claim => AuthorizationProvider.GetDestinations(identity, claim));

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

        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/connect/userinfo")]
        [HttpPost("~/connect/userinfo")]
        public async Task<IActionResult> Userinfo()
        {
            string? email = User.GetClaim(Claims.Subject);

            bool emailExists = false;
            if (!string.IsNullOrWhiteSpace(email))
            {
                EmailExistsQuery emailExistsQuery = new EmailExistsQuery(email);
                EmailExistsQueryResult emailExistsQueryResult = await _mediator.Send(emailExistsQuery);

                emailExists = emailExistsQueryResult.Exists;
            }

            if (!emailExists)
            {
                Dictionary<string, string?> authenticationPropertiesItems = new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is bound to an account that no longer exists."
                };

                AuthenticationProperties authenticationProperties = new AuthenticationProperties(authenticationPropertiesItems);

                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: authenticationProperties);
            }

            FindUserByEmailQuery findUserByEmailQuery = new FindUserByEmailQuery(email);
            FindUserByEmailQueryResult findUserByEmailQueryResult = await _mediator.Send(findUserByEmailQuery);

            if (findUserByEmailQueryResult.Result == FindUserByEmailQueryResultType.Succeded)
            {
                Dictionary<string, object> claims = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    [Claims.Subject] = email,
                };

                if (User.HasScope(Scopes.Email))
                {
                    claims[Claims.Email] = email;
                }

                return Ok(claims);
            }
            else
            {
                Dictionary<string, string?> authenticationPropertiesItems = new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ServerError,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "An unexpected error has occurred during the user info retrieval."
                };

                AuthenticationProperties authenticationProperties = new AuthenticationProperties(authenticationPropertiesItems);

                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: authenticationProperties);
            }
        }
    }
}
