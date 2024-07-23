using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Helpers;
using Accounting.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
namespace Accounting.BusinessLogics
{
    public class Users : IUsers
    {
        private readonly ILogger<Users>? _logger;
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
            User? user = null;
            bool isUsername = !NationalCodeValidator.IsValidNationalCode(username);
            password = SecurePasswordHasher.Hash(password);
            if (!isUsername)
                user = await _accounting.Users.FirstOrDefaultAsync(x => x.NationalCode == long.Parse(username) && x.Password == password);
            else
                user = await _accounting.Users.FirstOrDefaultAsync(x => x.UserName == username && x.Password == password);
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
            string token = string.Empty;
            User? user = await FindUserAsync(username, password);
            if (user != null && user.NationalCode != 0 && user.Status == 1)
            {
                token = await _auth.CreateTokenAsync(user);
            }
            return token;
        }

        public async Task<User?> GetSignupAsync(UserRequest userReq)
        {
            User? user = new();
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
                await InsertUserRoleByDefaultAsync(user.Id);
            }
            else if (userReq != null && userReq.NationalCode != 0)
            {
                user = await FindUserAsync(userReq!.NationalCode.ToString());
            }
            return user;
        }

        public async Task<Contact> InsertUserContactsAsync(UserContact userContact)
        {
            Contact? contact = new();
            User? user = await FindUserByIdAsync(userContact.UserId);
            Contact? cont = await FindUserContactAsync(userContact.UserId);
            if (user != null)
            {
                contact = cont ?? new Contact();
                try
                {
                    if (cont == null)
                    {
                        contact!.Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_contact");
                    }

                    contact.Status = userContact.Status;
                    contact.Tells = userContact.Tells;
                    contact.Addresses = JsonConvert.SerializeObject(userContact.Addresses);
                    contact.UserId = userContact.UserId;
                    contact.RegionId = userContact.RegionId;
                    contact.Mobiles = userContact.Mobiles;
                    contact.RegDate = DateTime.Now;
                    if (cont == null)
                    {
                        await _accounting.Contacts.AddAsync(contact);
                    }
                    else
                    {
                        _accounting.Contacts.Update(contact);
                    }

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
            UserInfo userInfo = new();
            User? user = await FindUserByIdAsync(profile.UserId);
            UserInfo? userinf = await FindUserInfoAsync(profile.UserId);
            if (user != null)
            {
                userInfo = userinf ?? new UserInfo();
                try
                {
                    if (userinf == null)
                    {
                        userInfo.Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_userinfo");
                    }

                    userInfo.FirstName = profile.FirstName;
                    userInfo.LastName = profile.LastName;
                    if (!string.IsNullOrEmpty(profile.BirthDay))
                    {
                        userInfo.BirthDay = DateOnly.Parse(profile.BirthDay!);
                    }

                    userInfo.Gender = profile.Gender;
                    userInfo.UserId = profile.UserId;
                    userInfo.SedadInfo = null;
                    userInfo.Status = 0;
                    userInfo.FatherName = profile.FatherName;
                    userInfo.RegDate = DateTime.Now;
                    if (userinf == null)
                    {
                        await _accounting.UserInfos.AddAsync(userInfo);
                    }
                    else
                    {
                        _accounting.UserInfos.Update(userInfo);
                    }

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

        public async Task InsertUserRoleByDefaultAsync(long userId)
        {
            UserRole userRole = new();
            userRole.Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_userrole");
            userRole.RoleId = 21; // By default is "CUSTOMER"
            userRole.UserId = userId;
            userRole.RegUserId = userId;
            userRole.Status = 1;
            userRole.RegDate = DateTime.Now;
            await _accounting.UserRoles.AddAsync(userRole);
            await _accounting.SaveChangesAsync();

        }

        public async Task<bool> IsExistUserAsync(long nationalCode)
        {
            return await _accounting.Users.AnyAsync(x => x.NationalCode == nationalCode);
        }

        public async Task SaveUserSessionInfo(SessionInfo session)
        {
            User? user = await FindUserByIdAsync(session.UserId);
            if (user != null && user.Id == session.UserId)
            {
                await _accounting.UserSessions.AddAsync(new UserSession()
                {
                    Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_usersession"),
                    UserId = user.Id,
                    SessionDate = DateTime.Now,
                    SessionInfo = session.SessionJsonInfo!
                });
                await _accounting.SaveChangesAsync();
            }
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

        public async Task UpdateUserAsync(User updatedUser)
        {
            User? existUser = await FindUserByIdAsync(updatedUser.Id);
            if (existUser != null && existUser.NationalCode != 0)
            {
                existUser.NationalCode = isValid(updatedUser.NationalCode) ? updatedUser.NationalCode : existUser.NationalCode;
                existUser.Password = isValid(updatedUser.Password!) ? updatedUser.Password : existUser.Password;
                existUser.Status = isValid(updatedUser.Status) ? updatedUser.Status : existUser.Status;
                existUser.UserName = isValid(updatedUser.UserName!) ? updatedUser.UserName : existUser.UserName;
                existUser.Otpinfo = isValid(updatedUser.Otpinfo!) ? updatedUser.Otpinfo : existUser.Otpinfo;
                existUser.RegDate = isValid(updatedUser.RegDate) ? updatedUser.RegDate : existUser.RegDate;
                existUser.Email = isValid(updatedUser.Email!) ? updatedUser.Email : existUser.Email;
                existUser.Id = isValid(updatedUser.Id) ? updatedUser.Id : existUser.Id;
                _accounting.Entry(existUser).State = EntityState.Modified;
                await _accounting.SaveChangesAsync();
            }
        }

        private bool isValid(dynamic data)
        {
            if (data != null)
            {
                if (data is string && !string.IsNullOrWhiteSpace(data))
                    return true;
                else if ((data is long || data is short || data is decimal || data is int) && data != 0)
                    return true;
                else if (data is bool)
                    return true;
                else if ((data is List<string> || data is List<long> || data is List<int> || data is List<decimal>) && data.Count > 0)
                    return true;
                else return false;
            }
            return false;
        }
    }
}

