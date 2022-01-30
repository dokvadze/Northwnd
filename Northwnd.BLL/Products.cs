using Northwnd.API.Interfaces;
using Northwnd.DAL.Models;
using Test.DAL;

namespace Northwnd.BLL
{
    public class Products : IProduct
    {

        private NorthwndDbContext _northwndDbContext;

        public Products(NorthwndDbContext northwndDbContext)
        {
            _northwndDbContext = northwndDbContext;
        }

        public async Task<List<Product>> GetProducts() => _northwndDbContext.Products.ToList();
        public async Task<Product> GetProduct(int productId) => _northwndDbContext.Products.Where(x => x.ProductID == productId).FirstOrDefault();
        public async Task<Product> AddProduct(ProductRequestModel product)
        {
            var newProductIdentifier = Guid.NewGuid();

            var Product = new Product
            {
                ProductName = product.ProductName,
                SupplierID = product.SupplierID,
                CategoryID = product.CategoryID,
                QuantityPerUnit = product.QuantityPerUnit,
                UnitPrice = product.UnitPrice,
                UnitsInStock = product.UnitsInStock,
                UnitsOnOrder = product.UnitsOnOrder,
                Discontinued = product.Discontinued,
                UniqueId = newProductIdentifier
            };

            _northwndDbContext.Add(Product);
            await _northwndDbContext.SaveChangesAsync();
            return _northwndDbContext.Products.Where(x => x.UniqueId == newProductIdentifier).FirstOrDefault();
        }
       

        public async Task<Product> EditProduct(int productId, ProductRequestModel product)
        {
            var Product = await GetProduct(productId);
            Product.ProductName = product.ProductName;
            Product.SupplierID = product.SupplierID;
            Product.CategoryID = product.CategoryID;
            Product.UnitPrice = product.UnitPrice;
            Product.QuantityPerUnit = product.QuantityPerUnit;
            Product.UnitsOnOrder = product.UnitsOnOrder;
            Product.Discontinued = product.Discontinued;


            _northwndDbContext.Products.Update(Product);
            await _northwndDbContext.SaveChangesAsync();

            return await GetProduct(productId);
        }

        public async Task DeleteProduct(int productId)
        {
            var product = GetProduct(productId);
            if (product != null) _northwndDbContext.Products.Remove(product.Result);
            await _northwndDbContext.SaveChangesAsync();
        }


    }
}