using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VHS.Data.Auth.Mappings;
using VHS.Data.Auth.Models.Auth;

namespace VHS.Data.Auth;

public class VHSAuthDBContext : IdentityDbContext<
    User,
    IdentityRole<Guid>,
    Guid,
    IdentityUserClaim<Guid>,
    IdentityUserRole<Guid>,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>
{
    private static readonly int COMMAND_TIMEOUT = (int)TimeSpan.FromMinutes(60).TotalSeconds;

    public VHSAuthDBContext(DbContextOptions<VHSAuthDBContext> options) : base(options)
    {
        Database.SetCommandTimeout(COMMAND_TIMEOUT);
    }

    public DbSet<UserSetting> UserSettings { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Auth
        modelBuilder.ApplyConfiguration(new UserMap());
        modelBuilder.ApplyConfiguration(new UserSettingMap());
    }
}
