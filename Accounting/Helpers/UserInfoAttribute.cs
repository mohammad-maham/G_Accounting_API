using Accounting.BusinessLogics;
using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace Accounting.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class UserInfoAttribute : Attribute, IActionFilter
    {
        private readonly IUsers? _users;
        private readonly ILogger<UserInfoAttribute>? _logger;

        public UserInfoAttribute()
        {
            _users ??= (new Users());
            ILoggerFactory? factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            ILogger<UserInfoAttribute>? logger = factory.CreateLogger<UserInfoAttribute>();
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            return;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            string token = string.Empty;
            UserInfo userInfo = new();
            StringValues user = context.HttpContext!.Request.Headers[HeaderNames.Authorization];
            AuthenticationHeaderValue.TryParse(user, out AuthenticationHeaderValue? headerValue);
            if (headerValue != null && headerValue.Parameter != null)
            {
                token = headerValue.Parameter;
                userInfo = _users!.GetUserInfoByToken(token);
            }
            string jsonData = JsonConvert.SerializeObject(userInfo);
            context.HttpContext!.Request.Headers["UserInfo"] = jsonData;
        }
    }
}
