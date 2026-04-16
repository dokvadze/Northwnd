using Xunit;
using Moq;
using Northwnd.BLL;
using Northwnd.DAL.Models;
using Test.DAL;
using Microsoft.EntityFrameworkCore;

namespace Northwnd.UnitTest
{
    public class ProductsTests
    {
        private Mock<NorthwndDbContext> GetMockDbContext()
        {
            var mockDbContext = new Mock<NorthwndDbContext>();
            return mockDbContext;
        }

        [Fact]
        public async Task GetProducts_ShouldReturnAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductID = 1, ProductName = "Product 1", UniqueId = Guid.NewGuid() },
                new Product { ProductID = 2, ProductName = "Product 2", UniqueId = Guid.NewGuid() }
            };

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.AsQueryable().Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.AsQueryable().Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.AsQueryable().ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.AsQueryable().GetEnumerator());

            var mockDbContext = new Mock<NorthwndDbContext>();
            mockDbContext.Setup(d => d.Products).Returns(mockSet.Object);

            var service = new Products(mockDbContext.Object);

            // Act
            var result = await service.GetProducts();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Product 1", result[0].ProductName);
        }

        [Fact]
        public async Task GetProduct_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var productId = 1;
            var products = new List<Product>
            {
                new Product { ProductID = 1, ProductName = "Product 1", UniqueId = Guid.NewGuid() },
                new Product { ProductID = 2, ProductName = "Product 2", UniqueId = Guid.NewGuid() }
            };

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.AsQueryable().Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.AsQueryable().Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.AsQueryable().ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.AsQueryable().GetEnumerator());

            var mockDbContext = new Mock<NorthwndDbContext>();
            mockDbContext.Setup(d => d.Products).Returns(mockSet.Object);

            var service = new Products(mockDbContext.Object);

            // Act
            var result = await service.GetProduct(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Product 1", result.ProductName);
            Assert.Equal(1, result.ProductID);
        }

        [Fact]
        public async Task AddProduct_WithValidData_ShouldReturnNewProduct()
        {
            // Arrange
            var newProduct = new ProductRequestModel
            {
                ProductName = "New Product",
                SupplierID = 1,
                CategoryID = 1,
                QuantityPerUnit = "10 units",
                UnitPrice = 19.99m,
                UnitsInStock = 100,
                UnitsOnOrder = 0,
                Discontinued = false
            };

            var createdProduct = new Product
            {
                ProductID = 3,
                ProductName = newProduct.ProductName,
                UniqueId = Guid.NewGuid()
            };

            var products = new List<Product>
            {
                new Product { ProductID = 1, ProductName = "Product 1", UniqueId = Guid.NewGuid() }
            };

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.AsQueryable().Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.AsQueryable().Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.AsQueryable().ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.AsQueryable().GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Product>())).Callback<Product>(products.Add);

            var mockDbContext = new Mock<NorthwndDbContext>();
            mockDbContext.Setup(d => d.Products).Returns(mockSet.Object);
            mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new Products(mockDbContext.Object);

            // Act
            var result = await service.AddProduct(newProduct);

            // Assert
            Assert.NotNull(result);
            mockDbContext.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_WithValidId_ShouldCallRemoveAndSave()
        {
            // Arrange
            var productId = 1;
            var product = new Product { ProductID = 1, ProductName = "Product 1", UniqueId = Guid.NewGuid() };

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.Setup(d => d.Find(It.IsAny<object[]>())).ReturnsAsync(product);
            mockSet.Setup(d => d.Remove(It.IsAny<Product>()));

            var mockDbContext = new Mock<NorthwndDbContext>();
            mockDbContext.Setup(d => d.Products).Returns(mockSet.Object);
            mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new Products(mockDbContext.Object);

            // Act
            await service.DeleteProduct(productId);

            // Assert
            mockDbContext.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EditProduct_WithValidData_ShouldUpdateAndReturnProduct()
        {
            // Arrange
            var productId = 1;
            var existingProduct = new Product
            {
                ProductID = 1,
                ProductName = "Old Product",
                UnitPrice = 10m,
                UniqueId = Guid.NewGuid()
            };

            var updatedProductData = new ProductRequestModel
            {
                ProductName = "Updated Product",
                SupplierID = 1,
                CategoryID = 1,
                UnitPrice = 20m,
                QuantityPerUnit = "10 units",
                UnitsOnOrder = 5,
                Discontinued = false
            };

            var products = new List<Product> { existingProduct };
            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.AsQueryable().Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.AsQueryable().Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.AsQueryable().ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.AsQueryable().GetEnumerator());

            var mockDbContext = new Mock<NorthwndDbContext>();
            mockDbContext.Setup(d => d.Products).Returns(mockSet.Object);
            mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new Products(mockDbContext.Object);

            // Act
            var result = await service.EditProduct(productId, updatedProductData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Product", result.ProductName);
            mockDbContext.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
