using GoldHelpers.Models;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Controllers
{
    [Route("errors/{code}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        public IActionResult Error(int code)
        {
            return new ObjectResult(new GApiResponse<string>() { StatusCode = code });
        }
    }
}