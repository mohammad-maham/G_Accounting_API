﻿using Accounting.BusinessLogics.IBusinessLogics;
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
        public async Task<IActionResult> SignIn([FromBody] UsersVM usersVM)
        {
            string token = string.Empty;
            if (!string.IsNullOrEmpty(usersVM.Username) && usersVM.Username != "0" && !string.IsNullOrEmpty(usersVM.Password) && usersVM.Password != "0")
            {
                token = await _users.GetSigninAsync(usersVM.Username, usersVM.Password);
                if (!string.IsNullOrEmpty(token))
                {
                    User? user = await _users.FindUserAsync(usersVM.Username, usersVM.Password);
                    if (user != null)
                    {
                        long otp = long.Parse(_auth.GenerateOTP(6));
                        await _auth.SendOTPAsync(user, otp, "Login Verfication", true);
                    }
                }
                else
                {
                    return BadRequest(new ApiResponse(404));
                }
            }
            else
            {
                return BadRequest(new ApiResponse(502, "لطفا پارامتر های ورودی را بازبینی کنید"));
            }

            return Ok(token);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SignUp([FromBody] UserRequest user)
        {
            User? registeredUser = null;
            if (user.NationalCode != 0 && user.Mobile != 0)
            {
                registeredUser = await _users.GetSignupAsync(user);
                if (registeredUser != null && registeredUser.Id != 0 && new int[] { 0, 11, 12 }.Contains(registeredUser.Status))
                {
                    registeredUser.Status = 11; // "Waiting Send OTP"
                    await _users.UpdateUserAsync(registeredUser);
                    long otp = long.Parse(_auth.GenerateOTP(6));
                    await _auth.SendOTPAsync(registeredUser, otp, "Register Verfication", true);
                    string? jsonData = JsonConvert.SerializeObject(new User()
                    {
                        Id = registeredUser.Id,
                        Email = registeredUser.Email,
                        Mobile = registeredUser.Mobile,
                        NationalCode = registeredUser.NationalCode,
                        UserName = registeredUser.UserName
                    });
                    registeredUser.Status = 12; // "Waiting Confirm OTP"
                    await _users.UpdateUserAsync(registeredUser);
                    return Ok(new ApiResponse(data: jsonData));
                }
            }
            return BadRequest(new ApiResponse(502, "با کدملی وارد شده، قبلا کاربری ثبت نام کرده است!"));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ForgotPassword([FromBody] UsersVM usersVM)
        {
            if (!string.IsNullOrEmpty(usersVM.Username) && usersVM.Username != "0")
            {
                User? user = await _users.FindUserAsync(usersVM.Username);
                if (user != null && user.Id != 0)
                {
                    long otp = long.Parse(_auth.GenerateOTP(6));
                    await _auth.SendOTPAsync(user, otp, "Forgot Password Verfication", true);
                    return Ok(new ApiResponse());
                }
            }
            return BadRequest(new ApiResponse(404));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> VerifyOTP([FromQuery] OTPVerify verify)
        {
            if (verify != null && verify.OTP != null && verify.OTP != 0 && !string.IsNullOrEmpty(verify.Username) && verify.Username != "0")
            {
                User? user = await _users.FindUserAsync(verify.Username);
                if (user != null)
                {
                    bool isValid = _auth.VerifyOTPAsync(user, verify.OTP.Value);
                    if (isValid)
                    {
                        user.Status = 1; // "ACTIVE"
                        await _users.UpdateUserAsync(user);
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
        public async Task<IActionResult> SetPassword([FromBody] NewPassword newPassword)
        {
            if (newPassword.NationalCode != 0 && !string.IsNullOrEmpty(newPassword.Password))
            {
                User? user = await _users.FindUserAsync(newPassword.NationalCode.ToString());
                if (user != null)
                {
                    user.Status = 13; // "Waiting Submit Password"
                    await _users.UpdateUserAsync(user);
                    bool isValid = _auth.VerifyOTPAsync(user, newPassword.OTP);
                    if (isValid)
                    {
                        await _users.SetPasswordAsync(newPassword.NationalCode.ToString(), newPassword.Password);
                        user.Status = 1; // "ACTIVATE"
                        await _users.UpdateUserAsync(user);
                        return Ok(new ApiResponse());
                    }
                    else
                    {
                        return BadRequest(new ApiResponse(201));
                    }
                }
            }
            return BadRequest(new ApiResponse(404));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateUser(User user)
        {
            if (user != null && user.Id != 0)
            {
                await _users.UpdateUserAsync(user);
                return Ok(new ApiResponse());
            }
            return BadRequest(new ApiResponse(500));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> CompleteProfile([FromBody] UserProfile profile)
        {
            if (profile != null)
            {
                UserInfo userInfo = await _users.InsertUserInfoAsync(profile);
                string? jsonData = JsonConvert.SerializeObject(userInfo);
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(500));
        }

        [HttpPost]
        [Authorize]
        [Route("[action]")]
        public async Task<IActionResult> SubmitContact([FromBody] UserContact userContact)
        {
            if (userContact != null)
            {
                Contact contact = await _users.InsertUserContactsAsync(userContact);
                string? jsonData = JsonConvert.SerializeObject(contact);
                return Ok(new ApiResponse(data: jsonData));
            }
            return Ok(new ApiResponse());
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SendOTP([FromBody] UsersVM usersVM)
        {
            if (usersVM.UserId != 0)
            {
                long otp = long.Parse(_auth.GenerateOTP(6));
                User? user = await _users.FindUserByIdAsync(usersVM.UserId!.Value);
                if (user != null)
                {
                    await _auth.SendOTPAsync(user, otp, "Verfication Code", true);
                    user.Status = 12; // "Waiting Confirm OTP"
                    await _users.UpdateUserAsync(user);
                    return Ok(new ApiResponse());
                }
            }
            return BadRequest(new ApiResponse(404));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SaveSessionInfo([FromBody] SessionInfo session)
        {
            if (session != null && session.UserId != 0)
            {
                await _users.SaveUserSessionInfo(session);
                return Ok(new ApiResponse());
            }
            return BadRequest(new ApiResponse(500));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateUserStatus([FromBody] UsersVM usersVM)
        {
            if (usersVM.UserId is not null and not 0)
            {
                User? user = await _users.FindUserByIdAsync(usersVM.UserId!.Value);
                if (user != null && user.Id != 0)
                {
                    user.Status = usersVM.Status!.Value;
                    await _users.UpdateUserAsync(user);
                    return Ok(new ApiResponse());
                }
            }
            return BadRequest(new ApiResponse(500));
        }
    }
}
