using ByteSyncer.Domain.Application.Models;
using ByteSyncer.Domain.Files.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;
using File = ByteSyncer.Domain.Files.Models.File;

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
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<CodeList> CodeLists { get; set; }
        public DbSet<CodeListItem> CodeListItems { get; set; }

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
