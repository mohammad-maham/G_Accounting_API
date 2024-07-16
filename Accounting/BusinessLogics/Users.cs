using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Helpers;
using Accounting.Models;
using Google.Apis.Gmail.v1.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
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
            (x.NationalCode == long.Parse(username) && !isUsername));
        }

        public async Task<User?> FindUserAsync(string username, string password)
        {
            bool isUsername = !NationalCodeValidator.IsValidNationalCode(username);
            password = SecurePasswordHasher.Hash(password);

            User? user = await _accounting.Users.FirstOrDefaultAsync(x =>
            ((x.NationalCode == long.Parse(username) && !isUsername) ||
            (x.UserName == username && isUsername)) &&
            x.Password == password);

            return user;
        }

        public async Task<User?> FindUserByIdAsync(long userId)
        {
            return await _accounting.Users.FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task<Contact?> FindUserContactAsync(long userId)
        {
            return await _accounting.Contacts.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<UserInfo?> FindUserInfoAsync(long userId)
        {
            return await _accounting.UserInfos.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        [Obsolete]
        public async Task<string> GetSigninAsync(string username, string password)
        {
            User? user = await FindUserAsync(username, password);
            return user != null ? await _auth.CreateTokenAsync(user) : "";
        }

        public async Task<User?> GetSignupAsync(UserRequest userReq)
        {
            User user = new();
            if (userReq != null && !await IsExistUserAsync((long)userReq.NationalCode))
            {
                user.Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_user");
                user.NationalCode = userReq.NationalCode;
                user.Email = userReq.Email;
                user.Mobile = userReq.Mobile;
                user.RegDate = DateTime.Now;
                user.Status = 0;
                await _accounting.Users.AddAsync(user);
                await _accounting.SaveChangesAsync();
            }
            return user;
        }

        public async Task<Contact> InsertUserContactsAsync(UserContact userContact)
        {
            Contact? contact = new Contact();
            User? user = await FindUserByIdAsync(userContact.UserId);
            Contact? cont = await FindUserContactAsync(userContact.UserId);
            if (user != null)
            {
                contact = cont != null ? cont : new Contact();
                try
                {
                    if (cont == null)
                        contact!.Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_contact");
                    contact.Status = userContact.Status;
                    contact.Tells = userContact.Tells;
                    contact.Addresses = JsonConvert.SerializeObject( userContact.Addresses);
                    contact.UserId = userContact.UserId;
                    contact.RegionId = userContact.RegionId;
                    contact.Mobiles = userContact.Mobiles;
                    contact.RegDate = DateTime.Now;
                    if (cont == null)
                        await _accounting.Contacts.AddAsync(contact);
                    else
                        _accounting.Contacts.Update(contact);
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
            UserInfo userInfo = new UserInfo();
            User? user = await FindUserByIdAsync(profile.UserId);
            UserInfo? userinf = await FindUserInfoAsync(profile.UserId);
            if (user != null)
            {
                userInfo = userinf != null ? userinf : new UserInfo();
                try
                {
                    if (userinf == null)
                        userInfo.Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_userinfo");
                    userInfo.FirstName = profile.FirstName;
                    userInfo.LastName = profile.LastName;
                    if (!string.IsNullOrEmpty(profile.BirthDay))
                        userInfo.BirthDay = DateOnly.Parse(profile.BirthDay!);
                    userInfo.Gender = profile.Gender;
                    userInfo.UserId = profile.UserId;
                    userInfo.SedadInfo = null;
                    userInfo.Status = 0;
                    userInfo.FatherName = profile.FatherName;
                    userInfo.RegDate = DateTime.Now;
                    if (userinf == null)
                        await _accounting.UserInfos.AddAsync(userInfo);
                    else
                        _accounting.UserInfos.Update(userInfo);
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
