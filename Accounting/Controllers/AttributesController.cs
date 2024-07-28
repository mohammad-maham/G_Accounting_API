using Accounting.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Accounting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttributesController : ControllerBase
    {
        [HttpGet]
        [Route("[action]")]
        public void GetAuthorize([FromBody] AuthorizationFilterContext context)
        {
            AuthorizeAttribute authorizeAttribute = new();
            authorizeAttribute.OnAuthorization(context);
        }

        [HttpGet]
        [Route("[action]")]
        public void GetUserInfo([FromBody] ActionExecutingContext context)
        {
            UserInfoAttribute userInfoAttribute = new();
            userInfoAttribute.OnActionExecuting(context);
        }
    }
}
