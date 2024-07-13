using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;
using Microsoft.EntityFrameworkCore;

namespace Accounting.BusinessLogics
{
    public class Users : IUsers
    {
        private readonly ILogger<Users> _logger;
        private readonly GAccountingDbContext _accounting;
        private readonly IAuthentication _auth;

        public Users(GAccountingDbContext accounting, ILogger<Users> logger, IAuthentication auth)
        {
            _accounting = accounting;
            _logger = logger;
            _auth = auth;
        }

        public async Task<string> GetSignin(long NationalCode, long Mobile)
        {
            User? user = await
                _accounting
                .Users
                .FirstOrDefaultAsync(x => x.NationalCode == NationalCode && x.Mobile == Mobile);
            return user != null ? _auth.CreateToken(user) : "";

        }

        public async Task<User?> GetSignup(User? user)
        {
            if (user != null && !await IsExistUser(user.Id))
            {
                await _accounting.Users.AddAsync(user);
                await _accounting.SaveChangesAsync();
            }
            else { user = null; }
            return user;
        }

        public async Task<bool> IsExistUser(long userId)
        {
            return await _accounting.Users.AnyAsync(x => x.Id == userId);
        }
    }
}
