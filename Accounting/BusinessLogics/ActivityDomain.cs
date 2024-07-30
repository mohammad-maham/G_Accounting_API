using Accounting.BusinessLogics.IBusinessLogics;
using Accounting.Models;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Region> GetProvinceAsync(long provinceId)
        {
            return await _accounting.Regions.FirstOrDefaultAsync(x => x.Id == provinceId && x.Status == 1);
        }

        public async Task<List<Region>> GetProvincesAsync()
        {
            return await _accounting.Regions.Where(x => x.ParentId == null && x.Status == 1).ToListAsync();
        }
    }
}
