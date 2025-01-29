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
        public IActionResult GetProvinceById([FromBody] long provinceId)
        {
            Region? region = _actDom.GetProvince(provinceId);
            if (region != null && region.Id != 0)
            {
                string jsonData = JsonConvert.SerializeObject(region);
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(400));
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult GetProvinces()
        {
            List<Region> regions = _actDom.GetProvinces();
            if (regions != null && regions.Count > 0)
            {
                string jsonData = JsonConvert.SerializeObject(regions);
                return Ok(new ApiResponse(data: jsonData));
            }
            return BadRequest(new ApiResponse(400));
        }
    }
}
