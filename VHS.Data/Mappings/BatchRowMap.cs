using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;
public class BatchRowMap : IEntityTypeConfiguration<BatchRow>
{
    public void Configure(EntityTypeBuilder<BatchRow> builder)
    {
        builder.ToTable("BatchRows");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.Batch)
               .WithMany(bp => bp.BatchRows)
               .HasForeignKey(x => x.BatchId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(x => x.FloorId).IsRequired(false);
        builder.Property(x => x.RackId).IsRequired(false);
        builder.Property(x => x.LayerId).IsRequired(false);
        builder.Property(x => x.AddedDateTime).IsRequired();
        builder.Property(b => b.DeletedDateTime).IsRequired(false);
    }
}
