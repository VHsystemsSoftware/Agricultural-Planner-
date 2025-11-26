using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class BatchPlanMap : IEntityTypeConfiguration<GrowPlan>
{
    public void Configure(EntityTypeBuilder<GrowPlan> builder)
    {
        builder.ToTable("GrowPlans");
        builder.HasKey(bc => bc.Id);
        builder.Property(bc => bc.Id).ValueGeneratedOnAdd();

        builder.Property(bc => bc.Name)
               .IsRequired()
               .HasMaxLength(255);

        builder.HasOne(bc => bc.Farm)
               .WithMany()
               .HasForeignKey(bc => bc.FarmId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(bp => bp.RecipeId)
               .IsRequired(false);

        builder.HasOne(bc => bc.Recipe)
               .WithMany(r => r.BatchPlans)
               .HasForeignKey(bc => bc.RecipeId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(bc => bc.TraysPerDay)
               .IsRequired();

        builder.Property(bc => bc.StartDate)
               .IsRequired(false);

        builder.HasMany(bc => bc.Batches)
               .WithOne(b => b.GrowPlan)
               .HasForeignKey(b => b.GrowPlanId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(bp => bp.StatusId)
               .IsRequired();

        builder.Property(bc => bc.AddedDateTime).IsRequired();
        builder.Property(bc => bc.ModifiedDateTime).IsRequired();
        builder.Property(bc => bc.DeletedDateTime).IsRequired(false);
    }
}
