﻿using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Errors;
using Accounting.Models;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> SignIn(string username, string password)
        {
            string token = string.Empty;
            if (!string.IsNullOrEmpty(username) && username != "0" && !string.IsNullOrEmpty(password) && password != "0")
            {
                token = await _users.GetSigninAsync(username, password);
                if (!string.IsNullOrEmpty(token))
                {
                    User? user = await _users.FindUserInfoAsync(username, password);
                    if (user != null)
                    {
                        long otp = long.Parse(_auth.GenerateOTP(6));
                        await _auth.SendOTPEmailAsync(new OTPEmail()
                        {
                            OTP = otp,
                            Email = user!.Email,
                            Subject = "Login Verifing",
                            Mobile = user.Mobile,
                            NationalCode = user.NationalCode
                        });
                    }
                }
            }
            else
            {
                return BadRequest(new ApiResponse(502, "Please set parameters!"));
            }

            return Ok(token);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SignUp([FromBody] User user)
        {
            User? registeredUser = null;
            if (user.NationalCode != 0 && user.Mobile != 0)
            {
                registeredUser = await _users.GetSignupAsync(user);
                return Ok(new ApiResponse(200, registeredUser));
            }
            return BadRequest(new ApiResponse(502, "The current user is already registered!"));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ForgotPassword(string username)
        {
            if (!string.IsNullOrEmpty(username) && username != "0")
            {
                User? user = await _users.FindUserInfoAsync(username);
                if (user != null)
                {
                    long otp = long.Parse(_auth.GenerateOTP(6));
                    await _auth.SendOTPEmailAsync(new OTPEmail()
                    {
                        OTP = otp,
                        Email = user!.Email,
                        Subject = "Forgot Password Verifing",
                        Mobile = user.Mobile,
                        NationalCode = user.NationalCode
                    });
                    return Ok();
                }
            }
            return BadRequest(new ApiResponse(404));
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> VerifyOTP([FromBody] OTPVerify verify)
        {
            if (verify != null && verify.OTP != null && verify.OTP != 0 && !string.IsNullOrEmpty(verify.Username) && verify.Username != "0")
            {
                User? user = await _users.FindUserInfoAsync(verify.Username);
                if (user != null)
                {
                    bool isValid = _auth.VerifyOTPAsync(user, verify.OTP.Value);
                    if (isValid) { return Ok(new ApiResponse(200)); }
                }
            }
            return BadRequest(new ApiResponse(401));
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> SetPassword(string username, string password)
        {
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                User? user = await _users.FindUserInfoAsync(username);
                if (user != null)
                {
                    await _users.SetPasswordAsync(username, password);
                    return Ok(new ApiResponse(200));
                }
            }
            return BadRequest(new ApiResponse(500));

        }
    }
}
