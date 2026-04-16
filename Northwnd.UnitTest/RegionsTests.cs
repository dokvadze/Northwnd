using Xunit;
using Moq;
using Northwnd.BLL;
using Northwnd.DAL.Models;
using Test.DAL;
using Microsoft.EntityFrameworkCore;

namespace Northwnd.UnitTest
{
    public class RegionsTests
    {
        [Fact]
        public async Task GetRegions_ShouldThrowNotImplementedException()
        {
            // Arrange
            var mockDbContext = new Mock<NorthwndDbContext>();
            var service = new Regions(mockDbContext.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => service.GetRegions());
        }

        [Fact]
        public async Task GetRegion_ShouldThrowNotImplementedException()
        {
            // Arrange
            var mockDbContext = new Mock<NorthwndDbContext>();
            var service = new Regions(mockDbContext.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => service.GetRegion(1));
        }

        [Fact]
        public async Task AddRegion_ShouldThrowNotImplementedException()
        {
            // Arrange
            var mockDbContext = new Mock<NorthwndDbContext>();
            var service = new Regions(mockDbContext.Object);
            var region = new Region { RegionId = 1, RegionDescription = "Test" };

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => service.AddRegion(region));
        }

        [Fact]
        public async Task EditRegion_ShouldThrowNotImplementedException()
        {
            // Arrange
            var mockDbContext = new Mock<NorthwndDbContext>();
            var service = new Regions(mockDbContext.Object);
            var region = new Region { RegionId = 1, RegionDescription = "Test" };

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => service.EditRegion(1, region));
        }

        [Fact]
        public async Task DeleteRegion_ShouldThrowNotImplementedException()
        {
            // Arrange
            var mockDbContext = new Mock<NorthwndDbContext>();
            var service = new Regions(mockDbContext.Object);

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => service.DeleteRegion(1));
        }

        [Fact]
        public void Regions_Constructor_ShouldAcceptDbContext()
        {
            // Arrange
            var mockDbContext = new Mock<NorthwndDbContext>();

            // Act
            var service = new Regions(mockDbContext.Object);

            // Assert
            Assert.NotNull(service);
        }
    }
}
