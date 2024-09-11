using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Errors;
using Accounting.Helpers;
using Accounting.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
                        return Ok(new ApiResponse(data: token));
                    }
                }
                else
                {
                    return BadRequest(new ApiResponse(400));
                }
            }
            return BadRequest(new ApiResponse(404));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult SignUp([FromBody] UserRequest user)
        {
            User? registeredUser = null;
            if (user.NationalCode != 0 && user.Mobile != 0 && user.Mobile != null)
            {
                bool isValidUser = _users.ValidateMobileNationalCode($"0{user.Mobile.Value.ToString()}", user.NationalCode.ToString());
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
                        return Ok(new ApiResponse(data: jsonData));
                    }
                }
                else
                {
                    return BadRequest(new ApiResponse(502, "کد ملی با شماره همراه مطابقت ندارد"));
                }
            }
            return BadRequest(new ApiResponse(502, "با کدملی وارد شده، قبلا کاربری ثبت نام کرده است!"));
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
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(404));
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
                    return Ok(new ApiResponse());
                }
            }
            return BadRequest(new ApiResponse(404));
        }

        [HttpPost]
        [Route("[action]")]
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
                        return Ok(new ApiResponse());
                    }
                    else
                    {
                        return BadRequest(new ApiResponse(201));
                    }
                }
            }
            return BadRequest(new ApiResponse(401));
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
                        return Ok(new ApiResponse());
                    }
                    else
                    {
                        return BadRequest(new ApiResponse(404, message: "کد تائید صحیح نمی باشد"));
                    }
                }
            }
            return BadRequest(new ApiResponse(404));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult UpdateUser([FromBody] User user)
        {
            if (user != null && user.Id != 0)
            {
                _users.UpdateUser(user);
                return Ok(new ApiResponse());
            }
            return BadRequest(new ApiResponse(500));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult CompleteProfile([FromBody] UserProfile profile)
        {
            if (profile != null)
            {
                UserInfo userInfo = _users.InsertUserInfo(profile);
                string? jsonData = JsonConvert.SerializeObject(userInfo);
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(500));
        }

        [HttpPost]
        [Authorize]
        //[UserInfo]
        [Route("[action]")]
        public IActionResult SubmitContact([FromBody] UserContact userContact)
        {
            if (userContact != null)
            {
                Contact contact = _users.InsertUserContacts(userContact);
                string? jsonData = JsonConvert.SerializeObject(contact);
                return Ok(new ApiResponse(data: jsonData));
            }
            return Ok(new ApiResponse());
        }

        [HttpPost]
        [Route("[action]")]
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
                    return Ok(new ApiResponse());
                }
            }
            return BadRequest(new ApiResponse(404));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult SaveSessionInfo([FromBody] SessionInfo session)
        {
            if (session != null && session.UserId != 0)
            {
                _users.SaveUserSessionInfo(session);
                return Ok(new ApiResponse());
            }
            return BadRequest(new ApiResponse(500));
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
                    return Ok(new ApiResponse());
                }
            }
            return BadRequest(new ApiResponse(500));
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
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(404));
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
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(404));
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
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(404));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public IActionResult ChangeUserRole(UsersRoleVM userRole)
        {
            if (userRole != null && userRole.UserId != null && userRole.UserId != 0 && userRole.RoleId != null && userRole.UserId != 0)
            {
                _users.ChangeUserRole(userRole);
                return Ok(new ApiResponse());
            }
            return BadRequest(new ApiResponse(404));
        }
    }
}