﻿using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Errors;
using Accounting.Helpers;
using Accounting.Models;
using Accounting.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using RestSharp;
using System.Globalization;
using System.Net;
using System.Reflection;
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
        public string GetSignin(string username, string password)
        {
            string token = string.Empty;
            User? user = FindUser(username, password);
            if (user != null && user.NationalCode != 0 && new List<int> { 1, 2, 3 }.Contains(user.Status))
                token = _auth.CreateToken(user);
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
            .Where(x => x.ui.Status == 1 && new[] { 11, 12 }.Contains(x.ur.RoleId))
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

        public UserInfo InsertUserInfo(UserProfile profile)
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
            string host = config.GetSection("ApiUrls").GetValue<string>("Gateway")!;
            short isActiveInqueries = config.GetValue<short>("ActiveInqueries");
            if (isActiveInqueries == 1)
            {
                try
                {
                    // BaseURL
                    RestClient client = new($"{host}/api/Authorization/GetValidateMobileNationalCode");
                    RestRequest request = new()
                    {
                        Method = Method.Post
                    };

                    // Parameters
                    request.AddJsonBody(new { Mobile = mobile, NationalCode = nationalCode });

                    // Headers
                    request.AddHeader("content-type", "application/json");
                    request.AddHeader("cache-control", "no-cache");

                    // Send SMS
                    RestResponse response = client.ExecutePost(request);
                    if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
                    {
                        ApiResponse? apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                        if (apiResponse != null && !string.IsNullOrEmpty(apiResponse.Data))
                        {
                            isOk = bool.Parse(apiResponse.Data);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return isOk;
            }
            else
            {
                return true;
            }
        }

        public bool ValidateUserInfo(UserInfoAuthVM infoAuthVM)
        {
            bool isOk = false;
            IConfigurationRoot? config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            string host = config.GetSection("ApiUrls").GetValue<string>("Gateway")!;
            short isActiveInqueries = config.GetValue<short>("ActiveInqueries");
            if (isActiveInqueries == 1)
            {
                try
                {
                    // BaseURL
                    RestClient client = new($"{host}/api/Authorization/GetValidateUserInfo");
                    RestRequest request = new()
                    {
                        Method = Method.Post
                    };

                    // Parameters
                    request.AddJsonBody(infoAuthVM);

                    // Headers
                    request.AddHeader("content-type", "application/json");
                    request.AddHeader("cache-control", "no-cache");

                    // Send SMS
                    RestResponse response = client.ExecutePost(request);
                    if (response != null && response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content))
                    {
                        ApiResponse? apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                        if (apiResponse != null && !string.IsNullOrEmpty(apiResponse.Data))
                        {
                            isOk = bool.Parse(apiResponse.Data);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return isOk;
            }
            else
            {
                return true;
            }
        }
    }
}