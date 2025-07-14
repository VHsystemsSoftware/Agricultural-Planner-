using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;
using System.Xml;

namespace VHS.Data.Core.Mappings;

public class TrayMap : IEntityTypeConfiguration<Tray>
{
    public void Configure(EntityTypeBuilder<Tray> builder)
    {
        builder.ToTable("Trays");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.FarmId).IsRequired();
        builder.HasOne(t => t.Farm)
               .WithMany()
               .HasForeignKey(t => t.FarmId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.Property(t => t.Tag)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(t => t.StatusId).IsRequired();

        builder.Property(t => t.AddedDateTime).IsRequired();
        builder.Property(t => t.DeletedDateTime).IsRequired(false);

		builder
	        .HasIndex(e => e.Tag)
	        .IsUnique();
	}
}
