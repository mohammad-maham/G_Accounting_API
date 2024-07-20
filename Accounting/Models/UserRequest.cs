using System.ComponentModel.DataAnnotations;

namespace Accounting.Models
{
    public class UserRequest
    {
        [Display(Name = "کد ملی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [RegularExpression(@"[0-9]{10}", ErrorMessage = "مقدار {0} نامعتبر می باشد")]
        public long NationalCode { get; set; }

        [Display(Name = "رایانامه")]
        [EmailAddress(ErrorMessage = "لطفا {0} معتبری را وارد کنید")]
        public string? Email { get; set; }

        [Display(Name = "موبایل")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [RegularExpression(@"[0-9]{10}", ErrorMessage = "مقدار {0} نامعتبر می باشد، می بایست بدون صفر و پیش کد وارد گردد")]
        public long? Mobile { get; set; }
    }
}
