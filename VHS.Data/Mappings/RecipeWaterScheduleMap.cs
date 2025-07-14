using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class RecipeWaterScheduleMap : IEntityTypeConfiguration<RecipeWaterSchedule>
{
    public void Configure(EntityTypeBuilder<RecipeWaterSchedule> builder)
    {
        builder.ToTable("RecipeWaterSchedules");
        builder.HasKey(rws => rws.Id);
        builder.Property(rws => rws.Id).ValueGeneratedOnAdd();

        builder.HasOne(rws => rws.Recipe)
               .WithMany(r => r.RecipeWaterSchedules)
               .HasForeignKey(rws => rws.RecipeId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(rws => rws.WaterZoneSchedule)
               .WithMany()
               .HasForeignKey(rws => rws.WaterZoneScheduleId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(rws => rws.TargetDWR)
               .IsRequired(false);

        builder.Property(rws => rws.AddedDateTime).IsRequired();
        builder.Property(rws => rws.ModifiedDateTime).IsRequired();
        builder.Property(rws => rws.DeletedDateTime).IsRequired(false);
    }
}
