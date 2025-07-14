using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VHS.Data.Models.Audit;

namespace VHS.Data.Audit.Mappings;

public class AuditLogMap : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.EntityName).IsRequired();
        builder.Property(x => x.Action).IsRequired();
        builder.Property(x => x.KeyValues).IsRequired();
        builder.Property(x => x.Timestamp).IsRequired();
    }
}
