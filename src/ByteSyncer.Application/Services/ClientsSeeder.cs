using ByteSyncer.Data.EF;
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

            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();

            IOpenIddictScopeManager manager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            object? apiScope = await manager.FindByNameAsync("api1");

            if (apiScope is not null)
            {
                await manager.DeleteAsync(apiScope);
            }

            await manager.CreateAsync(new OpenIddictScopeDescriptor
            {
                DisplayName = "Api scope",
                Name = "api1",
                Resources =
                {
                   "resource_server_1"
                }
            });
        }

        public async Task AddWebClients()
        {

            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();

            DataContext context = scope.ServiceProvider.GetRequiredService<DataContext>();

            await context.Database.EnsureCreatedAsync();

            IOpenIddictApplicationManager manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            object? client = await manager.FindByClientIdAsync("web-client");

            if (client is not null)
            {
                await manager.DeleteAsync(client);
            }

            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "web-client",
                ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                ConsentType = ConsentTypes.Explicit,
                DisplayName = "Postman client application",
                RedirectUris =
                {
                   new Uri("https://localhost:7183/swagger/oauth2-redirect.html")
                },
                PostLogoutRedirectUris =
                {
                   new Uri("https://localhost:7183/swagger")
                },
                Permissions =
                {
                   Permissions.Endpoints.Authorization,
                   Permissions.Endpoints.Logout,
                   Permissions.Endpoints.Token,
                   Permissions.GrantTypes.AuthorizationCode,
                   Permissions.ResponseTypes.Code,
                   Permissions.Scopes.Email,
                   Permissions.Scopes.Profile,
                   Permissions.Scopes.Roles,
                   $"{Permissions.Prefixes.Scope}api1"
                },
                //Requirements =
                //{
                //    Requirements.Features.ProofKeyForCodeExchange
                //}
            });
        }

        public async Task AddOIDCDebuggerClient()
        {
            await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();

            DataContext context = scope.ServiceProvider.GetRequiredService<DataContext>();

            await context.Database.EnsureCreatedAsync();

            IOpenIddictApplicationManager manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            object? client = await manager.FindByClientIdAsync("oidc-debugger");
            if (client is not null)
            {
                await manager.DeleteAsync(client);
            }

            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "oidc-debugger",
                ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F3654",
                ConsentType = ConsentTypes.Explicit,
                DisplayName = "Postman client application",
                RedirectUris =
                 {
                    new Uri("https://oidcdebugger.com/debug")
                 },
                PostLogoutRedirectUris =
                 {
                    new Uri("https://oauth.pstmn.io/v1/callback")
                 },
                Permissions =
                 {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    $"{Permissions.Prefixes.Scope}api1"
                 },
                //Requirements =
                //{
                //    Requirements.Features.ProofKeyForCodeExchange
                //}
            });
        }
    }
}
