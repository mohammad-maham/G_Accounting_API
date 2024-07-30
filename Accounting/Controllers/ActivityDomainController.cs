using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Errors;
using Accounting.Helpers;
using Accounting.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Accounting.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityDomainController : ControllerBase
    {
        private readonly ILogger<ActivityDomainController> _logger;
        private readonly IActivityDomain _actDom;

        public ActivityDomainController(ILogger<ActivityDomainController> logger, IActivityDomain actDom)
        {
            _logger = logger;
            _actDom = actDom;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GetProvinceById([FromBody] long provinceId)
        {
            Region? region = await _actDom.GetProvinceAsync(provinceId);
            if (region != null && region.Id != 0)
            {
                string jsonData = JsonConvert.SerializeObject(region);
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(400));
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GetProvinces()
        {
            List<Region> regions = await _actDom.GetProvincesAsync();
            if (regions != null && regions.Count > 0)
            {
                string jsonData = JsonConvert.SerializeObject(regions);
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(400));
        }
    }
}
