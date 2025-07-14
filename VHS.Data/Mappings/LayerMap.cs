using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class LayerMap : IEntityTypeConfiguration<Layer>
{
    public void Configure(EntityTypeBuilder<Layer> builder)
    {
        builder.ToTable("Layers");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedOnAdd();

        builder.Property(l => l.RackId).IsRequired();
        builder.Property(l => l.Number).IsRequired();

        builder.Property(l => l.AddedDateTime).IsRequired();
        builder.Property(l => l.DeletedDateTime).IsRequired(false);

		builder.HasMany(x => x.PreGrowTrayStates)
	        .WithOne()
	        .HasForeignKey(x => x.PreGrowLayerId)
	        .OnDelete(DeleteBehavior.NoAction);

		builder.HasMany(x => x.GrowTrayStates)
			.WithOne()
			.HasForeignKey(x => x.GrowLayerId)
			.OnDelete(DeleteBehavior.NoAction);
    }
}
