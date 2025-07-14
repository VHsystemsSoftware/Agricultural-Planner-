using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class RackMap : IEntityTypeConfiguration<Rack>
{
    public void Configure(EntityTypeBuilder<Rack> builder)
    {
        builder.ToTable("Racks");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.Property(r => r.Name)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(r => r.TypeId).IsRequired();
        builder.Property(r => r.LayerCount).IsRequired();
        builder.Property(r => r.TrayCountPerLayer).IsRequired();
			builder.Property(f => f.Number).IsRequired();

			builder.HasOne(r => r.Floor)
               .WithMany(f => f.Racks)
               .HasForeignKey(r => r.FloorId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(r => r.Layers)
               .WithOne(l => l.Rack)
               .HasForeignKey(l => l.RackId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(r => r.AddedDateTime).IsRequired();
        builder.Property(r => r.DeletedDateTime).IsRequired(false);
    }
}
