using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;

namespace Accounting.BusinessLogics
{
    public class Users : IUsers
    {
        private readonly GAccountingDbContext _accounting;
        private readonly AuthenticationService _authService;
        private readonly ILogger<Users> _logger;
        public Users(GAccountingDbContext accounting, ILogger<Users> logger, AuthenticationService authService)
        {
            _accounting = accounting;
            _logger = logger;
            _authService = authService;
        }

        public async Task<string> GetSignin(long NationalCode, long Mobile)
        {
            User? user = await
                _accounting
                .Users
                .FirstOrDefaultAsync(x => x.NationalCode == NationalCode && x.Mobile == Mobile);
            return user != null ? _authService.CreateToken(user) : "";

        }

        public async Task<User?> GetSignup(User? user)
        {
            if (user != null && !await IsExistUser(user.Id))
            {
                _ = await _accounting.Users.AddAsync(user);
                _ = await _accounting.SaveChangesAsync();
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
