using System.ComponentModel.DataAnnotations;

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

    public class OTPVerify
    {
        [Required]
        public string? Username { get; set; }

        [Required, RegularExpression(@"[0-9]{6}", ErrorMessage = "Invalid verification code!")]
        public long? OTP { get; set; }
    }
}
