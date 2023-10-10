using ByteSyncer.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ByteSyncer.IdentityProvider
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddDbSession(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddTransient<ISaveChangesInterceptor, AuditTrailInterceptor>();

            services.AddDbContext<DataContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                       .UseOpenIddict()
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
                       .EnableDetailedErrors(environment.IsDevelopment())
                       .EnableSensitiveDataLogging(environment.IsDevelopment());
            });

            return services;
        }

        public static WebApplication UseMigrations(this WebApplication application)
        {
            IServiceProvider services = application.Services;
            IWebHostEnvironment environment = application.Environment;

            using IServiceScope scope = services.CreateScope();

            DataContext context = scope.ServiceProvider.GetRequiredService<DataContext>();

            if (environment.IsDevelopment())
            {
                context.Database.EnsureDeleted();
            }

            context.Database.EnsureCreated();

            return application;
        }
    }
}
