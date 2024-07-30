using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IActivityDomain
    {
        Task<Region> GetProvinceAsync(long provinceId);
        Task<List<Region>> GetProvincesAsync();
    }
}