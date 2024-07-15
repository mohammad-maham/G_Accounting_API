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

        public async Task<User?> FindUserAsync(string username)
        {
            bool isUsername = !NationalCodeValidator.IsValidNationalCode(username);

            return await _accounting.Users.FirstOrDefaultAsync(x =>
            (x.UserName == username && isUsername) ||
            (x.NationalCode == long.Parse(username) && !isUsername)) ?? new User();
        }

        public async Task<User?> FindUserAsync(string username, string password)
        {
            bool isUsername = !NationalCodeValidator.IsValidNationalCode(username);
            password = SecurePasswordHasher.Hash(password);

            User? user = await _accounting.Users.SingleOrDefaultAsync(x =>
            ((x.NationalCode == long.Parse(username) && !isUsername) ||
            (x.UserName == username && isUsername)) &&
            x.Password == password);

            return user;
        }

        public async Task<Contact?> FindUserContactAsync(long userId)
        {
            return await _accounting.Contacts.FirstOrDefaultAsync(x => x.UserId == userId) ?? new Contact();
        }

        public async Task<UserInfo?> FindUserInfoAsync(long userId)
        {
            return await _accounting.UserInfos.FirstOrDefaultAsync(x => x.UserId == userId) ?? new UserInfo();
        }

        [Obsolete]
        public async Task<string> GetSigninAsync(string username, string password)
        {
            string? hashedPassword = SecurePasswordHasher.Hash(password);
            User? user = await FindUserAsync(username, hashedPassword);
            return user != null ? await _auth.CreateTokenAsync(user) : "";
        }

        public async Task<User?> GetSignupAsync(UserReq user)
        {
            User userInf = new();
            if (user != null && !await IsExistUserAsync((long)user.NationalCode))
            {
                userInf.Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_user");
                userInf.NationalCode = user.NationalCode;
                userInf.Email = user.Email;
                userInf.Mobile = user.Mobile;
                userInf.RegDate = DateTime.Now;
                userInf.Status = 0;
                await _accounting.Users.AddAsync(userInf);
                await _accounting.SaveChangesAsync();
            }
            return userInf;
        }

        public async Task<Contact> InsertUserContactsAsync(UserContact userContact)
        {
            Contact? contact = await FindUserContactAsync(userContact.UserId);
            if (contact == null)
            {
                try
                {
                    contact!.Status = userContact.Status;
                    contact.Tells = userContact.Tells;
                    contact.Addresses = userContact.Addresses;
                    contact.UserId = userContact.UserId;
                    contact.RegionId = userContact.RegionId;
                    contact.Mobiles = userContact.Mobiles;
                    contact.RegDate = DateTime.Now;
                    await _accounting.Contacts.AddAsync(contact);
                    await _accounting.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
            return contact;
        }

        public async Task<UserInfo> InsertUserInfoAsync(UserProfile profile)
        {
            UserInfo? userInfo = await FindUserInfoAsync(profile.UserId);
            if (userInfo == null)
            {
                try
                {
                    userInfo!.FirstName = profile.FirstName;
                    userInfo.LastName = profile.LastName;
                    userInfo.BirthDay = profile.BirthDay;
                    userInfo.Gender = profile.Gender;
                    userInfo.UserId = profile.UserId;
                    userInfo.SedadInfo = "";
                    userInfo.Status = 0;
                    userInfo.FirstName = profile.FirstName;
                    userInfo.RegDate = DateTime.Now;
                    await _accounting.UserInfos.AddAsync(userInfo);
                    await _accounting.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
            return userInfo;
        }

        public async Task<bool> IsExistUserAsync(long nationalCode)
        {
            return await _accounting.Users.AnyAsync(x => x.NationalCode == nationalCode);
        }

        [Obsolete]
        public async Task SetPasswordAsync(string username, string password)
        {
            User? user = await FindUserAsync(username);
            if (user != null)
            {
                string? hashedPassword = SecurePasswordHasher.Hash(password);
                user.Password = hashedPassword;
                await _accounting.SaveChangesAsync();
            }
        }
    }
}
