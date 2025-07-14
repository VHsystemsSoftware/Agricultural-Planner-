using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class TrayStateMap : IEntityTypeConfiguration<TrayState>
{
    public void Configure(EntityTypeBuilder<TrayState> builder)
    {
        builder.ToTable("TrayStates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.TrayId).IsRequired();

        builder.HasOne(t => t.GrowLayer)
                .WithMany(x=>x.GrowTrayStates)
			   .HasForeignKey(t => t.GrowLayerId)
               .OnDelete(DeleteBehavior.NoAction);

		builder.HasOne(t => t.Tray)
				.WithMany(x => x.TrayStates)
			   .HasForeignKey(t => t.TrayId)
			   .IsRequired(false)
			   .OnDelete(DeleteBehavior.NoAction);

		builder.HasOne(t => t.PreGrowLayer)
			  .WithMany(x => x.PreGrowTrayStates)
			 .HasForeignKey(t => t.PreGrowLayerId)
			 .OnDelete(DeleteBehavior.NoAction);

		builder.Property(t => t.AddedDateTime).IsRequired();

    }
}
