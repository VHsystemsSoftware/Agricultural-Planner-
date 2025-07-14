using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class FloorMap : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> builder)
    {
        builder.ToTable("Floors");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedOnAdd();

        builder.Property(f => f.FarmId).IsRequired();

        builder.Property(f => f.Name)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(f => f.Number).IsRequired();
        builder.Property(f => f.AddedDateTime).IsRequired();
        builder.Property(f => f.DeletedDateTime).IsRequired(false);

        builder.HasOne(f => f.Farm)
               .WithMany(farm => farm.Floors)
               .HasForeignKey(f => f.FarmId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(f => f.Racks)
               .WithOne(r => r.Floor)
               .HasForeignKey(r => r.FloorId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
