using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Errors;
using Accounting.Helpers;
using Accounting.Models;
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
            long userId = TokenDecryptor.GetUserIdByToken(attributes.Token!);
            if (userId != 0)
            {
                User? user = _users.FindUserById(userId);
                isValid = user != null && user.Id != 0;
            }
            return isValid ? Ok(new ApiResponse(data: "true")) : BadRequest(new ApiResponse(404, data: "false"));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult GetUserInfo([FromBody] AttributesVM attributes)
        {
            long userId = TokenDecryptor.GetUserIdByToken(attributes.Token!);
            UserInfo? userInfo = _users!.FindUserInfo(userId);
            if (userInfo != null && userInfo.Id != 0)
            {
                string jsonData = JsonConvert.SerializeObject(userInfo);
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(404));
        }
    }
}
