using System.ComponentModel.DataAnnotations;

namespace Accounting.Models
{
    public class UserContact
    {
        [Display(Name = "شناسه کاربر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long UserId { get; set; }

        [Display(Name = "وضعیت")]
        [Range(0, 1, ErrorMessage = "مقدار {0} می بایست 0 و یا 1 باشد")]
        public short Status { get; set; }

        [Display(Name = "تاریخ ثبت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime RegDate { get; set; }

        [Display(Name = "شناسه منظقه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int RegionId { get; set; }

        [Display(Name = "آدرس")]
        public List<string>? Addresses { get; set; }

        [Display(Name = "تلفن")]
        public List<decimal>? Tells { get; set; }

        [Display(Name = "موبایل")]
        public List<decimal>? Mobiles { get; set; }

    }
}
