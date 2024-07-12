using Accounting.BusinessLogics.IBusinessLogics;
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
        private readonly IUsers _login;
        private readonly IAuthentication _auth;

        public UserController(ILogger<UserController> logger, IUsers login, IAuthentication auth)
        {
            _logger = logger;
            _login = login;
            _auth = auth;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> SignIn(long NationalCode, long Mobile)
        {
            OTPEmail otpEmail = new();
            string token = await _login.GetSignin(NationalCode, Mobile);
            if (!string.IsNullOrEmpty(token))
            {
                await _auth.SendOTPEmailAsync(otpEmail);
            }

            return Ok(token);
        }

        [HttpPost]
        //[Authorize]
        [Route("[action]")]
        public async Task<IActionResult> SignUp([FromBody] User user)
        {
            User? registeredUser = null;
            if (user.NationalCode != 0 && user.Mobile != 0)
            {
                registeredUser = await _login.GetSignup(user);
                return Ok(new ApiResponse(200, registeredUser));
            }
            else
            {
                return BadRequest(new ApiResponse(502, "The current user is already registered!"))
            }
        }
    }
}
