using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class BatchMap : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.ToTable("Batches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedOnAdd();

        builder.HasOne(b => b.Farm)
               .WithMany()
               .HasForeignKey(b => b.FarmId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(b => b.BatchPlan)
               .WithMany(bc => bc.Batches)
               .HasForeignKey(b => b.BatchPlanId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(br => br.BatchRows)
               .WithOne(b => b.Batch)
               .HasForeignKey(b => b.BatchId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(b => b.SeedDate)
               .IsRequired(false);

        builder.Property(b => b.HarvestDate)
               .IsRequired(false);

        builder.Property(b => b.StatusId)
               .IsRequired();

        builder.Property(b => b.AddedDateTime)
               .IsRequired();

        builder.Property(b => b.ModifiedDateTime)
               .IsRequired();

        builder.Property(b => b.DeletedDateTime)
               .IsRequired(false);
    }
}
