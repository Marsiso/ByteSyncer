using ByteSyncer.Data.EF.Application.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using File = ByteSyncer.Domain.Files.Models.File;

namespace ByteSyncer.Data.EF.Files.Configurations
{
    public class FileConfiguration : ChangeTrackingEntityConfiguration<File>
    {
        public override void Configure(EntityTypeBuilder<Domain.Files.Models.File> builder)
        {
            base.Configure(builder);

            builder.ToTable(Tables.Files, Schemas.Files);

            builder.HasIndex(file => file.FolderID);

            builder.Property(file => file.SafeName)
                   .HasMaxLength(256);

            builder.Property(file => file.UnsafeName)
                   .HasMaxLength(256);

            builder.Property(file => file.Location)
                   .HasMaxLength(2048);
        }
    }
}
