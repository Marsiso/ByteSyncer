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

            builder.Property(user => user.Email)
                   .HasMaxLength(256);

            builder.Property(user => user.GivenName)
                   .HasMaxLength(256)
                   .IsUnicode();

            builder.Property(user => user.FamilyName)
                   .HasMaxLength(256)
                   .IsUnicode();

            builder.Property(user => user.Password)
                   .HasMaxLength(512);

            builder.Property(user => user.PasswordSalt)
                   .HasMaxLength(512);
        }
    }
}
