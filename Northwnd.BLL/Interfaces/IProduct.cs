using Northwnd.DAL.Models;

namespace Northwnd.API.Interfaces
{
    public interface IProduct
    {
        Task<List<Product>> GetProducts();
        Task<Product> GetProduct(int productId);
        Task<Product> AddProduct(ProductRequestModel product);
        Task<Product> EditProduct(int productId, ProductRequestModel product);
        Task DeleteProduct(int productId);
    }
}
