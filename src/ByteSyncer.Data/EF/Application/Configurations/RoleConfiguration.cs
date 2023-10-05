using ByteSyncer.Data.EF.Application.Configurations.Common;
using ByteSyncer.Domain.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ByteSyncer.Data.EF.Application.Configurations
{
    public class RoleConfiguration : ChangeTrackingEntityConfiguration<Role>
    {
        public override void Configure(EntityTypeBuilder<Role> builder)
        {
            base.Configure(builder);

            builder.ToTable(Tables.Roles, Schemas.Application);

            builder.Property(role => role.Name)
                   .HasMaxLength(256)
                   .IsUnicode();

            builder.Property(role => role.Permissions)
                   .HasMaxLength(256)
                   .IsUnicode();

            builder.HasMany(role => role.UserRoles)
                   .WithOne(userRole => userRole.Role)
                   .HasForeignKey(userRole => userRole.RoleID)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
