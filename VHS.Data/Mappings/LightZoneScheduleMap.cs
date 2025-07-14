using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class LightZoneScheduleMap : IEntityTypeConfiguration<LightZoneSchedule>
{
    public void Configure(EntityTypeBuilder<LightZoneSchedule> builder)
    {
        builder.ToTable("LightZoneSchedules");
        builder.HasKey(lzs => lzs.Id);
        builder.Property(lzs => lzs.Id).ValueGeneratedOnAdd();

        builder.HasOne(lzs => lzs.LightZone)
               .WithMany(lz => lz.LightZoneSchedules)
               .HasForeignKey(lzs => lzs.LightZoneId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(lzs => lzs.StartTime).IsRequired();
        builder.Property(lzs => lzs.EndTime).IsRequired();
        builder.Property(lzs => lzs.Intensity)
               .HasPrecision(5, 2)
               .IsRequired();
        builder.Property(lzs => lzs.CalculatedDLI).IsRequired(false);

        builder.Property(lzs => lzs.AddedDateTime).IsRequired();
        builder.Property(lzs => lzs.ModifiedDateTime).IsRequired();
        builder.Property(lzs => lzs.DeletedDateTime).IsRequired(false);
    }
}
