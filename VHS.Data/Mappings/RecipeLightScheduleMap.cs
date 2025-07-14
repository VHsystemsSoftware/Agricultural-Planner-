using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class RecipeLightScheduleMap : IEntityTypeConfiguration<RecipeLightSchedule>
{
    public void Configure(EntityTypeBuilder<RecipeLightSchedule> builder)
    {
        builder.ToTable("RecipeLightSchedules");
        builder.HasKey(rls => rls.Id);
        builder.Property(rls => rls.Id).ValueGeneratedOnAdd();

        builder.HasOne(rls => rls.Recipe)
               .WithMany(r => r.RecipeLightSchedules)
               .HasForeignKey(rls => rls.RecipeId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(rls => rls.LightZoneSchedule)
               .WithMany()
               .HasForeignKey(rls => rls.LightZoneScheduleId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(rls => rls.TargetDLI)
               .IsRequired(false);

        builder.Property(rls => rls.AddedDateTime).IsRequired();
        builder.Property(rls => rls.ModifiedDateTime).IsRequired();
        builder.Property(rls => rls.DeletedDateTime).IsRequired(false);
    }
}
