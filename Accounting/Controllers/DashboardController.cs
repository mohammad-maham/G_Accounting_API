using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Errors;
using Accounting.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Accounting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboard _dashboard;
        private readonly IUsers _users;

        public DashboardController(ILogger<DashboardController> logger, IDashboard dashboard, IUsers users)
        {
            _logger = logger;
            _dashboard = dashboard;
            _users = users;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GetDashboard(UsersVM user)
        {
            if (user != null && user.UserId != null)
            {
                User? existUser = await _users.FindUserByIdAsync(user.UserId!.Value);
                if (existUser != null && existUser.Id != 0)
                {
                    DashboardVM? dashboard = await _dashboard.GetUserInfoAsync(existUser.Id);
                    string jsonData = JsonConvert.SerializeObject(dashboard);
                    return Ok(new ApiResponse(data: jsonData));
                }
            }
            return BadRequest(new ApiResponse(404));
        }
    }
}
