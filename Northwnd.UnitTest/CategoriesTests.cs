using Xunit;
using Moq;
using Northwnd.BLL;
using Northwnd.DAL.Models;
using Test.DAL;
using Microsoft.EntityFrameworkCore;

namespace Northwnd.UnitTest
{
    public class CategoriesTests
    {
        [Fact]
        public async Task GetCategories_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { CategoryID = 1, CategoryName = "Electronics", UniqueId = Guid.NewGuid() },
                new Category { CategoryID = 2, CategoryName = "Books", UniqueId = Guid.NewGuid() }
            };

            var mockSet = new Mock<DbSet<Category>>();
            mockSet.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(() => categories.AsQueryable().Provider);
            mockSet.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(() => categories.AsQueryable().Expression);
            mockSet.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(() => categories.AsQueryable().ElementType);
            mockSet.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(categories.AsQueryable().GetEnumerator());

            var mockDbContext = new Mock<NorthwndDbContext>();
            mockDbContext.SetupGet(m => m.Categories).Returns(mockSet.Object);

            var service = new Categories(mockDbContext.Object);

            // Act
            var result = await service.GetCategories();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Electronics", result[0].CategoryName);
        }

        [Fact]
        public async Task GetCategory_WithValidId_ShouldReturnCategory()
        {
            // Arrange
            var categoryId = 1;
            var categories = new List<Category>
            {
                new Category { CategoryID = 1, CategoryName = "Electronics", UniqueId = Guid.NewGuid() },
                new Category { CategoryID = 2, CategoryName = "Books", UniqueId = Guid.NewGuid() }
            };

            var mockSet = new Mock<DbSet<Category>>();
            mockSet.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(() => categories.AsQueryable().Provider);
            mockSet.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(() => categories.AsQueryable().Expression);
            mockSet.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(() => categories.AsQueryable().ElementType);
            mockSet.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(categories.AsQueryable().GetEnumerator());

            var mockDbContext = new Mock<NorthwndDbContext>();
            mockDbContext.SetupGet(d => d.Categories).Returns(mockSet.Object);

            var service = new Categories(mockDbContext.Object);

            // Act
            var result = await service.GetCategory(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Electronics", result.CategoryName);
            Assert.Equal(1, result.CategoryID);
        }

        [Fact]
        public async Task AddCategory_WithValidData_ShouldReturnNewCategory()
        {
            // Arrange
            var newCategory = new CategoryRequestModel
            {
                CategoryName = "New Category",
                Description = "Test Description",
                Picture = null
            };

            var categories = new List<Category>();

            var mockSet = new Mock<DbSet<Category>>();
            mockSet.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(() => categories.AsQueryable().Provider);
            mockSet.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(() => categories.AsQueryable().Expression);
            mockSet.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(() => categories.AsQueryable().ElementType);
            mockSet.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(categories.AsQueryable().GetEnumerator());
            mockSet.Setup(d => d.Add(It.IsAny<Category>())).Callback<Category>(categories.Add);

            var mockDbContext = new Mock<NorthwndDbContext>();
            mockDbContext.SetupGet(d => d.Categories).Returns(mockSet.Object);
            mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new Categories(mockDbContext.Object);

            // Act
            var result = await service.AddCategory(newCategory);

            // Assert
            mockDbContext.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCategory_WithValidId_ShouldCallRemoveAndSave()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { CategoryID = 1, CategoryName = "Electronics", UniqueId = Guid.NewGuid() };

            var mockSet = new Mock<DbSet<Category>>();
            mockSet.Setup(d => d.Find(It.IsAny<object[]>())).Returns(category);
            mockSet.Setup(d => d.Remove(It.IsAny<Category>()));

            var mockDbContext = new Mock<NorthwndDbContext>();
            mockDbContext.SetupGet(d => d.Categories).Returns(mockSet.Object);
            mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new Categories(mockDbContext.Object);

            // Act
            await service.DeleteCategory(categoryId);

            // Assert
            mockDbContext.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EditCategory_WithValidData_ShouldUpdateAndReturnCategory()
        {
            // Arrange
            var categoryId = 1;
            var existingCategory = new Category
            {
                CategoryID = 1,
                CategoryName = "Old Name",
                Description = "Old Description",
                UniqueId = Guid.NewGuid()
            };

            var categories = new List<Category> { existingCategory };
            var mockSet = new Mock<DbSet<Category>>();
            mockSet.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(() => categories.AsQueryable().Provider);
            mockSet.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(() => categories.AsQueryable().Expression);
            mockSet.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(() => categories.AsQueryable().ElementType);
            mockSet.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(categories.AsQueryable().GetEnumerator());

            var mockDbContext = new Mock<NorthwndDbContext>();
            mockDbContext.SetupGet(d => d.Categories).Returns(mockSet.Object);
            mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var service = new Categories(mockDbContext.Object);

            var updatedCategory = new CategoryRequestModel
            {
                CategoryName = "Updated Name",
                Description = "Updated Description",
                Picture = null
            };

            // Act
            var result = await service.EditCategory(categoryId, updatedCategory);

            // Assert
            Assert.NotNull(result);
            mockDbContext.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
