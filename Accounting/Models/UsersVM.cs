namespace Accounting.Models
{
    public class UsersVM
    {
        public long? UserId { get; set; }
        public long? NationalCode { get; set; }
        public DateTime? RegDate { get; set; }
        public short? Status { get; set; } = 0;
        public string? Email { get; set; }
        public long? Mobile { get; set; }
        public string? Otpinfo { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class GetUsersVM
    {
        public long? UserId { get; set; }
        public string? Username { get; set; }
    }
}
