using ByteSyncer.IdentityProvider;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddRazorPages();

services.AddSingleton(configuration)
        .AddDbSession(configuration, environment);

WebApplication application = builder.Build();

application.UseMigrations();

if (environment.IsDevelopment())
{
    application.UseDeveloperExceptionPage();
}
else
{
    application.UseExceptionHandler("/Error");
    application.UseHsts();
}

application.UseHttpsRedirection();
application.UseStaticFiles();

application.UseRouting();

application.UseAuthorization();

application.MapRazorPages();

application.Run();