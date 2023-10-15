using ByteSyncer.Data.EF;
using ByteSyncer.Domain.Constants;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ByteSyncer.Application.Services
{
    public class ClientsSeeder
    {
        private readonly IServiceProvider _serviceProvider;

        public ClientsSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task AddScopes()
        {

            await using AsyncServiceScope serviceScope = _serviceProvider.CreateAsyncScope();

            IOpenIddictScopeManager oidcScopeManager = serviceScope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            object? resourceApiScope = await oidcScopeManager.FindByNameAsync(AuthorizationDefaults.ApiScopeNaming);

            if (resourceApiScope is not null)
            {
                await oidcScopeManager.DeleteAsync(resourceApiScope);
            }

            OpenIddictScopeDescriptor resourceApiScopeDescriptor = new OpenIddictScopeDescriptor
            {
                DisplayName = AuthorizationDefaults.ApiScopeFriendlyNaming,
                Name = AuthorizationDefaults.ApiScopeNaming,
                Resources = { AuthorizationDefaults.ApiResourceValue }
            };

            await oidcScopeManager.CreateAsync(resourceApiScopeDescriptor);
        }

        public async Task AddWebClients()
        {

            await using AsyncServiceScope serviceScope = _serviceProvider.CreateAsyncScope();

            DataContext databaseContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();

            await databaseContext.Database.EnsureCreatedAsync();

            IOpenIddictApplicationManager oidcApplicationManager = serviceScope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            object? client = await oidcApplicationManager.FindByClientIdAsync(AuthorizationDefaults.WebClientID);

            if (client is not null)
            {
                await oidcApplicationManager.DeleteAsync(client);
            }

            var oidcWebApplicationDescriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = AuthorizationDefaults.WebClientID,
                ClientSecret = AuthorizationDefaults.DevelopmentClientSecretValue,
                ConsentType = ConsentTypes.Explicit,
                DisplayName = "Web client application",
                RedirectUris = { new Uri("https://localhost:7061/swagger/oauth2-redirect.html") },
                PostLogoutRedirectUris = { new Uri("https://localhost:7061/swagger") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    $"{Permissions.Prefixes.Scope}{AuthorizationDefaults.ApiScopeNaming}"
                },
                Requirements = { Requirements.Features.ProofKeyForCodeExchange }
            };

            await oidcApplicationManager.CreateAsync(oidcWebApplicationDescriptor);
        }

        public async Task AddOidcDebuggerClient()
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();

            DataContext context = scope.ServiceProvider.GetRequiredService<DataContext>();

            await context.Database.EnsureCreatedAsync();

            IOpenIddictApplicationManager manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            object? client = await manager.FindByClientIdAsync(AuthorizationDefaults.OidcDebuggerClientID);
            if (client is not null)
            {
                await manager.DeleteAsync(client);
            }

            OpenIddictApplicationDescriptor oidcDebuggerApplicationDescriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = AuthorizationDefaults.OidcDebuggerClientID,
                ClientSecret = AuthorizationDefaults.DevelopmentClientSecretValue,
                ConsentType = ConsentTypes.Explicit,
                DisplayName = "OpenID Connect debugger client application",
                RedirectUris = { new Uri("https://oidcdebugger.com/debug") },
                PostLogoutRedirectUris = { new Uri("https://oauth.pstmn.io/v1/callback") },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    $"{Permissions.Prefixes.Scope}{AuthorizationDefaults.ApiScopeNaming}"
                },
                Requirements = { Requirements.Features.ProofKeyForCodeExchange }
            };

            await manager.CreateAsync(oidcDebuggerApplicationDescriptor);
        }
    }
}
