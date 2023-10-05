using ByteSyncer.Data.EF.Application.Configurations.Common;
using ByteSyncer.Domain.Files.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ByteSyncer.Data.EF.Files.Configurations
{
    public class FolderConfiguration : ChangeTrackingEntityConfiguration<Folder>
    {
        public override void Configure(EntityTypeBuilder<Folder> builder)
        {
            base.Configure(builder);

            builder.ToTable(Tables.Folders, Schemas.Files);

            builder.Property(folder => folder.Name)
                   .HasMaxLength(256);

            builder.HasMany(folder => folder.Files)
                   .WithOne(file => file.Folder)
                   .HasForeignKey(file => file.FolderID)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(folder => folder.Parent)
                   .WithMany(folder => folder.Children)
                   .HasForeignKey(folder => folder.ParentID)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
