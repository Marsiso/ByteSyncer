using System.Reflection;
using ByteSyncer.Application.Application.Mappings;
using ByteSyncer.Application.Application.Validators;
using ByteSyncer.Application.Options;
using ByteSyncer.Application.Services;
using ByteSyncer.Core.Application.Commands;
using ByteSyncer.Domain.Contracts;
using ByteSyncer.IdentityProvider;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddRazorPages();

services.AddAuthentication()
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

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
        .AddSingleton<IPasswordProtector, PasswordProtector>();

WebApplication application = builder.Build();

application.UseMigrations();

if (environment.IsDevelopment())
{
    application.UseDeveloperExceptionPage();
}
else
{
    application.UseExceptionHandler("/Error")
               .UseHsts();
}

application.UseHttpsRedirection()
           .UseStaticFiles();

application.UseRouting()
           .UseAuthentication()
           .UseAuthorization();

application.MapRazorPages();

application.Run();
