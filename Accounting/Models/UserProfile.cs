using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Accounting.Models
{
    public class UserProfile
    {
        [Display(Name = "شناسه کاربر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long UserId { get; set; }

        [Display(Name = "نام")]
        [MaxLength(100, ErrorMessage = "حداکثر مقدار {0} 100 کاراکتر می باشد")]
        public string? FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [MaxLength(100, ErrorMessage = "حداکثر مقدار {0} 100 کاراکتر می باشد")]
        public string? LastName { get; set; }

        [Display(Name = "تاریخ تولد")]
        [MaxLength(10, ErrorMessage = "حداکثر مقدار {0} 10 کاراکتر می باشد")]
        public string? BirthDay { get; set; } = DateTime.Now.ToString("yyyy/MM/dd", new CultureInfo("fa-IR"));

        [Display(Name = "جنسیت")]
        [Range(0, 1, ErrorMessage = "مقدار {0} می بایست 0 و یا 1 باشد")]
        public short? Gender { get; set; } = 0;

        [Display(Name = "نام پدر")]
        [MaxLength(100, ErrorMessage = "حداکثر مقدار {0} 100 کاراکتر می باشد")]
        public string? FatherName { get; set; }
    }
}
