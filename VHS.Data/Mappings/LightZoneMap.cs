using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class LightZoneMap : IEntityTypeConfiguration<LightZone>
{
    public void Configure(EntityTypeBuilder<LightZone> builder)
    {
        builder.ToTable("LightZones");
        builder.HasKey(lz => lz.Id);
        builder.Property(lz => lz.Id).ValueGeneratedOnAdd();

        builder.Property(lz => lz.Name)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(lz => lz.TargetDLI).IsRequired(false);

        builder.HasOne(lz => lz.Farm)
               .WithMany()
               .HasForeignKey(lz => lz.FarmId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(lz => lz.LightZoneSchedules)
               .WithOne(lzs => lzs.LightZone)
               .HasForeignKey(lzs => lzs.LightZoneId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(lz => lz.AddedDateTime).IsRequired();
        builder.Property(lz => lz.ModifiedDateTime).IsRequired();
        builder.Property(lz => lz.DeletedDateTime).IsRequired(false);
    }
}
