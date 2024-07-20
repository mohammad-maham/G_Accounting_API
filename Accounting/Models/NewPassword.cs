namespace Accounting.Models
{
    public class NewPassword
    {
        public long NationalCode { get; set; }
        public long OTP { get; set; }
        public string? Password { get; set; }
    }
}
