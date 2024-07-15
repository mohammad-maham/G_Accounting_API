using System.Collections;

namespace Accounting.Models
{
    public class UserProfile
    {
        public long UserId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateOnly? BirthDay { get; set; }

        public BitArray? Gender { get; set; }

        public string? FatherName { get; set; }
    }
}
