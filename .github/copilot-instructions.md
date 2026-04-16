# Northwnd Copilot Instructions

## Project Overview

Northwnd is an **ASP.NET Core (.NET 10) 🚀** monolithic application managing products, categories, and regions for the Northwind database. It follows a **3-layer architecture**: API (controllers) → BLL (business logic) → DAL (data access). Currently being modernized to **microservices architecture** (see `MICROSERVICE_MODERNIZATION_STRATEGY.md`).

### ✨ Recent Updates (April 2026)
- **Upgraded to .NET 10.0** with latest compatible packages
- **Enhanced Swagger/OpenAPI** with full documentation
- **Comprehensive unit tests** with xUnit + Moq
- **CI/CD pipelines** with GitHub Actions
- **Production-ready startup scripts** for macOS/Linux/Windows

## Quick Start (2 Minutes)

### Prerequisites
**[⚠️ IMPORTANT]** Install .NET 10.0 SDK from: https://dotnet.microsoft.com/download/dotnet/10.0  
See [DOTNET_10_INSTALLATION.md](../DOTNET_10_INSTALLATION.md) for detailed instructions

### Start the API with Swagger UI

**macOS/Linux:**
```bash
cd /Users/lashadokvadze/Desktop/Northwnd
./start-api.sh
```

**Windows:**
```cmd
cd /Users/lashadokvadze/Desktop/Northwnd
start-api.bat
```

**Manual:**
```bash
dotnet run --project Test.API/Northwnd.API.csproj
```

Then open: **https://localhost:7123** 🎉

### Run Unit Tests
```bash
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj --configuration Release
```

**For complete setup**, see [GETTING_STARTED.md](../GETTING_STARTED.md), [SWAGGER_GUIDE.md](../SWAGGER_GUIDE.md), and [TEST_DOCUMENTATION.md](../TEST_DOCUMENTATION.md)

---

## Architecture & Key Components

### Current Structure (Monolith)
- **Northwnd.API** (`Test.API/`): ASP.NET Core 6.0 REST API with Swagger
  - Controllers: `ProductController`, `CategoryController`, `RegionController`
  - Dependencies: EntityFrameworkCore 6.0.1, SQL Server
  - Key file: `Program.cs` - configures DbContext, DI, Swagger
  
- **Northwnd.BLL** (`Northwnd.BLL/`): Business logic layer
  - Implements: `IProduct`, `ICategory`, `IRegion` interfaces
  - Key pattern: Constructor injection of `NorthwndDbContext`
  - Methods are async but not all return Task correctly (see Performance Notes)
  
- **Northwnd.DAL** (`Test.DAL/`): Data access layer
  - `NorthwndDbContext`: Single DbContext managing Product, Category, Region DbSets
  - Models in `Models/` folder (Product.cs, Category.cs, Region.cs)
  
- **Northwnd.UI** (`Test.Web/`): Razor Pages frontend calling API endpoints

### Database
- Single SQL Server instance with connection string: `"NorthwndConntectionString"` (typo preserved)
- Configuration in `appsettings.json`
- Entities: Product, Category, Region (models defined in DAL)

## Critical Developer Workflows

### Building & Running
```bash
# Build solution (requires .NET 10.0 SDK)
dotnet build Northwnd.sln

# Run API (starts Swagger UI on https://localhost:7xxx)
dotnet run --project Test.API/Northwnd.API.csproj

# Run UI
dotnet run --project Test.Web/Northwnd.UI.csproj

# Run tests
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj
```

### Database Migrations (EF Core)
```bash
# Add migration
dotnet ef migrations add MigrationName -p Test.DAL/Northwnd.DAL.csproj -s Test.API/Northwnd.API.csproj

# Update database
dotnet ef database update -p Test.DAL/Northwnd.DAL.csproj -s Test.API/Northwnd.API.csproj
```

### API Testing
- Swagger UI available at `https://localhost:port/swagger`
- **NEW**: Enhanced with API title, version, contact, and license info
- **NEW**: XML documentation auto-included from code comments
- **NEW**: Try It Out - test endpoints directly from browser
- Endpoints pattern: `api/{resource}` (e.g., `api/GetProducts`, `api/AddProduct`)
- All endpoints use `async Task` but currently lack proper async/await chains

## Code Patterns & Conventions

### 1. Dependency Injection (Program.cs)
```csharp
builder.Services.AddScoped<IProduct, Products>();
builder.Services.AddScoped<ICategory, Categories>();
builder.Services.AddScoped<IRegion, Regions>();
```
**Rule**: Always inject via interfaces; register in Program.cs using `AddScoped`

### 2. BLL Implementation Pattern
```csharp
public class Products : IProduct
{
    private NorthwndDbContext _northwndDbContext;
    
    public Products(NorthwndDbContext northwndDbContext)
    {
        _northwndDbContext = northwndDbContext;
    }
    
    public async Task<List<Product>> GetProducts() 
        => _northwndDbContext.Products.ToList();  // ⚠️ Missing await
}
```
**Convention**: Single underscore prefix for private fields (`_context`); async methods return `Task<T>`

### 3. Controller Pattern
```csharp
[HttpGet]
[Route("api/GetProducts")]
public async Task<ActionResult> GetProducts()
{
    var result = await _product.GetProducts();
    return Ok(result);
}
```
**Convention**: 
- Route naming: `api/{Action}` (not REST-style)
- Always return `ActionResult` or `IActionResult`
- Use `Ok()`, `BadRequest()`, `NotFound()` for responses

### 4. Models & DTOs
- **Entity Models**: `Product`, `Category`, `Region` in DAL (contain Guid, timestamps)
- **Request DTOs**: `ProductRequestModel` (contains only input fields)
- **Convention**: Models are entities (with ID, Guid); RequestModel suffixes for DTOs

## Performance & Known Issues

### ⚠️ Current Problems
1. **Missing awaits**: BLL methods marked `async` but don't await queries
   - `GetProducts()`: Returns `Task<List<Product>>` but executes synchronously
   - **Fix**: Change to `await _context.Products.ToListAsync()`

2. **N+1 queries**: No `.Include()` for related entities (Products don't load Categories)
   - **Fix**: Use `await _context.Products.Include(p => p.CategoryId).ToListAsync()`

3. **Typo in connection string**: `"NorthwndConntectionString"` (should be "ConnectionString")
   - Preserved for backwards compatibility; note when refactoring

4. **CORS not configured**: UI on different port may fail without CORS headers
   - **Fix**: Add `builder.Services.AddCors()` in Program.cs

## Modernization Path (Microservices)

**Current State**: Monolith on .NET 10 with xUnit tests  
**Target State**: 3 independent microservices + API Gateway

For detailed strategy, see `MICROSERVICE_MODERNIZATION_STRATEGY.md` and `QUICK_START_IMPLEMENTATION.md`

### Key Changes When Modernizing
1. Each service gets **separate database** (ProductDb, CategoryDb, RegionDb)
2. Communication via **HTTP REST** between services (no shared DbContext)
3. Add **API Gateway** for routing
4. Implement **event-driven architecture** (event bus)
5. Add **service discovery** (Consul/Eureka)
6. Containerize with **Docker & Kubernetes**

## Common Tasks & How To

### Add a New Endpoint
1. Create method in BLL (e.g., `IProduct.SearchByName()`)
2. Implement in BLL class (e.g., `Products.cs`)
3. Add controller action in `ProductController`
4. Register in DI if new service
5. Add to appsettings if needed
6. Test via Swagger UI

### Modify Entity Properties
1. Edit model in `Test.DAL/Models/Product.cs`
2. Create & apply EF migration
3. Update BLL/Controllers as needed
4. Regenerate SQL Server schema

### Add Unit Tests
- Project: `Northwnd.UnitTest/`
- Currently uses MSTest → migrate to xUnit during modernization
- Pattern: Arrange → Act → Assert

## File Reference Map

| File | Purpose |
|------|---------|
| `Test.API/Program.cs` | API configuration, DI setup, middleware |
| `Test.API/Controllers/*.cs` | HTTP endpoint handlers |
| `Northwnd.BLL/*.cs` | Business logic, interfaces implementations |
| `Test.DAL/NorthwndDbContext.cs` | Entity Framework context |
| `Test.DAL/Models/*.cs` | Entity definitions |
| `Test.Web/Program.cs` | Razor Pages UI setup |
| `appsettings.json` | Connection strings, configuration |
| `.gitignore` | Git ignore patterns (Visual Studio standard) |

## Solution Files

| File | Type |
|------|------|
| `Northwnd.sln` | Solution file (primary) |
| `Northwnd.BLL/Northwnd.BLL.csproj` | BLL project file |
| `Test.API/Northwnd.API.csproj` | API project file |
| `Test.DAL/Northwnd.DAL.csproj` | DAL project file |
| `Test.Web/Northwnd.UI.csproj` | UI project file |
| `Northwnd.UnitTest/Northwnd.UnitTest.csproj` | Test project file |

## Key NuGet Dependencies

- `Microsoft.EntityFrameworkCore` (8.0.0): ORM for data access (EF Core 8 - latest stable)
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.0): SQL Server provider
- `Swashbuckle.AspNetCore` (6.6.2): Swagger/OpenAPI documentation
- `xunit` (2.6.6): Unit testing framework
- `Moq` (4.20.70): Mocking library
- **Runtime**: .NET 10.0 (latest LTS-equivalent)

## Testing Strategy

**Current**: MSTest framework in `Northwnd.UnitTest/`
**During Modernization**: Switch to xUnit + Moq for better async support and isolation

### Example Test Pattern (xUnit)
```csharp
[Fact]
public async Task GetProducts_ReturnsAllProducts()
{
    var mockContext = new Mock<NorthwndDbContext>();
    var service = new Products(mockContext.Object);
    var result = await service.GetProducts();
    Assert.NotNull(result);
}
```

## Git Conventions

- Primary repository: `/Users/lashadokvadze/Desktop/Northwnd/` (.git folder present)
- Standard VisualStudio .gitignore applied
- Branch strategy: Not specified; follow team convention

## Quick Reference: DI Resolution Order

1. **Program.cs** registers services:
   ```csharp
   builder.Services.AddDbContext<NorthwndDbContext>();
   builder.Services.AddScoped<IProduct, Products>();
   ```

2. **Controller constructor** requests dependency:
   ```csharp
   public ProductController(IProduct product)
   ```

3. **Framework injects** at runtime

4. **BLL constructor** receives DbContext:
   ```csharp
   public Products(NorthwndDbContext db)
   ```

---

**Last Updated**: April 2026 | **Status**: Monolith (Microservice migration planned) | See MICROSERVICE_MODERNIZATION_STRATEGY.md for upgrade path
