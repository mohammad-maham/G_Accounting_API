using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Errors;
using Accounting.Helpers;
using Accounting.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Accounting.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUsers _users;

        public ReportsController(IUsers users, ILogger<UserController> logger)
        {
            _users = users;
            _logger = logger;
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult GetUsers([FromBody] UsersList filter)
        {
            string jsonData = string.Empty;
            List<UsersList> users = new List<UsersList>();
            if (filter != null)
            {
                users = _users.GetUsersListByFilter(filter);
                if (users != null && users.Count > 0)
                {
                    jsonData = JsonConvert.SerializeObject(users);
                    return Ok(new ApiResponse(data: jsonData));
                }
            }
            return BadRequest(new ApiResponse(404));
        }
    }
}