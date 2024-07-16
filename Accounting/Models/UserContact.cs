using System.ComponentModel.DataAnnotations;

namespace Accounting.Models
{
    public class UserContact
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        [Range(0, 1)]
        public short Status { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime RegDate { get; set; }

        [Required]
        public int RegionId { get; set; }

        public List<string>? Addresses { get; set; }

        public List<decimal>? Tells { get; set; }

        public List<decimal>? Mobiles { get; set; }

    }
}
