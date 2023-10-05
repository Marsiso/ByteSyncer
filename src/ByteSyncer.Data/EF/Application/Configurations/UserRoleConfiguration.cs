using ByteSyncer.Data.EF.Application.Configurations.Common;
using ByteSyncer.Domain.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ByteSyncer.Data.EF.Application.Configurations
{
    public class UserRoleConfiguration : ChangeTrackingEntityConfiguration<UserRole>
    {
        public override void Configure(EntityTypeBuilder<UserRole> builder)
        {
            base.Configure(builder);

            builder.ToTable(Tables.UserRoles, Schemas.Application);

            builder.HasIndex(userRole => userRole.UserID);
            builder.HasIndex(userRole => userRole.RoleID);
        }
    }
}
