using System.Reflection;
using ByteSyncer.Application.Application.Mappings;
using ByteSyncer.Application.Application.Validators;
using ByteSyncer.Application.Options;
using ByteSyncer.Application.Services;
using ByteSyncer.Core.Application.Commands;
using ByteSyncer.Data.EF;
using ByteSyncer.Domain.Contracts;
using ByteSyncer.IdentityProvider;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using static OpenIddict.Abstractions.OpenIddictConstants;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddControllers();
services.AddRazorPages();

services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Index";
        });

services.AddOpenIddict()
        .AddCore(options =>
        {
            options.UseEntityFrameworkCore()
                   .UseDbContext<DataContext>();
        })
        .AddServer(options =>
        {
            options.SetAuthorizationEndpointUris("connect/authorize")
                   .SetLogoutEndpointUris("connect/logout")
                   .SetTokenEndpointUris("connect/token")
                   .SetUserinfoEndpointUris("connect/userinfo");

            options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

            options.AllowAuthorizationCodeFlow();

            options.AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

            options.AddDevelopmentEncryptionCertificate()
                   .AddDevelopmentSigningCertificate();

            options.UseAspNetCore()
                   .EnableAuthorizationEndpointPassthrough()
                   .EnableLogoutEndpointPassthrough()
                   .EnableTokenEndpointPassthrough()
                   .EnableUserinfoEndpointPassthrough();
        });

services.AddOptions<PasswordProtectorOptions>()
        .Configure(options =>
        {
            options.Pepper = "SecurePasswordPepper";
        })
        .ValidateDataAnnotations()
        .ValidateOnStart();

services.AddSingleton(configuration)
        .AddDbSession(configuration, environment)
        .AddAutoMapper(typeof(UserProfile))
        .AddMediatR(options => options.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(RegisterCommand))))
        .AddValidatorsFromAssembly(Assembly.GetAssembly(typeof(RegisterCommandValidator)))
        .AddSingleton<IPasswordProtector, PasswordProtector>()
        .AddTransient<AuthorizationProvider>()
        .AddTransient<ClientsSeeder>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7183")
              .AllowAnyHeader();
    });
});

WebApplication application = builder.Build();

application.UseMigrations();

using (IServiceScope scope = application.Services.CreateScope())
{
    ClientsSeeder seeder = scope.ServiceProvider.GetRequiredService<ClientsSeeder>();

    seeder.AddOidcDebuggerClient().GetAwaiter().GetResult();
    seeder.AddWebClients().GetAwaiter().GetResult();
    seeder.AddScopes().GetAwaiter().GetResult();
}

if (environment.IsDevelopment())
{
    application.UseDeveloperExceptionPage()
               .UseSwagger()
               .UseSwaggerUI();
}
else
{
    application.UseExceptionHandler("/Error")
               .UseHsts();
}

application.UseHttpsRedirection()
           .UseCors()
           .UseStaticFiles();

application.UseRouting()
           .UseAuthentication()
           .UseAuthorization();

application.MapControllers();
application.MapRazorPages();

application.Run();
