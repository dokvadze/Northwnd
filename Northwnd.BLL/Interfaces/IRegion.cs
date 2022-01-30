using Northwnd.DAL.Models;

namespace Northwnd.API.Interfaces
{
    public interface IRegion
    {
        Task<List<Region>> GetRegions();
        Task<Region> GetRegion(int regionId);
        Task<Region> AddRegion(Region region);
        Task<Region> EditRegion(int regionId, Region product);
        Task DeleteRegion(int regionId);
    }
}
