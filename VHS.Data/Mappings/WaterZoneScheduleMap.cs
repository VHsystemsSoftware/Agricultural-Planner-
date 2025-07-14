using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;
public class WaterZoneScheduleMap : IEntityTypeConfiguration<WaterZoneSchedule>
{
    public void Configure(EntityTypeBuilder<WaterZoneSchedule> builder)
    {
        builder.ToTable("WaterZoneSchedules");
        builder.HasKey(wzs => wzs.Id);
        builder.Property(wzs => wzs.Id).ValueGeneratedOnAdd();

        builder.HasOne(wzs => wzs.WaterZone)
               .WithMany(wz => wz.WaterZoneSchedules)
               .HasForeignKey(wzs => wzs.WaterZoneId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(wzs => wzs.StartTime).IsRequired();
        builder.Property(wzs => wzs.EndTime).IsRequired();

        builder.Property(wzs => wzs.Volume)
               .HasPrecision(10, 2)
               .IsRequired();

        builder.Property(wzs => wzs.VolumeUnit)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(wzs => wzs.CalculatedDWR)
               .IsRequired(false);

        builder.Property(wzs => wzs.AddedDateTime).IsRequired();
        builder.Property(wzs => wzs.ModifiedDateTime).IsRequired();
        builder.Property(wzs => wzs.DeletedDateTime).IsRequired(false);
    }
}
