using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;

namespace Accounting.BusinessLogics
{
    public class ActivityDomain : IActivityDomain
    {
        private readonly ILogger<ActivityDomain> _logger;
        private readonly GAccountingDbContext _accounting;

        public ActivityDomain(ILogger<ActivityDomain> logger, GAccountingDbContext context)
        {
            _logger = logger;
            _accounting = context;
        }

        public Region GetProvince(long provinceId)
        {
            return _accounting.Regions.FirstOrDefault(x => x.Id == provinceId && x.Status == 1);
        }

        public List<Region> GetProvinces()
        {
            return _accounting.Regions.Where(x => x.ParentId == null && x.Status == 1).ToList();
        }
    }
}
