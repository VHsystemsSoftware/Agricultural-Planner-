using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class JobTrayMap : IEntityTypeConfiguration<JobTray>
{
	public void Configure(EntityTypeBuilder<JobTray> builder)
	{
		builder.ToTable("JobTrays");
		builder.HasKey(x => x.Id);
		builder.Property(x => x.Id).ValueGeneratedOnAdd();

		builder.Property(x => x.AddedDateTime).IsRequired();

		builder.HasOne(x => x.Job)
			.WithMany(x => x.JobTrays)
			.HasForeignKey(x => x.JobId)
			.IsRequired(false)
			.OnDelete(DeleteBehavior.NoAction);

		builder.HasOne(x => x.Tray)
			.WithMany()
			.HasForeignKey(x => x.TrayId)
			.OnDelete(DeleteBehavior.NoAction);

		builder.Property(x => x.DestinationLocation)
			.IsRequired();

		builder.HasOne(x => x.DestinationLayer)
			.WithMany()
			.HasForeignKey(x => x.DestinationLayerId)
			.OnDelete(DeleteBehavior.NoAction);

		builder.Property(x => x.OrderInJob);
		builder.Property(x => x.OrderOnLayer);

		builder.Property(x => x.RecipeId)
			   .IsRequired(false);
	}
}
