using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VHS.Data.Models.Audit;

namespace VHS.Data.Audit.Mappings;

public class OPCAuditMap : IEntityTypeConfiguration<OPCAudit>
{
    public void Configure(EntityTypeBuilder<OPCAudit> builder)
    {
        builder.ToTable("OPCAudits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
    }
}
