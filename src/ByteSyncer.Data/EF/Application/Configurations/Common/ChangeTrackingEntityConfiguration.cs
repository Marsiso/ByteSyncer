using ByteSyncer.Domain.Application.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ByteSyncer.Data.EF.Application.Configurations.Common
{
    public class ChangeTrackingEntityConfiguration<TEntity> : EntityBaseConfiguration<TEntity> where TEntity : ChangeTrackingEntity
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);

            builder.HasOne(entity => entity.UserCreatedBy)
                   .WithMany()
                   .HasForeignKey(entity => entity.CreatedBy)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(entity => entity.UserUpdatedBy)
                   .WithMany()
                   .HasForeignKey(entity => entity.UpdatedBy)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
