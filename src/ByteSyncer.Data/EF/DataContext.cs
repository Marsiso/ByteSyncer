using System.Reflection;
using ByteSyncer.Domain.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ByteSyncer.Data.EF
{
    public class DataContext : DbContext
    {
        private readonly ISaveChangesInterceptor _interceptor;

        public DataContext(DbContextOptions<DataContext> options, ISaveChangesInterceptor interceptor) : base(options)
        {
            _interceptor = interceptor;
        }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_interceptor)
                          .UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(DataContext)));
        }
    }
}
