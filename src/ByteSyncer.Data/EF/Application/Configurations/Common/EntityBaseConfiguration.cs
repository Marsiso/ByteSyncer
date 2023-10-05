using ByteSyncer.Domain.Application.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ByteSyncer.Data.EF.Application.Configurations.Common
{
    public class EntityBaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : EntityBase
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasKey(entity => entity.ID);

            builder.HasIndex(entity => entity.IsActive);

            builder.HasQueryFilter(entity => entity.IsActive);
        }
    }
}
