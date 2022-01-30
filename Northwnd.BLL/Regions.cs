using Northwnd.API.Interfaces;
using Northwnd.DAL.Models;
using Test.DAL;

namespace Northwnd.BLL
{
    public class Regions : IRegion
    {

        private NorthwndDbContext _northwndDbContext;

        public Regions(NorthwndDbContext northwndDbContext)
        {
            _northwndDbContext = northwndDbContext;
        }

        public Task<Region> AddRegion(Region region)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRegion(int regionId)
        {
            throw new NotImplementedException();
        }

        public Task<Region> EditRegion(int regionId, Region product)
        {
            throw new NotImplementedException();
        }

        public Task<Region> GetRegion(int regionId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Region>> GetRegions()
        {
            throw new NotImplementedException();
        }
    }
}