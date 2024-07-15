using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Helpers;
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

        public async Task<User?> FindUserInfoAsync(string username)
        {
            bool isUsername = !NationalCodeValidator.IsValidNationalCode(username);

            return await _accounting.Users.FirstOrDefaultAsync(x =>
            (x.UserName == username && isUsername) ||
            (x.NationalCode == long.Parse(username) && !isUsername));
        }

        public async Task<User?> FindUserInfoAsync(string username, string password)
        {
            bool isUsername = !NationalCodeValidator.IsValidNationalCode(username);

            return await _accounting.Users.FirstOrDefaultAsync(x =>
            (x.NationalCode == long.Parse(username) && !isUsername) ||
            (x.UserName == username && isUsername &&
            x.Password == password));
        }

        [Obsolete]
        public async Task<string> GetSigninAsync(string username, string password)
        {
            string? hashedPassword = SecurePasswordHasher.Hash(password);
            User? user = await FindUserInfoAsync(username, hashedPassword);
            return user != null ? await _auth.CreateTokenAsync(user) : "";
        }

        public async Task<User?> GetSignupAsync(User? user)
        {
            if (user != null && !await IsExistUserAsync(user.Id))
            {
                await _accounting.Users.AddAsync(user);
                await _accounting.SaveChangesAsync();
            }
            else { user = null; }
            return user;
        }

        public async Task<bool> IsExistUserAsync(long userId)
        {
            return await _accounting.Users.AnyAsync(x => x.Id == userId);
        }

        [Obsolete]
        public async Task SetPasswordAsync(string username, string password)
        {
            User? user = await FindUserInfoAsync(username);
            if (user != null)
            {
                string? hashedPassword = SecurePasswordHasher.Hash(password);
                user.Password = hashedPassword;
                await _accounting.SaveChangesAsync();
            }
        }
    }
}
