using System.ComponentModel.DataAnnotations;

namespace Accounting.Models
{
    public class UserRequest
    {
        [Required, RegularExpression(@"[0-9]{10}", ErrorMessage = "Invalid national code!")]
        public decimal NationalCode { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required, RegularExpression(@"[0-9]{12}", ErrorMessage = "Invalid mobile, must be started (98)!")]
        public decimal? Mobile { get; set; }
    }
}
