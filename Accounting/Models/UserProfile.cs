using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Accounting.Models
{
    public class UserProfile
    {
        [Required]
        public long UserId { get; set; }

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public string? BirthDay { get; set; }

        [Range(0, 1)]
        public short? Gender { get; set; }

        [MaxLength(100)]
        public string? FatherName { get; set; }
    }
}
