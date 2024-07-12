namespace Accounting.Models
{
    public class OTPEmail
    {
        public string? To { get; set; }
        public string? Subject { get; set; }
        public long OTP { get; set; }
    }
}
