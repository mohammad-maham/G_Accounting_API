using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Helpers;
using Accounting.Models;
using Accounting.Services;
using GoldHelpers.Helpers;
using GoldHelpers.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using System.Net;
namespace Accounting.BusinessLogics
{
    public class Users : IUsers
    {
        private readonly ILogger<Users>? _logger;
        private readonly GAccountingDbContext _accounting;
        private readonly IAuthentication _auth;

        public Users()
        {
            _accounting = new GAccountingDbContext();
            _auth = new AuthenticationService();
        }

        public Users(GAccountingDbContext accounting, ILogger<Users> logger, IAuthentication auth)
        {
            _accounting = accounting;
            _logger = logger;
            _auth = auth;
        }

        public User? FindUser(string username)
        {
            bool isUsername = !NationalCodeValidator.IsValidNationalCode(username);

            return _accounting.Users.FirstOrDefault(x =>
            (x.UserName == username && isUsername) ||
            (x.NationalCode == long.Parse(username) && !isUsername));
        }

        public User? FindUser(string username, string password)
        {
            User? user = null;
            bool isUsername = !NationalCodeValidator.IsValidNationalCode(username);
            password = SecurePasswordHasher.Hash(password);

            user = !isUsername
                ? _accounting.Users.FirstOrDefault(x => x.NationalCode == long.Parse(username) && x.Password == password)
                : _accounting.Users.FirstOrDefault(x => x.UserName == username && x.Password == password);
            return user;
        }

        public User? FindUserById(long userId)
        {
            return _accounting.Users.FirstOrDefault(x => x.Id == userId);
        }

        public Contact? FindUserContact(long userId)
        {
            return _accounting.Contacts.FirstOrDefault(x => x.UserId == userId);
        }
        public UserInfo? FindUserInfo(long userId)
        {
            return _accounting.UserInfos.FirstOrDefault(x => x.UserId == userId);
        }
        public LegalUserInfo? FindLegalUserInfo(long userId)
        {
            return _accounting.LegalUserInfos.FirstOrDefault(x => x.UserId == userId);
        }

        public UserInfoVM? FindFullUserInfo(long userId)
        {
            UserInfoVM? userInfo = new();
            userInfo = _accounting.UserInfos.SelectMany(usr => _accounting.UserRoles.Where(ur => ur.UserId == usr.UserId), (usr, ur) => new { usr, ur })
                   .SelectMany(urs => _accounting.Roles.Where(r => r.Id == urs.ur.RoleId), (urs, r) => new { urs, r })
                   .Where(w => w.urs.usr.UserId == userId)
                   .Select(x => new UserInfoVM()
                   {
                       Id = x.urs.usr.Id,
                       UserId = x.urs.usr.UserId,
                       BirthDay = x.urs.usr.BirthDay,
                       FatherName = x.urs.usr.FatherName,
                       FirstName = x.urs.usr.FirstName,
                       LastName = x.urs.usr.LastName,
                       Gender = x.urs.usr.Gender,
                       NationalCardImage = x.urs.usr.NationalCardImage,
                       RegDate = x.urs.usr.RegDate,
                       SedadInfo = x.urs.usr.SedadInfo,
                       Status = x.urs.usr.Status,
                       UserRoleId = x.r.Id,
                       UserRole = x.r.Description
                   })
                   .FirstOrDefault();

            if (userInfo != null && userInfo.UserId > 0)
            {
                User? user = _accounting.Users.FirstOrDefault(x => x.Id == userInfo.UserId);
                if (user != null && user.Id > 0)
                {
                    userInfo.Mobile = $"0{user.Mobile}";
                    userInfo.Email = user.Email;
                }
            }
            return userInfo;
        }

        public FullUserInfoVM GetFindFullUserInfo(long userId)
        {
            FullUserInfoVM? userInfo = new();
            var userInfos = _accounting.Users
                .SelectMany(x =>
                _accounting.UserInfos.Where(y => y.UserId == userId)
                .DefaultIfEmpty(),
                (u, ui) => new { u, ui })
                .ToList();

            userInfo = userInfos
                .Select(x => new FullUserInfoVM() { User = x.u, UserInfo = x.ui })
                .FirstOrDefault();

            return userInfo;
        }

        [Obsolete]
        public string GetSignin(string username, string? password = "", string? ip = "")
        {
            bool isFailed = false;
            User? user = new();
            string token = string.Empty;

            try
            {
                user = FindUser(username);
                if (user != null && !string.IsNullOrEmpty(password))
                {
                    user = FindUser(username, password);
                    isFailed = user == null || (user != null && user.UnlockDate != null && user.UnlockDate.Value.Date > DateTime.Now.Date);
                }

                var sessionInf = new
                {
                    UserLoginStatus = string.IsNullOrEmpty(token) ? "Success" : "Failed"
                };
                string jsonInfo = JsonConvert.SerializeObject(sessionInf);

                UserSession session = new UserSession()
                {
                    SessionDate = DateTime.Now,
                    Status = string.IsNullOrEmpty(token) ? 1 : -1,
                    SessionInfo = jsonInfo,
                    Ip = ip,
                    UserId = user!.Id
                };

                _accounting.UserSessions.Add(session);
                _accounting.SaveChanges();

                if (isFailed)
                {
                    (bool isBanned, int expireInterval) = this.CheckUserSessionBanState(user!.Id, ip);

                    if (isBanned)
                        user.UnlockDate = DateTime.Now.AddMinutes(expireInterval);
                    else
                        user.UnlockDate = null;

                    _accounting.Entry<User>(user).State = EntityState.Modified;
                    _accounting.SaveChanges();
                }
                else
                {
                    bool isValidUser = user != null && user.NationalCode != 0 && new List<int> { 1, 2, 3 }.Contains(user.Status);
                    if (isValidUser)
                        token = _auth.CreateToken(user!);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return token;
        }

        public User? GetSignup(UserRequest userReq)
        {
            User? user = new();
            if (userReq != null && !IsExistUser(userReq.NationalCode))
            {
                user.Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_user");
                user.NationalCode = userReq.NationalCode;
                user.Email = userReq.Email;
                user.Mobile = userReq.Mobile;
                user.RegDate = DateTime.Now;
                user.Status = 0;
                _accounting.Users.Add(user);
                _accounting.SaveChanges();
                InsertUserRoleByDefault(user.Id);
            }
            else if (userReq != null && userReq.NationalCode != 0)
            {
                user = FindUser(userReq!.NationalCode.ToString());
            }
            return user;
        }

        public UserInfo GetUserInfoByToken(string token)
        {
            UserInfo? userInfo = new();
            var sessions = _accounting.UserInfos
                .SelectMany(x =>
                _accounting.SessionMgrs.Where(y => y.Token == token)
                .DefaultIfEmpty(),
                (ui, sm) => new { ui, sm })
                .ToList();

            userInfo = sessions
                .Select(x => x.ui)
                .FirstOrDefault();

            return userInfo;
        }

        public List<GetUsersVM> GetUsersList()
        {
            List<GetUsersVM> users = [];

            users = _accounting.UserInfos
                .SelectMany(ui => _accounting.UserRoles.Where(x => x.UserId == ui.UserId), (ui, ur) => new { ui, ur })
            .Where(x => x.ui.Status == 1 && new long[] { 11, 12 }.Contains(x.ur.RoleId))
            .Select(x => new GetUsersVM() { UserId = x.ui.UserId, Username = $"{x.ui.FirstName} {x.ui.LastName}" })
            .ToList();

            users.Add(new GetUsersVM() { UserId = -10, Username = "کیف پول" });

            return users;
        }

        public Contact InsertUserContacts(UserContact userContact)
        {
            Contact? contact = new();
            User? user = FindUserById(userContact.UserId);
            Contact? cont = FindUserContact(userContact.UserId);
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
                        _accounting.Contacts.Add(contact);
                    }
                    else
                    {
                        _accounting.Contacts.Update(contact);
                    }

                    _accounting.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
            return contact;
        }

        public UserInfo InsertRealUserInfo(UserProfile profile)
        {
            UserInfo userInfo = new();
            User? user = FindUserById(profile.UserId);
            UserInfo? userinf = FindUserInfo(profile.UserId);
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
                    userInfo.BirthDay = profile.BirthDay;
                    userInfo.Gender = profile.Gender;
                    userInfo.UserId = profile.UserId;
                    userInfo.SedadInfo = null;
                    userInfo.Status = 0;
                    userInfo.FatherName = profile.FatherName;
                    userInfo.RegDate = DateTime.Now;
                    userInfo.NationalCardImage = profile.NationalCardImage;

                    if (userinf == null)
                    {
                        _accounting.UserInfos.Add(userInfo);
                    }
                    else
                    {
                        _accounting.UserInfos.Update(userInfo);
                    }

                    _accounting.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
            return userInfo;
        }

        public void InsertUserRoleByDefault(long userId)
        {
            UserRole userRole = new()
            {
                Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_userrole"),
                RoleId = 21, // By default is "CUSTOMER"
                UserId = userId,
                RegUserId = userId,
                Status = 1,
                RegDate = DateTime.Now
            };
            _accounting.UserRoles.Add(userRole);
            _accounting.SaveChanges();

        }

        public bool IsExistUser(long nationalCode)
        {
            return _accounting.Users.Any(x => x.NationalCode == nationalCode);
        }

        public void SaveUserSessionInfo(SessionInfo session)
        {
            User? user = FindUserById(session.UserId);
            if (user != null && user.Id == session.UserId)
            {
                _accounting.UserSessions.Add(new UserSession()
                {
                    Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_usersession"),
                    UserId = user.Id,
                    SessionDate = DateTime.Now,
                    SessionInfo = session.SessionJsonInfo!
                });
                _accounting.SaveChanges();
            }
        }

        [Obsolete]
        public void SetPassword(string username, string password)
        {
            User? user = FindUser(username);
            if (user != null)
            {
                string? hashedPassword = SecurePasswordHasher.Hash(password);
                user.Password = hashedPassword;
                _accounting.SaveChanges();
            }
        }

        public void UpdateUser(User updatedUser)
        {
            User? existUser = FindUserById(updatedUser.Id);
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
                _accounting.SaveChanges();
            }
        }

        private bool isValid(dynamic data)
        {
            return data != null
                ? data is string && !string.IsNullOrWhiteSpace(data)
                    ? true
                    : data is (long or short or decimal or int) and not (dynamic)0
|| data is bool
                    || (bool)((data is List<string> || data is List<long> || data is List<int> || data is List<decimal>) && data.Count > 0)
                : false;
        }

        public List<UsersList> GetUsersListByFilter(UsersList users)
        {
            List<UsersList> usersLists = [];

            var lstUsers = _accounting.Users
                .SelectMany(usr => _accounting.UserRoles.Where(userRoles => userRoles.UserId == usr.Id), (usr, userRoles) => new { usr, userRoles })
                .SelectMany(userInfRoles => _accounting.Roles.Where(roles => roles.Id == userInfRoles.userRoles.RoleId), (userInfRoles, roles) => new { userInfRoles, roles })
                .SelectMany(userInfRoleUsers => _accounting.UserInfos.Where(usrInfo => usrInfo.UserId == userInfRoleUsers.userInfRoles.usr.Id).DefaultIfEmpty(), (userInfRoleUsers, usrInfo) => new { userInfRoleUsers, usrInfo })
                .SelectMany(userInfo => _accounting.Statuses.Where(status => status.Id == userInfo.userInfRoleUsers.userInfRoles.usr.Status).DefaultIfEmpty(), (userInfo, status) => new { userInfo, status });

            if (users.FromRegDate != null)
            {
                lstUsers = lstUsers.Where(x => x.userInfo.userInfRoleUsers.userInfRoles.usr.RegDate >= users.FromRegDate);
            }
            if (users.ToRegDate != null)
            {
                lstUsers = lstUsers.Where(x => x.userInfo.userInfRoleUsers.userInfRoles.usr.RegDate <= users.ToRegDate);
            }
            if (users.RoleId is not null and not 0)
            {
                lstUsers = lstUsers.Where(x => x.userInfo.userInfRoleUsers.roles.Id == users.RoleId);
            }

            IEnumerable<UsersList> usr = lstUsers.ToList().Select(x => new UsersList()
            {
                UserId = x.userInfo.userInfRoleUsers.userInfRoles.usr.Id,
                Username = x.userInfo.userInfRoleUsers.userInfRoles.usr.UserName,
                StatusId = x.userInfo.userInfRoleUsers.userInfRoles.usr.Status,
                Status = x.status?.Caption,
                Birthday = x.userInfo.usrInfo?.BirthDay/*ConvertGregDateTimeToPersianString(x.userInfo.usrInfo != null && x.userInfo.usrInfo!.BirthDay.HasValue ? x.userInfo.usrInfo!.BirthDay!.Value.ToDateTime(TimeOnly.MinValue) : null, true)*/,
                Fathername = x.userInfo.usrInfo?.FatherName ?? "",
                Firstname = x.userInfo.usrInfo?.FirstName ?? "",
                Lastname = x.userInfo.usrInfo?.LastName ?? "",
                Mobile = x.userInfo.userInfRoleUsers.userInfRoles.usr.Mobile,
                NationalCode = x.userInfo.userInfRoleUsers.userInfRoles.usr.NationalCode,
                RegDate = ConvertGregDateTimeToPersianString(x.userInfo.userInfRoleUsers.userInfRoles.usr.RegDate, false),
                RoleId = x.userInfo.userInfRoleUsers.userInfRoles.userRoles.RoleId,
                Role = x.userInfo.userInfRoleUsers.roles.Description,
            });

            usersLists = usr.ToList();

            return usersLists;
        }

        public string ConvertGregDateTimeToPersianString(DateTime? date, bool onlyDate = false)
        {
            string data = string.Empty;
            if (date != null)
            {
                data = date.Value.ToString(onlyDate ? "yyyy/MM/dd" : "yyyy/MM/dd HH:mm:ss", new CultureInfo("fa-IR"));
            }
            return data;
        }

        public List<Role> GetRolesList()
        {
            return _accounting.Roles.Where(x => x.Status == 1).ToList();
        }

        public List<Status> GetStatusesList()
        {
            return _accounting.Statuses.ToList();
        }

        public void ChangeUserRole(UsersRoleVM userRole)
        {
            UserRole role = _accounting.UserRoles.Where(x => x.UserId == userRole.UserId).FirstOrDefault() ?? new UserRole();
            if (role != null && role.Id != 0)
            {
                role.RoleId = userRole.RoleId!.Value;
                _accounting.UserRoles.Update(role);
                _accounting.SaveChanges();
            }
        }

        public bool ValidateMobileNationalCode(string mobile, string nationalCode)
        {
            bool isOk = false;
            IConfigurationRoot? config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            short isActiveInqueries = config.GetValue<short>("ActiveInqueries");

            if (isActiveInqueries == 1)
            {
                try
                {
                    GoldAPIResult? result = new GoldAPIResponse(GoldHosts.Gateway, "/api/Authorization/GetValidateMobileNationalCode", new
                    {
                        Mobile = mobile,
                        NationalCode = nationalCode
                    }).Post();

                    isOk = result != null && !string.IsNullOrEmpty(result.Data) && bool.Parse(result.Data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return isOk;
            }
            else
                return true;
        }

        public bool ValidateRealUserInfo(RealUserInfoAuthVM infoAuthVM)
        {
            bool isOk = false;
            IConfigurationRoot? config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            short isActiveInqueries = config.GetValue<short>("ActiveInqueries");

            if (isActiveInqueries == 1)
            {
                try
                {
                    GoldAPIResult? result = new GoldAPIResponse(GoldHosts.Gateway, "/api/Authorization/GetValidateRealUserInfo", infoAuthVM).Post();
                    isOk = result != null && !string.IsNullOrEmpty(result.Data) && bool.Parse(result.Data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return isOk;
            }
            else
                return true;
        }

        public LegalUserInfo InsertLegalUserInfo(LegalUserInfo profile)
        {
            LegalUserInfo userInfo = new();
            User? user = FindUserById(profile.UserId);
            LegalUserInfo? userinf = FindLegalUserInfo(profile.UserId);
            if (user != null)
            {
                userInfo = userinf ?? new LegalUserInfo();
                try
                {
                    if (userinf == null)
                    {
                        userInfo.Id = DataBaseHelper.GetPostgreSQLSequenceNextVal(_accounting, "seq_userinfo");
                    }

                    userInfo.UserId = profile.UserId;
                    userInfo.Name = profile.Name;
                    userInfo.RegistrationDate = DateTime.Now;
                    userInfo.RegistrationNumber = profile.RegistrationNumber;
                    userInfo.UserId = profile.UserId;
                    userInfo.Status = 0;
                    userInfo.RegDate = DateTime.Now;
                    userInfo.RegistrationRegionId = profile.RegistrationRegionId;
                    userInfo.LastModifyInfoDate = profile.LastModifyInfoDate;
                    userInfo.AgentRole = profile.AgentRole;

                    if (userinf == null)
                    {
                        _accounting.LegalUserInfos.Add(userInfo);
                    }
                    else
                    {
                        _accounting.LegalUserInfos.Update(userInfo);
                    }

                    _accounting.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    throw;
                }
            }
            return userInfo;
        }

        public LegalUserInfoAuthResult? ValidateLegalUserInfo(LegalUserInfoAuthVM infoAuthVM)
        {
            LegalUserInfoAuthResult? model = new();

            IConfigurationRoot? config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            short isActiveInqueries = config.GetValue<short>("ActiveInqueries");

            if (isActiveInqueries == 1)
            {
                try
                {
                    GoldAPIResult? result = new GoldAPIResponse(GoldHosts.Gateway, "/api/Authorization/GetValidateLegalUserInfo", infoAuthVM).Post();

                    if (result != null && !string.IsNullOrEmpty(result.Data))
                        model = JsonConvert.DeserializeObject<LegalUserInfoAuthResult>(result.Data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return model;
            }
            else
                return model;
        }

        public (bool, int) CheckUserSessionBanState(long? userId = 0, string? ip = "")
        {
            int counter = 0;
            int expireInterval = 0;
            bool result = false;

            if (userId == null || ip == null) return (false, 0);

            List<UserSession>? sessions = _accounting.UserSessions
                .Where(x => (x.UserId == userId.Value && userId > 0) || (x.Ip == ip && string.IsNullOrEmpty(ip)))
                .ToList();

            for (int i = 0; i < counter; i++)
            {
                UserSession session = sessions[i];
                bool failSt = session.SessionDate.Date == DateTime.Now.Date && session.Status == -1;
                if (failSt) counter++;
            }

            result = counter >= 3;

            if (counter >= 3) expireInterval = 10;
            else if (counter >= 5) expireInterval = 30;
            else if (counter >= 7) expireInterval = 60;
            else if (counter >= 10) expireInterval = 120;
            else expireInterval = 1440;

            return (result, expireInterval);
        }
    }
}