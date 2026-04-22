# AGENTS.md - AI Agent Guidelines for Northwnd

## Quick Context
**Northwnd** is an ASP.NET Core 10.0 monolithic application managing **Products, Categories, Regions** via a **3-layer architecture**: API → BLL → DAL. Single SQL Server database on path to microservices (see `MICROSERVICE_MODERNIZATION_STRATEGY.md`).

**Runtime**: .NET 10.0 SDK required. **Testing**: xUnit + Moq. **Build**: `dotnet build Northwnd.sln`

---

## The One Thing You Must Understand: Request Flow

```
HTTP Request
    ↓
[ProductController] (Test.API/Controllers/)
    ├─ Interface: IProduct (injected in constructor)
    ↓
[Products BLL] (Northwnd.BLL/Products.cs)
    ├─ Receives: NorthwndDbContext via constructor
    ├─ Methods are ASYNC but often DON'T AWAIT (see Known Issues)
    ↓
[NorthwndDbContext] (Test.DAL/)
    ├─ DbSet<Product>, DbSet<Category>, DbSet<Region>
    ↓
[SQL Server]
```

**KEY**: Each entity (Product/Category/Region) follows the EXACT same pattern across all 3 layers. Copy-paste to add domains.

---

## Critical Patterns & Gotchas

### 1. Dependency Injection (Program.cs)
```csharp
// Test.API/Program.cs - Single source of truth
builder.Services.AddDbContext<NorthwndDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NorthwndConntectionString"))
);
builder.Services.AddScoped<IProduct, Products>();
builder.Services.AddScoped<ICategory, Categories>();
builder.Services.AddScoped<IRegion, Regions>();
```
**Rule**: Always register interfaces → implementations. Forget registration = null reference at runtime.

### 2. Controller Endpoints (NOT REST-style)
```csharp
[HttpGet]
[Route("api/GetProductById/{productId}")]
public async Task<ActionResult> GetProductById(int productId)
{
    var result = await _product.GetProduct(productId);
    return Ok(result);  // Always wrap in Ok()/BadRequest()/NotFound()
}
```
**Convention**: Routes are `api/{ActionName}` not `api/{resource}/{id}`. Return `ActionResult` or `IActionResult`.

### 3. BLL Implementation (⚠️ ASYNC BUG PRESENT)
```csharp
public class Products : IProduct
{
    private NorthwndDbContext _northwndDbContext;
    
    public Products(NorthwndDbContext northwndDbContext)
    {
        _northwndDbContext = northwndDbContext;
    }
    
    // ⚠️ BUG: Marked async Task<T> but returns sync .ToList()
    public async Task<List<Product>> GetProducts() 
        => _northwndDbContext.Products.ToList();
    
    // SHOULD BE:
    // public async Task<List<Product>> GetProducts()
    //     => await _northwndDbContext.Products.ToListAsync();
}
```
**Impact**: Affects ALL services (Products, Categories, Regions). Silent async/sync mismatch causes thread pool starvation under load.

**When fixing**: 
1. Add `.ToListAsync()`, `.FirstOrDefaultAsync()`, `.SingleOrDefaultAsync()` to all EF queries
2. Use `await` for all database operations
3. Search-replace `.ToList()` → `.ToListAsync()` in `Northwnd.BLL/*.cs`

### 4. Models & DTOs (Test.DAL/Models)
```csharp
// Entity (receives from DB & returns to API)
public class Product
{
    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public Guid? UniqueId { get; set; }  // New field
}

// DTO (input for POST/PUT only)
public class ProductRequestModel
{
    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    // No ProductID, UniqueId (server-generated)
}
```
**Rule**: POST/PUT use `*RequestModel`. GET responses use full entity.

### 5. Database Connection String (Typo Preserved)
```json
{
    "ConnectionStrings": {
        "NorthwndConntectionString": "Server=...;Database=Northwind;..."
    }
}
```
Key is `"NorthwndConntectionString"` (missing 'e' in "Connection"). **DO NOT FIX** - breaks legacy apps.

---

## Essential Commands

| Task | Command |
|------|---------|
| Run API (Swagger UI: https://localhost:7123) | `./start-api.sh` or `dotnet run -p Test.API/Northwnd.API.csproj` |
| Run all tests | `dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj --configuration Release` |
| Run single test class | `dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj -k ProductsTests` |
| Add EF migration | `dotnet ef migrations add MigrationName -p Test.DAL/Northwnd.DAL.csproj -s Test.API/Northwnd.API.csproj` |
| Apply migration | `dotnet ef database update -p Test.DAL/Northwnd.DAL.csproj -s Test.API/Northwnd.API.csproj` |
| Build solution | `dotnet build Northwnd.sln` |

---

## Mocking Pattern for Tests (xUnit + Moq)

```csharp
[Fact]
public async Task GetProducts_ReturnsAllProducts()
{
    // Arrange: Create mock data
    var products = new List<Product>
    {
        new Product { ProductID = 1, ProductName = "Product 1" }
    };

    // Arrange: Setup mock DbSet (MUST implement IQueryable)
    var mockSet = new Mock<DbSet<Product>>();
    mockSet.As<IQueryable<Product>>()
        .Setup(m => m.Provider).Returns(products.AsQueryable().Provider);
    mockSet.As<IQueryable<Product>>()
        .Setup(m => m.Expression).Returns(products.AsQueryable().Expression);
    mockSet.As<IQueryable<Product>>()
        .Setup(m => m.ElementType).Returns(products.AsQueryable().ElementType);
    mockSet.As<IQueryable<Product>>()
        .Setup(m => m.GetEnumerator()).Returns(products.AsQueryable().GetEnumerator());

    // Arrange: Setup mock DbContext
    var mockContext = new Mock<NorthwndDbContext>();
    mockContext.SetupGet(d => d.Products).Returns(mockSet.Object);

    // Act
    var service = new Products(mockContext.Object);
    var result = await service.GetProducts();

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.Count);
}
```
**CRITICAL**: Mock DbSet must implement all IQueryable properties (Provider, Expression, ElementType, GetEnumerator).
See `ProductsTests.cs` lines 19-46 for real example.

---

## Project Layout

| Path | Purpose |
|------|---------|
| `Test.API/Program.cs` | DI hub, Swagger config, middleware setup |
| `Test.API/Controllers/*.cs` | HTTP endpoints (ProductController, CategoryController, RegionController) |
| `Northwnd.BLL/{Entity}.cs` | Business logic (Products, Categories, Regions) |
| `Northwnd.BLL/Interfaces/I{Entity}.cs` | Contracts (IProduct, ICategory, IRegion) |
| `Test.DAL/NorthwndDbContext.cs` | EF Core DbContext with 3 DbSets |
| `Test.DAL/Models/{Entity}.cs` | Entity + RequestModel DTOs |
| `Northwnd.UnitTest/{Entity}Tests.cs` | xUnit tests with Moq mocks |
| `Test.Web/` | Separate Razor Pages UI (calls API) |

---

## Namespaces (Misaligned with Folders - Tech Debt)

| Code | Namespace |
|------|-----------|
| Controllers | `Test.API.Controllers` |
| Interfaces | `Northwnd.API.Interfaces` (NOT in Test.API!) |
| BLL implementations | `Northwnd.BLL` |
| DbContext | `Test.DAL` |
| Entity models | `Northwnd.DAL.Models` |

This mismatch is legacy tech debt. When refactoring, consider aligning paths with namespaces.

---

## Adding a New Endpoint (Step-by-Step)

**Goal**: Add `SearchProductsByName(string name)` endpoint

1. **Interface** (`Northwnd.BLL/Interfaces/IProduct.cs`):
   ```csharp
   Task<List<Product>> SearchProductsByName(string name);
   ```

2. **BLL Implementation** (`Northwnd.BLL/Products.cs`):
   ```csharp
   public async Task<List<Product>> SearchProductsByName(string name)
   {
       return await _northwndDbContext.Products
           .Where(p => p.ProductName.Contains(name))
           .ToListAsync();  // IMPORTANT: Use async!
   }
   ```

3. **Controller** (`Test.API/Controllers/ProductController.cs`):
   ```csharp
   [HttpGet]
   [Route("api/SearchProductsByName/{name}")]
   public async Task<ActionResult> SearchProductsByName(string name)
   {
       var result = await _product.SearchProductsByName(name);
       return Ok(result);
   }
   ```

4. **Register in DI** (if new service - not needed for existing `IProduct`):
   Already registered in Program.cs via `builder.Services.AddScoped<IProduct, Products>()`.

5. **Test** (`Northwnd.UnitTest/ProductsTests.cs`):
   Add test following ProductsTests.cs pattern (mock DbSet, mock context, inject, assert).

6. **Verify**: Run `dotnet test` and test via Swagger UI at https://localhost:7123/swagger.

---

## Known Issues & Fixes

### 🔴 High Priority
1. **Async methods don't await EF calls** (ALL services)
   - **Symptom**: Methods return Task but execute synchronously
   - **Fix**: Add `.Async()` suffix to all EF methods (`ToListAsync()`, `FirstOrDefaultAsync()`)
   - **Files**: Northwnd.BLL/Products.cs, Categories.cs, Regions.cs

2. **No .Include() for relationships** (N+1 queries)
   - **Symptom**: Slow queries when accessing related data
   - **Fix**: Use `.Include(p => p.Category)` when loading related entities

3. **No CORS configured**
   - **Symptom**: UI on different port fails with CORS errors
   - **Fix**: Add `builder.Services.AddCors()` in Program.cs

### 🟡 Medium Priority
- Monolithic design prevents independent scaling
- Single database limits multi-tenancy/isolation
- See `MICROSERVICE_MODERNIZATION_STRATEGY.md` for split plan

---

## Architecture Roadmap

| Phase | State | Target |
|-------|-------|--------|
| **Current** | Monolith (.NET 10) on SQL Server | 3 Microservices (ProductAPI, CategoryAPI, RegionAPI) |
| **Databases** | Shared NorthwndDb | Separate ProductDb, CategoryDb, RegionDb |
| **Communication** | In-process DI | REST/gRPC + API Gateway |
| **Testing** | xUnit + Moq DbContext mocks | Integration tests with TestContainers |
| **CI/CD** | GitHub Actions | Container orchestration (Docker/K8s) |

---

## Quick Reference: The 3-Domain Pattern

**Each of Products, Categories, Regions follows this structure:**

```
IProduct ← implemented by → Products (BLL)
    ↓                          ↓
ProductController        uses NorthwndDbContext
    ↓                          ↓
HTTP routes                  Product entity
                                  ↓
                          ProductRequestModel (DTO)
```

**To add a 4th domain (Suppliers)**:
1. Create `Supplier.cs`, `SupplierRequestModel` in Models/
2. Create `Suppliers.cs` BLL class implementing `ISupplier` interface
3. Create `SupplierController.cs` with same endpoint patterns
4. Register in Program.cs: `builder.Services.AddScoped<ISupplier, Suppliers>();`
5. Add `public DbSet<Supplier> Suppliers { get; set; }` to NorthwndDbContext

---

## Before You Code: Checklist

- [ ] `.NET 10.0 SDK` installed (`dotnet --version`)
- [ ] Solution builds: `dotnet build Northwnd.sln`
- [ ] Tests pass: `dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj`
- [ ] API runs: `./start-api.sh` → Swagger UI at https://localhost:7123
- [ ] Existing patterns understood (read ProductsTests.cs and Products.cs)
- [ ] Async/await used correctly (`.ToListAsync()` not `.ToList()`)
- [ ] New services registered in Program.cs BEFORE using
- [ ] XML comments added to public methods (for Swagger docs)

---

**Last Updated**: April 2026 | **Status**: Monolith (Microservices planned) | **Maintainer**: Northwnd Development Team

