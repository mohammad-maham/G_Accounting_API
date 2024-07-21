using System.ComponentModel.DataAnnotations;

namespace Accounting.Models
{
    public class SessionInfo
    {
        [Display(Name = "شناسه کاربری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long UserId { get; set; }

        [Display(Name = "اطلاعات Session")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string? SessionJsonInfo { get; set; }
    }
}
