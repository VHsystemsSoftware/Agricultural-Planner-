using Microsoft.EntityFrameworkCore;
using VHS.Data.Audit.Mappings;
using VHS.Data.Models.Audit;

namespace VHS.Data.Audit;

public class VHSAuditDBContext : DbContext
{
    private static readonly int COMMAND_TIMEOUT = (int)TimeSpan.FromMinutes(60).TotalSeconds;

    public VHSAuditDBContext(DbContextOptions<VHSAuditDBContext> options) : base(options)
    {
        Database.SetCommandTimeout(COMMAND_TIMEOUT);
    }

    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<OPCAudit> OPCAudits { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AuditLogMap());
        modelBuilder.ApplyConfiguration(new OPCAuditMap());
    }
}
