using ByteSyncer.Data.EF.Application.Configurations.Common;
using ByteSyncer.Domain.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ByteSyncer.Data.EF.Application.Configurations
{
    public class CodeListConfiguration : ChangeTrackingEntityConfiguration<CodeList>
    {
        public override void Configure(EntityTypeBuilder<CodeList> builder)
        {
            base.Configure(builder);

            builder.ToTable(Tables.CodeLists, Schemas.Application);

            builder.Property(user => user.Name)
                   .HasMaxLength(512);

            builder.HasMany(codeList => codeList.CodeListItems)
                   .WithOne()
                   .HasForeignKey(codeListItem => codeListItem.CodeListID)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
