namespace Accounting.Models
{
    public class UserContact
    {
        public long UserId { get; set; }

        public short Status { get; set; }

        public DateTime RegDate { get; set; }

        public int RegionId { get; set; }

        public List<string>? Addresses { get; set; }

        public List<decimal>? Tells { get; set; }

        public List<decimal>? Mobiles { get; set; }

    }
}
