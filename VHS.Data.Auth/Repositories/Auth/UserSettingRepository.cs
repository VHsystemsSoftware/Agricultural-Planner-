using VHS.Data.Common.Infrastructure;
using VHS.Data.Auth.Models;
using VHS.Data.Auth.Models.Auth;

namespace VHS.Data.Auth.Repositories;

public interface IUserSettingRepository : IRepository<UserSetting>
{
}
public class UserSettingRepository : Repository<UserSetting>, IUserSettingRepository
{
    private readonly VHSAuthDBContext _context;

    public UserSettingRepository(VHSAuthDBContext context) : base(context)
    {
        _context = context;
    }
}
