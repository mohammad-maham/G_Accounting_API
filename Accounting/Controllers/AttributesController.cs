using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Errors;
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
        public async Task<IActionResult> GetAuthorize([FromBody] AttributesVM attributes)
        {
            bool isValid = await _auth!.VerifyTokenAsync(attributes.Token!, false);
            return isValid ? Ok(new ApiResponse(data: "true")) : BadRequest(new ApiResponse(404, data: "false"));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GetUserInfo([FromBody] AttributesVM attributes)
        {
            UserInfo? userInfo = await _users!.GetUserInfoByToken(attributes.Token!);
            if (userInfo != null && userInfo.Id != 0)
            {
                string jsonData = JsonConvert.SerializeObject(userInfo);
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(404));
        }
    }
}
