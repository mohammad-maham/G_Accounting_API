using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Helpers;
using Accounting.Models;
using GoldHelpers.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Accounting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUsers _users;
        private readonly IAuthentication _auth;

        public UserController(ILogger<UserController> logger, IUsers users, IAuthentication auth)
        {
            _logger = logger;
            _users = users;
            _auth = auth;
        }

        [HttpPost]
        [Route("[action]")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SignIn([FromBody] UsersVM usersVM)
        {
            string token = string.Empty;
            string? username = usersVM.Username ?? usersVM.NationalCode.ToString();
            if (!string.IsNullOrEmpty(username) && username != "0" && !string.IsNullOrEmpty(usersVM.Password) && usersVM.Password != "0")
            {
                token = _users.GetSignin(username!, usersVM.Password);
                if (!string.IsNullOrEmpty(token))
                {
                    User? user = _users.FindUser(username!, usersVM.Password);
                    if (user != null)
                    {
                        /*long otp = long.Parse(_auth.GenerateOTP(6));
                        await _auth.SendOTPAsync(user, otp, "Login Verfication", true);*/
                        return Ok(new GoldAPIResult(data: token));
                    }
                }
                else
                {
                    return BadRequest(new GoldAPIResult(503));
                }
            }
            return BadRequest(new GoldAPIResult(503));
        }

        [HttpPost]
        [Route("[action]")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SignUp([FromBody] UserRequest user)
        {
            User? registeredUser = null;
            if (user.NationalCode != 0 && user.Mobile != 0 && user.Mobile != null)
            {
                bool isValidUser = _users.ValidateMobileNationalCode($"0{user.Mobile.Value}", user.NationalCode.ToString());
                if (isValidUser)
                {
                    registeredUser = _users.GetSignup(user);
                    if (registeredUser != null && registeredUser.Id != 0 && new int[] { 0, 11, 12 }.Contains(registeredUser.Status))
                    {
                        registeredUser.Status = 11; // "Waiting Send OTP"
                        _users.UpdateUser(registeredUser);
                        long otp = long.Parse(_auth.GenerateOTP(6));
                        _auth.SendOTP(registeredUser, otp, "Register Verfication", true);
                        string? jsonData = JsonConvert.SerializeObject(new User()
                        {
                            Id = registeredUser.Id,
                            Email = registeredUser.Email,
                            Mobile = registeredUser.Mobile,
                            NationalCode = registeredUser.NationalCode,
                            UserName = registeredUser.UserName
                        });
                        registeredUser.Status = 12; // "Waiting Confirm OTP"
                        _users.UpdateUser(registeredUser);
                        return Ok(new GoldAPIResult(data: jsonData));
                    }
                }
                else
                {
                    return BadRequest(new GoldAPIResult(502, "کد ملی با شماره همراه مطابقت ندارد"));
                }
            }
            return BadRequest(new GoldAPIResult(502, "با کدملی وارد شده، قبلا کاربری ثبت نام کرده است!"));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult GetUserInfo([FromBody] User? user)
        {
            if (user != null && user.Id != 0)
            {
                UserInfoVM? userInfo = _users.FindFullUserInfo(user.Id);
                string jsonData = JsonConvert.SerializeObject(userInfo);
                return Ok(new GoldAPIResult(data: jsonData));
            }
            return BadRequest(new GoldAPIResult(404));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult GetUserInfoById([FromBody] User? user)
        {
            if (user != null && user.Id != 0)
            {
                User? userInfo = _users.FindUserById(user.Id);
                string jsonData = JsonConvert.SerializeObject(userInfo);
                return Ok(new GoldAPIResult(data: jsonData));
            }
            return BadRequest(new GoldAPIResult(404));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult ForgotPassword([FromBody] UsersVM usersVM)
        {
            if (!string.IsNullOrEmpty(usersVM.Username) && usersVM.Username != "0")
            {
                User? user = _users.FindUser(usersVM.Username);
                if (user != null && user.Id != 0)
                {
                    long otp = long.Parse(_auth.GenerateOTP(6));
                    _auth.SendOTP(user, otp, "Forgot Password Verfication", true);
                    return Ok(new GoldAPIResult());
                }
            }
            return BadRequest(new GoldAPIResult(404));
        }

        [HttpPost]
        [Route("[action]")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult VerifyOTP([FromQuery] OTPVerify verify)
        {
            if (verify != null && verify.OTP != null && verify.OTP != 0 && !string.IsNullOrEmpty(verify.Username) && verify.Username != "0")
            {
                User? user = _users.FindUser(verify.Username);
                if (user != null)
                {
                    bool isValid = _auth.VerifyOTP(user, verify.OTP.Value);
                    if (isValid)
                    {
                        user.Status = 1; // "ACTIVE"
                        _users.UpdateUser(user);
                        return Ok(new GoldAPIResult());
                    }
                    else
                    {
                        return BadRequest(new GoldAPIResult(201));
                    }
                }
            }
            return BadRequest(new GoldAPIResult(401));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult SetPassword([FromBody] NewPassword newPassword)
        {
            if (newPassword.NationalCode != 0 && !string.IsNullOrEmpty(newPassword.Password))
            {
                User? user = _users.FindUser(newPassword.NationalCode.ToString());
                if (user != null)
                {
                    user.Status = 13; // "Waiting Submit Password"
                    _users.UpdateUser(user);
                    bool isValid = _auth.VerifyOTP(user, newPassword.OTP);
                    if (isValid)
                    {
                        _users.SetPassword(newPassword.NationalCode.ToString(), newPassword.Password);
                        user.Status = 1; // "ACTIVATE"
                        _users.UpdateUser(user);
                        return Ok(new GoldAPIResult());
                    }
                    else
                    {
                        return BadRequest(new GoldAPIResult(404, message: "کد تائید صحیح نمی باشد"));
                    }
                }
            }
            return BadRequest(new GoldAPIResult(404));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult UpdateUser([FromBody] User user)
        {
            if (user != null && user.Id != 0)
            {
                _users.UpdateUser(user);
                return Ok(new GoldAPIResult());
            }
            return BadRequest(new GoldAPIResult(500));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult CompleteRealProfile([FromBody] UserProfile profile)
        {
            RealUserInfoAuthVM infoAuthVM = new();
            if (profile != null && profile.UserId != 0)
            {
                User? user = _users.FindUserById(profile.UserId);
                if (user != null && user.Id != 0)
                {
                    infoAuthVM.Name = profile.FirstName;
                    infoAuthVM.Family = profile.LastName;
                    infoAuthVM.BirthDate = profile.BirthDay;
                    infoAuthVM.NationalId = user.NationalCode.ToString();
                    infoAuthVM.Mobile = $"0{user.Mobile}";
                    infoAuthVM.NationalCode = user.NationalCode.ToString();

                    bool isValidUserInfo = _users.ValidateRealUserInfo(infoAuthVM);
                    if (isValidUserInfo)
                    {
                        UserInfo userInfo = _users.InsertRealUserInfo(profile);
                        user.Status = 2; // "COMPLETE-PROFILE"
                        _users.UpdateUser(user);
                        string? jsonData = JsonConvert.SerializeObject(userInfo);
                        return Ok(new GoldAPIResult(data: jsonData));
                    }
                    else
                    {
                        return BadRequest(new GoldAPIResult(400, message: "اطلاعات هویتی مطابقت ندارد!"));
                    }
                }
            }
            return BadRequest(new GoldAPIResult(500));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult CompleteLegalProfile([FromBody] LegalUserInfo profile)
        {
            LegalUserInfoAuthVM infoAuthVM = new();
            if (profile != null && profile.UserId != 0)
            {
                User? user = _users.FindUserById(profile.UserId);
                if (user != null && user.Id != 0)
                {
                    infoAuthVM.NationalCode = user.NationalCode.ToString();

                    LegalUserInfoAuthResult? authResult = _users.ValidateLegalUserInfo(infoAuthVM);
                    bool isOk = authResult != null && authResult.Validation == true && authResult!.NationalId == user.NationalCode.ToString() && authResult!.Name == profile.Name;

                    if (isOk)
                    {
                        LegalUserInfo userInfo = _users.InsertLegalUserInfo(profile);
                        user.Status = 2; // "COMPLETE-PROFILE"
                        _users.UpdateUser(user);
                        string? jsonData = JsonConvert.SerializeObject(userInfo);
                        return Ok(new GoldAPIResult(data: jsonData));
                    }
                    else
                    {
                        return BadRequest(new GoldAPIResult(400, message: "اطلاعات هویتی مطابقت ندارد!"));
                    }
                }
            }
            return BadRequest(new GoldAPIResult(500));
        }

        [HttpPost]
        [Authorize]
        //[UserInfo]
        [Route("[action]")]
        public IActionResult SubmitContact([FromBody] UserContact userContact)
        {
            if (userContact != null && userContact.UserId != 0)
            {
                User? user = _users.FindUserById(userContact.UserId);
                if (user != null)
                {
                    Contact contact = _users.InsertUserContacts(userContact);
                    user.Status = 3; // "SUBMIT-CONTACT"
                    _users.UpdateUser(user);
                    string? jsonData = JsonConvert.SerializeObject(contact);
                    return Ok(new GoldAPIResult(data: jsonData));
                }
            }
            return Ok(new GoldAPIResult());
        }

        [HttpPost]
        [Route("[action]")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult SendOTP([FromBody] UsersVM usersVM)
        {
            if (usersVM.UserId != 0)
            {
                long otp = long.Parse(_auth.GenerateOTP(6));
                User? user = _users.FindUserById(usersVM.UserId!.Value);
                if (user != null)
                {
                    _auth.SendOTP(user, otp, "Verfication Code", true);
                    user.Status = 12; // "Waiting Confirm OTP"
                    _users.UpdateUser(user);
                    return Ok(new GoldAPIResult());
                }
            }
            return BadRequest(new GoldAPIResult(404));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult SaveSessionInfo([FromBody] SessionInfo session)
        {
            if (session != null && session.UserId != 0)
            {
                _users.SaveUserSessionInfo(session);
                return Ok(new GoldAPIResult());
            }
            return BadRequest(new GoldAPIResult(500));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult UpdateUserStatus([FromBody] UsersVM usersVM)
        {
            if (usersVM.UserId is not null and not 0)
            {
                User? user = _users.FindUserById(usersVM.UserId!.Value);
                if (user != null && user.Id != 0)
                {
                    user.Status = usersVM.Status!.Value;
                    _users.UpdateUser(user);
                    return Ok(new GoldAPIResult());
                }
            }
            return BadRequest(new GoldAPIResult(500));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult GetUsers()
        {
            List<GetUsersVM> users = [];
            users = _users.GetUsersList();
            if (users != null && users.Count > 0)
            {
                string jsonData = JsonConvert.SerializeObject(users);
                return Ok(new GoldAPIResult(data: jsonData));
            }
            return BadRequest(new GoldAPIResult(404));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult GetRoles()
        {
            List<Role> roles = [];
            roles = _users.GetRolesList();
            if (roles != null && roles.Count > 0)
            {
                string jsonData = JsonConvert.SerializeObject(roles);
                return Ok(new GoldAPIResult(data: jsonData));
            }
            return BadRequest(new GoldAPIResult(404));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult GetStatuses()
        {
            List<Status> statuses = [];
            statuses = _users.GetStatusesList();
            if (statuses != null && statuses.Count > 0)
            {
                string jsonData = JsonConvert.SerializeObject(statuses);
                return Ok(new GoldAPIResult(data: jsonData));
            }
            return BadRequest(new GoldAPIResult(404));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult ChangeUserRole(UsersRoleVM userRole)
        {
            if (userRole != null && userRole.UserId != null && userRole.UserId != 0 && userRole.RoleId != null && userRole.UserId != 0)
            {
                _users.ChangeUserRole(userRole);
                return Ok(new GoldAPIResult());
            }
            return BadRequest(new GoldAPIResult(404));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Auth([FromBody] UsersVM user)
        {
            bool isOk = false;
            string token = string.Empty;

            bool isInquiery = user.NationalCode != null && user.NationalCode > 0
                && string.IsNullOrEmpty(user.Password);

            bool isCompleteOk = user.NationalCode != null && user.NationalCode > 0
                && !string.IsNullOrEmpty(user.Password);

            if (isCompleteOk)
            {
                token = _users.GetSignin(user.NationalCode.ToString()!, user.Password!, user.IP);
                return Ok(new GoldAPIResult(data: token));
            }
            else if (isInquiery)
            {
                User? findedUser = _users.FindUser(user.NationalCode.ToString()!);
                isOk = findedUser != null && findedUser.Id > 0;
                return Ok(new GoldAPIResult(isOk ? 200 : 404, data: isOk ? "exist" : "not_exists"));
            }

            return BadRequest(new GoldAPIResult(404));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult SendAuthOTP([FromBody] UsersVM user)
        {
            bool isValidUserMobile = false;

            if (user.NationalCode is not null and > 0)
            {
                User? findedUser = _users.FindUser(user.NationalCode.ToString()!);
                bool isExist = findedUser != null && findedUser.Id > 0;

                long? mobile = findedUser?.Mobile;

                if ((mobile != null && mobile > 0) || (user.Mobile != null && user.Mobile > 0))
                {
                    isValidUserMobile = _users.ValidateMobileNationalCode($"0{user.Mobile}", user.NationalCode!.ToString()!);
                    mobile = isValidUserMobile ? user.Mobile : mobile;
                }

                if (isExist && isValidUserMobile)
                {
                    findedUser!.Mobile = mobile;
                    if (findedUser.Mobile is not null and > 0)
                    {
                        long otp = long.Parse(_auth.GenerateOTP(6));
                        _auth.SendOTP(findedUser!, otp, user.Origin ?? "");

                        findedUser.Status = 12; // "Waiting Confirm OTP"
                        _users.UpdateUser(findedUser);
                        return Ok(new GoldAPIResult(isExist ? 200 : 400, data: isExist ? $"sended_otp:{findedUser.Mobile.ToString()!.Substring(findedUser.Mobile.ToString()!.Length - 4)}" : "not_sended_otp"));
                    }
                }
                else if (user.Mobile is not null and > 0 && isValidUserMobile)
                {
                    UserRequest request = new UserRequest() { NationalCode = user.NationalCode!.Value, Mobile = user.Mobile };
                    User? newUser = _users.GetSignup(request);
                    if (newUser != null)
                    {
                        newUser!.Status = 11; // "Waiting Send OTP"
                        _users.UpdateUser(newUser);
                        if (newUser.Mobile is not null and > 0)
                        {
                            long otp = long.Parse(_auth.GenerateOTP(6));
                            _auth.SendOTP(newUser!, otp, user.Origin ?? "");

                            newUser.Status = 12; // "Waiting Confirm OTP"
                            _users.UpdateUser(newUser);
                            return Ok(new GoldAPIResult(newUser != null ? 200 : 400, data: newUser != null ? $"sended_otp:{newUser.Mobile.ToString()!.Substring(newUser.Mobile.ToString()!.Length - 4)}" : "not_sended_otp"));
                        }
                    }
                }
                else
                {
                    if (!isValidUserMobile)
                    {
                        return BadRequest(new GoldAPIResult(400, data: "not_valid_user_mobile", message: "شماره تلفن کاربر با کد ملی آن مطابقت ندارد"));
                    }
                }
            }
            return BadRequest(new GoldAPIResult(404));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult VerifyAuthOTP([FromBody] UsersVM user)
        {
            if (user.NationalCode != null && user.NationalCode > 0 && user.OTP > 0)
            {
                User? findedUser = _users.FindUser(user.NationalCode.ToString()!);
                bool isExist = findedUser != null && findedUser.Id > 0;

                if (isExist)
                {
                    bool isValid = _auth.VerifyOTP(findedUser!, user.OTP!.Value);
                    if (isValid)
                    {
                        string token = _users.GetSignin(user.NationalCode.ToString()!, ip: user.IP);

                        if (!string.IsNullOrEmpty(user.Password))
                        {
                            // match further only if there are two digits anywhere
                            // match further only if there is an upper-lower case letter
                            // match further only if theres anything except letter or digit
                            // match 8 or more characters
                            string regex = @"^(?=(.*\d){2})(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z\d]).{8,}$";
                            Match match = Regex.Match(user.Password!, regex, RegexOptions.IgnoreCase);

                            if (match.Success)
                            {
                                string idCode = IdentificationCodeGen.GenerateCode(findedUser!.NationalCode.ToString());
                                _users.SetPassword(user.NationalCode.ToString()!, user.Password);
                                findedUser.ReferralCode = user.ReferralCode;
                                findedUser.IdentificationCode = idCode;
                                return Ok(new GoldAPIResult(200, data: "setted_password"));
                            }
                            else
                                return BadRequest(new GoldAPIResult(504));
                        }

                        findedUser!.Status = 1; // "ACTIVE"
                        _users.UpdateUser(findedUser!);

                        return Ok(new GoldAPIResult(data: token));
                    }
                    else
                        return BadRequest(new GoldAPIResult(201));
                }
            }
            return BadRequest(new GoldAPIResult(404));
        }
    }
}