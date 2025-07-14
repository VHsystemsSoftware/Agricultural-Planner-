using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VHS.Data.Core.Mappings;

public class TrayStateAuditMap : IEntityTypeConfiguration<TrayStateAudit>
{
    public void Configure(EntityTypeBuilder<TrayStateAudit> builder)
    {
        builder.ToTable("TrayStateAudits");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

		builder.Property(t => t.AddedDateTime).IsRequired();

    }
}
