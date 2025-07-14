using VHS.Data.Common.Infrastructure;
using VHS.Data.Auth.Models.Auth;

namespace VHS.Data.Auth.Repositories;

public interface IUserRepository : IRepository<User>
{
}
public class UserRepository : Repository<User>, IUserRepository
{
    private readonly VHSAuthDBContext _context;

    public UserRepository(VHSAuthDBContext context) : base(context)
    {
        _context = context;
    }
}
