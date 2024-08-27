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

    public class UsersList
    {
        public long? UserId { get; set; }
        public string? Username { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Role { get; set; }
        public long? RoleId { get; set; }
        public string? Status { get; set; }
        public int? StatusId { get; set; }
        public string? RegDate { get; set; }
        public long? Mobile { get; set; }
        public long? NationalCode { get; set; }
        public string? Fathername { get; set; }
        public string? Birthday { get; set; }
        public DateTime? FromRegDate { get; set; }
        public DateTime? ToRegDate { get; set; }
    }
}