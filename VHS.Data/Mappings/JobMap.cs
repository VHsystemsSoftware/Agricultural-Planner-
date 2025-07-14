using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class JobMap : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("Jobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.OrderOnDay)
               .IsRequired()
               .HasDefaultValue(1);

        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(100);

		builder.Property(x => x.AddedDateTime).IsRequired();
        builder.Property(x => x.ModifiedDateTime).IsRequired();
        builder.Property(t => t.DeletedDateTime).IsRequired(false);
    }
}
