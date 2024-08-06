using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IActivityDomain
    {
        Region GetProvince(long provinceId);
        List<Region> GetProvinces();
    }
}