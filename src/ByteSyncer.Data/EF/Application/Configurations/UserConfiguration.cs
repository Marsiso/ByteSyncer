using ByteSyncer.Data.EF.Application.Configurations.Common;
using ByteSyncer.Domain.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ByteSyncer.Data.EF.Application.Configurations
{
    public class UserConfiguration : ChangeTrackingEntityConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);

            builder.ToTable(Tables.Users, Schemas.Application);

            builder.HasIndex(user => user.Email)
                   .IsUnique();

            builder.HasIndex(user => user.RootFolderID)
                   .IsUnique();

            builder.Property(user => user.Email)
                   .HasMaxLength(256);

            builder.Property(user => user.GivenName)
                   .HasMaxLength(256)
                   .IsUnicode();

            builder.Property(user => user.FamilyName)
                   .HasMaxLength(256)
                   .IsUnicode();

            builder.Property(user => user.PasswordHash)
                   .HasMaxLength(1024);

            builder.Property(user => user.SecurityStamp)
                   .HasMaxLength(512);

            builder.HasOne(user => user.RootFolder)
                   .WithOne()
                   .HasForeignKey<User>(user => user.RootFolderID)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(role => role.UserRoles)
                   .WithOne(userRole => userRole.User)
                   .HasForeignKey(userRole => userRole.UserID)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
