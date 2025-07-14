using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class WaterZoneMap : IEntityTypeConfiguration<WaterZone>
{
    public void Configure(EntityTypeBuilder<WaterZone> builder)
    {
        builder.ToTable("WaterZones");
        builder.HasKey(wz => wz.Id);
        builder.Property(wz => wz.Id).ValueGeneratedOnAdd();

        builder.Property(wz => wz.Name)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(wz => wz.TargetDWR)
               .IsRequired(false);

        builder.HasOne(wz => wz.Farm)
               .WithMany()
               .HasForeignKey(wz => wz.FarmId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(wz => wz.WaterZoneSchedules)
               .WithOne(wzs => wzs.WaterZone)
               .HasForeignKey(wzs => wzs.WaterZoneId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(wz => wz.AddedDateTime).IsRequired();
        builder.Property(wz => wz.ModifiedDateTime).IsRequired();
        builder.Property(wz => wz.DeletedDateTime).IsRequired(false);
    }
}
