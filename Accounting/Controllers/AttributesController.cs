using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Helpers;
using Accounting.Models;
using GoldHelpers.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Accounting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttributesController : ControllerBase
    {
        private readonly IUsers _users;
        private readonly IAuthentication _auth;
        private readonly ILogger<AttributesController> _logger;

        public AttributesController(ILogger<AttributesController> logger, IUsers users, IAuthentication auth)
        {
            _users = users;
            _logger = logger;
            _auth = auth;
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult GetAuthorize([FromBody] AttributesVM attributes)
        {
            //bool isValid = await _auth!.VerifyTokenAsync(attributes.Token!, false);
            bool isValid = false;
            if (attributes != null && !string.IsNullOrEmpty(attributes.Token))
            {
                long userId = TokenDecryptor.GetUserIdByToken(attributes.Token);
                if (userId != 0)
                {
                    User? user = _users.FindUserById(userId);
                    bool isTokenValid = _auth.VerifyToken(attributes.Token, false);
                    isValid = user != null && user.Id != 0 && isTokenValid;
                }
                return isValid ? Ok(new GApiResponse<string>() { Data = "true" }) : BadRequest(new GApiResponse<string>() { Data = "false", StatusCode = 401 });
            }
            return BadRequest(new GApiResponse<string>() { Data = "false", StatusCode = 404 });
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult GetUserInfo([FromBody] AttributesVM attributes)
        {
            //UserInfo? userInfo = _users!.FindUserInfo(userId);
            //FullUserInfoVM? userInfo = _users!.GetFindFullUserInfo(userId);
            bool isValid = false;
            if (attributes != null && !string.IsNullOrEmpty(attributes.Token))
            {
                long userId = TokenDecryptor.GetUserIdByToken(attributes.Token);
                User? user = _users.FindUserById(userId);
                if (user != null && user.Id != 0)
                {
                    bool isTokenValid = _auth.VerifyToken(attributes.Token, false);
                    isValid = user != null && user.Id != 0 && isTokenValid;
                    if (isValid)
                    {
                        string jsonData = JsonConvert.SerializeObject(user);
                        return Ok(new GApiResponse<User>() { Data = user });
                    }
                    else
                    {
                        return BadRequest(new GApiResponse<string>() { StatusCode = 401 });
                    }
                }
            }
            return BadRequest(new GApiResponse<string>() { StatusCode = 404 });
        }
    }
}
