namespace Accounting.Errors
{
    public class ApiResponse
    {
        public ApiResponse(int? statusCode = 200, string? message = "", dynamic? data = null)
        {
            StatusCode = statusCode;
            Message = string.IsNullOrEmpty(message) ? GetDefaultMessageForStatusCode(statusCode ?? 200) : message;
            Data = data;
        }

        public int? StatusCode { get; set; } = 200;
        public dynamic? Data { get; set; }
        public string? Message { get; set; }

        private string GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                200 => "success!",
                400 => "bad request!",
                401 => "unauthorized!",
                404 => "data not found!",
                500 => "the system infrastructure has encountered a problem!",
                501 => "verification timeouted!",
                _ => null
            };
        }
    }
}