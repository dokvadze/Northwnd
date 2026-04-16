# Unit Tests & GitHub Actions Documentation

## Overview

This document describes the comprehensive unit testing and CI/CD setup for the Northwnd project using xUnit, Moq, and GitHub Actions.

## Test Infrastructure

### Test Framework Migration
- **Previous**: MSTest (.NET Framework 4.7.2)
- **Current**: xUnit + Moq (.NET 6.0)
- **Reason**: Better async/await support, isolation capabilities, and alignment with microservices modernization strategy

### Test Project Structure
```
Northwnd.UnitTest/
├── Northwnd.UnitTest.csproj    (modernized to .NET 6.0 with xUnit)
├── ProductsTests.cs             (5 comprehensive test methods)
├── CategoriesTests.cs           (5 comprehensive test methods)
└── RegionsTests.cs              (6 tests for NotImplementedException validation)
```

## Unit Tests Documentation

### ProductsTests.cs
Tests for the `Products` service (IProduct implementation):

1. **GetProducts_ShouldReturnAllProducts**
   - Verifies retrieval of all products from database
   - Mocks DbSet<Product> and validates list count

2. **GetProduct_WithValidId_ShouldReturnProduct**
   - Tests single product retrieval by ProductID
   - Validates product name and ID matching

3. **AddProduct_WithValidData_ShouldReturnNewProduct**
   - Tests product creation with ProductRequestModel
   - Verifies SaveChangesAsync is called
   - Validates new product is persisted

4. **DeleteProduct_WithValidId_ShouldCallRemoveAndSave**
   - Tests product deletion workflow
   - Verifies Remove() and SaveChangesAsync() are called

5. **EditProduct_WithValidData_ShouldUpdateAndReturnProduct**
   - Tests product update with new data
   - Validates property changes and persistence

### CategoriesTests.cs
Tests for the `Categories` service (ICategory implementation):

1. **GetCategories_ShouldReturnAllCategories**
   - Retrieves all categories
   - Validates count and data structure

2. **GetCategory_WithValidId_ShouldReturnCategory**
   - Single category retrieval by CategoryID
   - Validates category name and ID

3. **AddCategory_WithValidData_ShouldReturnNewCategory**
   - Category creation workflow
   - Verifies SaveChangesAsync call

4. **DeleteCategory_WithValidId_ShouldCallRemoveAndSave**
   - Category deletion workflow
   - Verifies removal and save operations

5. **EditCategory_WithValidData_ShouldUpdateAndReturnCategory**
   - Category update workflow
   - Validates property changes

### RegionsTests.cs
Tests for the `Regions` service (IRegion implementation):

**Note**: Regions implementation is incomplete (throws NotImplementedException)

1. **GetRegions_ShouldThrowNotImplementedException**
2. **GetRegion_ShouldThrowNotImplementedException**
3. **AddRegion_ShouldThrowNotImplementedException**
4. **EditRegion_ShouldThrowNotImplementedException**
5. **DeleteRegion_ShouldThrowNotImplementedException**
6. **Regions_Constructor_ShouldAcceptDbContext** (validates constructor injection)

## Running Tests Locally

### Prerequisites
- .NET 6.0 SDK installed
- All NuGet packages restored

### Commands

```bash
# Restore dependencies
dotnet restore Northwnd.sln

# Build solution
dotnet build Northwnd.sln --configuration Release

# Run all tests
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj --configuration Release

# Run specific test class
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj -k "ProductsTests" --configuration Release

# Run tests with code coverage
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj --configuration Release --collect:"XPlat Code Coverage"

# Run tests verbosely
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj --configuration Release --verbosity detailed
```

## GitHub Actions Workflows

Three CI/CD workflows are configured in `.github/workflows/`:

### 1. build.yml - Build Verification
**Triggers**: Push to main/develop, Pull requests to main/develop

**Jobs**:
- Checkout code
- Setup .NET 6.0
- Restore dependencies
- Build solution (Release configuration)
- Verify no build errors

**Status Badge**: Can be added to README for build status

### 2. tests.yml - Unit Test Execution
**Triggers**: Push to main/develop, Pull requests to main/develop

**Jobs**:
- Checkout code
- Setup .NET 6.0
- Restore dependencies
- Build solution
- Run all unit tests
- Generate test reports (using dorny/test-reporter action)

**Outputs**:
- Test results displayed in GitHub UI
- Test report artifacts in TestResults/ directory

### 3. ci-cd.yml - Comprehensive Pipeline
**Triggers**: Push to main/develop, Pull requests to main/develop

**Jobs**:
1. **build-and-test** (parallel execution)
   - Build full solution
   - Run Products tests
   - Run Categories tests
   - Run Regions tests
   - Collect code coverage
   - Upload coverage to Codecov

2. **build-api** (parallel)
   - Build API project independently

3. **build-ui** (parallel)
   - Build UI project independently

**Features**:
- NuGet package caching for faster builds
- Code coverage collection and reporting
- Parallel job execution for performance

## NuGet Dependencies for Tests

```xml
<PackageReference Include="xunit" Version="2.4.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.5.0" />
<PackageReference Include="Moq" Version="4.18.4" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="6.0.1" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="6.0.1" />
```

## Test Configuration

### runsettings.xml
Located at `/Northwnd/runsettings.xml`

Configuration includes:
- Test result directory: `./TestResults`
- Test session timeout: 600 seconds
- Code coverage modules: Includes all Northwnd.* and Test.* assemblies
- Verifiable instrumentation: Enabled for accurate coverage

## Mock Usage Patterns

### DbSet Mocking Example
```csharp
var mockSet = new Mock<DbSet<Product>>();
mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.AsQueryable().Provider);
mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.AsQueryable().Expression);
mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.AsQueryable().ElementType);
mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.AsQueryable().GetEnumerator());
```

### SaveChangesAsync Mocking
```csharp
mockDbContext.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
```

### Verification Example
```csharp
mockDbContext.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
```

## Known Issues & TODO

1. **Missing Awaits in BLL**: Some async methods don't await database operations
   - Impact: Potential race conditions
   - Fix: Update Products.GetProducts() and Categories.GetCategories() to use `.ToListAsync()`

2. **Regions Implementation**: All methods throw NotImplementedException
   - Status: Tests validate this behavior
   - Next: Implement methods as part of microservices modernization

3. **Connection String Typo**: `"NorthwndConntectionString"` (missing 'e')
   - Status: Preserved for backwards compatibility
   - Note: Document during migration

## Integration with Microservices Modernization

When implementing microservices architecture:

1. **Database per Service**: Each microservice will have separate DbContext tests
2. **API Integration Tests**: Add tests for service-to-service communication
3. **Event Handling Tests**: Test event publishing and consumption
4. **Gateway Tests**: Test API Gateway routing and authentication

## Code Coverage Goals

**Target**: 80%+ code coverage for BLL layer

**Current Status**: 
- ProductsTests: Covers all CRUD operations
- CategoriesTests: Covers all CRUD operations
- RegionsTests: Covers constructor and NotImplementedException paths

## Continuous Improvement

### Future Enhancements
1. Add performance benchmarks (BenchmarkDotNet)
2. Add integration tests with real database (TestContainers)
3. Add security tests (OWASP validation)
4. Add load testing (NBomber)
5. Implement test result trending in CI/CD

### Quality Metrics
- Build success rate
- Test pass rate
- Code coverage percentage
- Test execution time

## Troubleshooting

### Tests Fail Locally but Pass in CI
- Check .NET SDK version matches CI (6.0.x)
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Rebuild solution: `dotnet build --no-cache`

### GitHub Actions Workflow Not Triggering
- Verify branch names in workflow (main/develop)
- Check workflow file syntax (YAML format)
- Ensure workflow file is in `.github/workflows/`

### Code Coverage Not Reporting
- Install coverage tool: `dotnet add package coverlet.msbuild`
- Use `/p:CollectCoverage=true` in test command
- Check TestResults/ directory for .cobertura.xml files

## Resources

- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore)
- [Moq Documentation](https://github.com/moq/moq4/wiki)
- [GitHub Actions .NET Documentation](https://github.com/actions/setup-dotnet)
- [Entity Framework Testing Best Practices](https://docs.microsoft.com/en-us/ef/ef6/save/testing)

---

**Last Updated**: April 2026
**Status**: Production-ready for current monolith; plan for microservices enhancement
