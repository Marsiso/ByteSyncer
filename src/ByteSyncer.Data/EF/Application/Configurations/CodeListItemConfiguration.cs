using ByteSyncer.Data.EF.Application.Configurations.Common;
using ByteSyncer.Domain.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ByteSyncer.Data.EF.Application.Configurations
{
    public class CodeListItemConfiguration : ChangeTrackingEntityConfiguration<CodeListItem>
    {
        public override void Configure(EntityTypeBuilder<CodeListItem> builder)
        {
            base.Configure(builder);

            builder.ToTable(Tables.CodeListItems, Schemas.Application);

            builder.HasIndex(codeListItem => codeListItem.CodeListID);

            builder.Property(codeListItem => codeListItem.Value)
                   .HasMaxLength(256);
        }
    }
}
