namespace Accounting.Errors
{
    public class ApiResponse
    {
        public ApiResponse(int? statusCode = 200, string? message = "", string? data = null)
        {
            StatusCode = statusCode;
            Message = string.IsNullOrEmpty(message) ? GetDefaultMessageForStatusCode(statusCode ?? 200) : message;
            Data = data;
        }

        public int? StatusCode { get; set; } = 200;
        public string? Data { get; set; }
        public string? Message { get; set; }

        private string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                200 => "عملیات با موفقیت انجام یافت",
                201 => "کد تائیدیه صحیح نمی باشد و یا منقضی شده است",
                400 => "درخواست قابل پردازش نمی باشد!",
                401 => "درخواست فاقد اعتبار معتبر می باشد!",
                404 => "داده یافت نشد!",
                500 => "زیرساخت سیستم با مشکل مواجه شده است!",
                501 => "زمان تأییدیه به پایان رسیده است!",
                _ => null
            };
        }
    }
}