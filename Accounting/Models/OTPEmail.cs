namespace Accounting.Models
{
    public class OTPEmail
    {
        public string? Email { get; set; }
        public string? Subject { get; set; }
        public long OTP { get; set; }
        public decimal? Mobile { get; set; }
        public decimal NationalCode { get; set; }
    }

    public partial class OTPInfo
    {
        public DateTime OTPSendDateTime { get; set; } = DateTime.Now;
        public long OTP { get; set; }
        public string? Origin { get; set; }
    }

    public partial class OTPVerify
    {
        public string? Username { get; set; }
        public long? OTP { get; set; }
    }
}
