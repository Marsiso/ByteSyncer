using System.Reflection;
using ByteSyncer.Domain.Constants;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddControllersWithViews();
services.AddRazorPages();

services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer("https://localhost:7197/");
        options.AddAudiences(AuthorizationDefaults.ApiResourceValue);

        options.AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
services.AddAuthorization();

services.AddEndpointsApiExplorer();

services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(SwaggerDefaults.VersionNaming, new OpenApiInfo
    {
        Title = "ByteSyncer's Resource Server",
        Version = SwaggerDefaults.VersionFriendlyNaming,
        Description = "ByteSyncer's resource server, which is also a RESTful web API and backend for a front-end web application running using a web assembly technology. The resource server is protected by the OpenID Connect identity provider and role/property-based authorization.",
        Contact = new OpenApiContact
        {
            Email = "olsak.marek@ooutlook.cz",
            Name = "Marek Olšák",
            Url = new Uri("https://www.linkedin.com/in/marek-ol%C5%A1%C3%A1k-1715b724a/")
        },
        TermsOfService = new Uri("https://example.com/terms"),
        License = new OpenApiLicense()
        {
          Name  = "MIT",
          Url = new Uri("https://en.wikipedia.org/wiki/MIT_License"),
        }
    });

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://localhost:7197/connect/authorize"),
                TokenUrl = new Uri("https://localhost:7197/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { AuthorizationDefaults.ApiScopeNaming, AuthorizationDefaults.ApiScopeFriendlyNaming }
                }
            },
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            Array.Empty<string>()
        }
    });

    // TODO: Enable XML comments for the OpenAPI Specification.
    /*string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);*/
});

WebApplication application = builder.Build();

if (environment.IsDevelopment())
{
    application.UseWebAssemblyDebugging();

    application.UseSwagger();
    application.UseSwaggerUI(options =>
    {
        options.OAuthAppName("ByteSyncer's Resource Server");
        options.OAuthClientId(AuthorizationDefaults.WebClientID);
        options.OAuthClientSecret(AuthorizationDefaults.DevelopmentClientSecretValue);
        options.OAuthUsePkce();
    });
}
else
{
    application.UseExceptionHandler("/Error");
    application.UseHsts();
}

application.UseHttpsRedirection();
application.UseBlazorFrameworkFiles();
application.UseStaticFiles();

application.UseRouting();

application.UseAuthentication();
application.UseAuthorization();

application.MapRazorPages();
application.MapControllers();
application.MapFallbackToFile("index.html");

application.Run();
