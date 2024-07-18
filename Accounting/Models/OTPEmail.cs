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
        [Display(Name = "نام کاربری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string? Username { get; set; }

        [Display(Name = "کد OTP")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [RegularExpression(@"[0-9]{6}", ErrorMessage = "کد OTP نا معتبر می باشد")]
        public long? OTP { get; set; }
    }
}
