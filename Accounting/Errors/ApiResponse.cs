namespace Accounting.Errors
{
    public class ApiResponse
    {
        public ApiResponse(int statusCode, dynamic? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }

        public int StatusCode { get; set; }
        public dynamic Message { get; set; }

        private dynamic GetDefaultMessageForStatusCode(int statusCode)
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