using System.Collections.Immutable;
using System.Net.Mime;
using System.Security.Claims;
using System.Web;
using ByteSyncer.Application.Services;
using ByteSyncer.Core.CQRS.Application.Queries;
using ByteSyncer.Domain.Application.Models;
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
        private readonly AuthorizationProvider _authorizationProvider;
        private readonly IMediator _mediator;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public AuthorizationController(
           AuthorizationProvider authorizationProvider,
           IMediator mediator,
           IOpenIddictApplicationManager applicationManager,
           IOpenIddictAuthorizationManager authorizationManager,
           IOpenIddictScopeManager scopeManager)
        {
            _authorizationProvider = authorizationProvider;
            _mediator = mediator;
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
        {
            OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            object application = await _applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken) ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

            if (await _applicationManager.GetConsentTypeAsync(application, cancellationToken) != ConsentTypes.Explicit)
            {
                Dictionary<string, string?> authenticationPropertyDictionary = new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidClient,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Only clients with explicit consent type are allowed."
                };

                AuthenticationProperties authenticationProperties = new AuthenticationProperties(authenticationPropertyDictionary);

                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: authenticationProperties);
            }

            IDictionary<string, StringValues> parameters = _authorizationProvider.ParseOAuth2Parameters(HttpContext, new List<string> { Parameters.Prompt });

            AuthenticateResult result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!_authorizationProvider.IsAuthenticated(result, request))
            {
                return Challenge(properties: new AuthenticationProperties
                {
                    RedirectUri = _authorizationProvider.BuildRedirectUrl(HttpContext.Request, parameters)
                }, new[] { CookieAuthenticationDefaults.AuthenticationScheme });
            }

            string? consentType = await _applicationManager.GetConsentTypeAsync(application, cancellationToken);

            if (request.HasPrompt(Prompts.Login))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Challenge(properties: new AuthenticationProperties
                {
                    RedirectUri = _authorizationProvider.BuildRedirectUrl(HttpContext.Request, parameters)
                }, new[] { CookieAuthenticationDefaults.AuthenticationScheme });
            }

            string consentClaim = result.Principal.GetClaim(AuthorizationDefaults.ConsentNaming);

            if (consentClaim != AuthorizationDefaults.GrantAccessValue || request.HasPrompt(Prompts.Consent))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                string returnUrl = _authorizationProvider.BuildRedirectUrl(HttpContext.Request, parameters);
                string returnUrlEncoded = HttpUtility.UrlEncode(returnUrl);
                string consentRedirectUrl = $"/Consent?returnUrl={returnUrlEncoded}";

                return Redirect(consentRedirectUrl);
            }

            string? email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;

            FindUserUsingEmailQuery query = new FindUserUsingEmailQuery(email);
            FindUserUsingEmailQueryResult queryResult = await _mediator.Send(query, cancellationToken);

            if (queryResult.ResultType == FindUserUsingEmailQueryResultType.UserNotFound)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Challenge(properties: new AuthenticationProperties
                {
                    RedirectUri = _authorizationProvider.BuildRedirectUrl(HttpContext.Request, parameters)
                }, new[] { CookieAuthenticationDefaults.AuthenticationScheme });
            }

            if (queryResult.ResultType != FindUserUsingEmailQueryResultType.UserFound)
            {
                Dictionary<string, string?> authenticationPropertyDictionary = new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ServerError,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "An unexpected error has occurred during the user info retrieval."
                };

                AuthenticationProperties authenticationProperties = new AuthenticationProperties(authenticationPropertyDictionary);

                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: authenticationProperties);
            }

            ClaimsIdentity identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            ImmutableArray<string> scopes = request.GetScopes();
            List<string> resources = await _scopeManager.ListResourcesAsync(scopes, cancellationToken)
                                                        .ToListAsync(cancellationToken: cancellationToken);

            User user = queryResult.GetResult();

            ImmutableArray<string> roles = user.UserRoles.Select(userRole => userRole.Role.Name)
                                                         .ToImmutableArray();

            identity.SetClaim(Claims.Subject, user.ID)
                    .SetClaim(Claims.Email, user.Email)
                    .SetClaims(Claims.Role, roles)
                    .SetScopes(scopes)
                    .SetResources(resources)
                    .SetDestinations(claim => AuthorizationProvider.GetDestinations(identity, claim));

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("~/connect/token")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Exchange(CancellationToken cancellationToken)
        {
            OpenIddictRequest request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            {
                throw new InvalidOperationException("The specified grant type is not supported.");
            }

            AuthenticateResult result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            string? email = result.Principal.GetClaim(Claims.Email);

            FindUserUsingEmailQuery query = new FindUserUsingEmailQuery(email);
            FindUserUsingEmailQueryResult queryResult = await _mediator.Send(query, cancellationToken);

            if (queryResult.ResultType == FindUserUsingEmailQueryResultType.UserFound)
            {
                User user = queryResult.GetResult();

                ClaimsIdentity identity = new ClaimsIdentity(result.Principal.Claims,
                      authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                      nameType: Claims.Name,
                      roleType: Claims.Role);

                ImmutableArray<string> roles = user.UserRoles.Select(userRole => userRole.Role.Name)
                                                             .ToImmutableArray();

                identity.SetClaim(Claims.Subject, user.ID)
                        .SetClaim(Claims.Email, user.Email)
                        .SetClaim(Claims.Name, $"{user.GivenName} {user.FamilyName}")
                        .SetClaims(Claims.Role, roles);

                identity.SetDestinations(claim => AuthorizationProvider.GetDestinations(identity, claim));

                ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

                return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            else if (queryResult.ResultType == FindUserUsingEmailQueryResultType.UserNotFound)
            {
                Dictionary<string, string?> authenticationPropertyDictionary = new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Cannot find user from the token."
                };

                AuthenticationProperties authenticationProperties = new AuthenticationProperties(authenticationPropertyDictionary);

                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: authenticationProperties);
            }
            else
            {
                Dictionary<string, string?> authenticationPropertyDictionary = new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ServerError,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "An unexpected error has occurred during the user info retrieval."
                };

                AuthenticationProperties authenticationProperties = new AuthenticationProperties(authenticationPropertyDictionary);

                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: authenticationProperties);
            }
        }

        [HttpPost("~/connect/logout")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Userinfo(CancellationToken cancellationToken)
        {
            string? email = User.GetClaim(Claims.Email);

            FindUserUsingEmailQuery query = new FindUserUsingEmailQuery(email);
            FindUserUsingEmailQueryResult queryResult = await _mediator.Send(query, cancellationToken);

            if (queryResult.ResultType == FindUserUsingEmailQueryResultType.UserFound)
            {
                User user = queryResult.GetResult();

                Dictionary<string, object> claims = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    [Claims.Subject] = user.ID,
                };

                if (User.HasScope(Scopes.Email))
                {
                    claims[Claims.Email] = user.Email;
                }

                if (User.HasScope(Scopes.Profile))
                {
                    claims[Claims.GivenName] = user.GivenName;
                    claims[Claims.FamilyName] = user.FamilyName;
                }

                if (User.HasScope(Scopes.Roles))
                {
                    ImmutableArray<string> roleNames = user.UserRoles.Select(userRole => userRole.Role.Name)
                                                                     .ToImmutableArray();

                    claims[Claims.FamilyName] = roleNames;
                }

                return Ok(claims);
            }
            else if (queryResult.ResultType == FindUserUsingEmailQueryResultType.UserNotFound)
            {
                Dictionary<string, string?> authenticationPropertyDictionary = new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The specified access token is bound to an account that no longer exists."
                };

                AuthenticationProperties authenticationProperties = new AuthenticationProperties(authenticationPropertyDictionary);

                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: authenticationProperties);
            }
            else
            {
                Dictionary<string, string?> authenticationPropertyDictionary = new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ServerError,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "An unexpected error has occurred during the user info retrieval."
                };

                AuthenticationProperties authenticationProperties = new AuthenticationProperties(authenticationPropertyDictionary);

                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: authenticationProperties);
            }
        }
    }
}
