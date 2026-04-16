# AGENTS.md - AI Agent Guidelines for Northwnd

## Quick Context
**Northwnd** is an ASP.NET Core 10.0 monolithic application on the path to microservices. It manages three domains (Products, Categories, Regions) via a 3-layer architecture: API → BLL → DAL. Single SQL Server database. **Actively modernizing** to microservices (see `MICROSERVICE_MODERNIZATION_STRATEGY.md`).

---

## Architecture: The 3-Layer Pattern

**You MUST understand this flow to work effectively:**

```
HTTP Request
    ↓
[ProductController] (Test.API/Controllers/)
    ↓ (depends on: IProduct interface)
[Products BLL] (Northwnd.BLL/Products.cs) - business logic here
    ↓ (depends on: NorthwndDbContext)
[NorthwndDbContext] (Test.DAL/) - queries Products DbSet
    ↓
[SQL Server]
```

**Key insight**: Each domain has 3 implementations (Products/Categories/Regions) following the EXACT same pattern. Copy-paste one, adjust names.

---

## Critical Code Locations & Patterns

### 1. **Dependency Injection Hub** → `Test.API/Program.cs`
- **Single source of truth** for all service registration
- **Pattern**: `builder.Services.AddScoped<IInterface, Implementation>()`
- **When adding features**: Register new BLL classes here FIRST
- **Common mistake**: Forgetting to register → runtime null reference

### 2. **Controller Pattern** → `Test.API/Controllers/*.cs`
- **Route format**: `[Route("api/GetProducts")]` NOT REST style
- **Always return**: `ActionResult` or `IActionResult` (not raw objects)
- **Injection**: Interface in constructor (e.g., `IProduct product`)
- **Example endpoint**:
  ```csharp
  [HttpGet]
  [Route("api/GetProductById/{productId}")]
  public async Task<ActionResult> GetProductById(int productId)
  {
      var result = await _product.GetProduct(productId);
      return Ok(result);
  }
  ```

### 3. **BLL Service Pattern** → `Northwnd.BLL/{Entity}.cs`
- **Receives** `NorthwndDbContext` in constructor
- **Private field**: `private NorthwndDbContext _northwndDbContext;`
- **⚠️ CRITICAL BUG**: Methods marked `async Task<T>` but **DON'T AWAIT** queries
  - Current: `return _northwndDbContext.Products.ToList();` (synchronous)
  - Should be: `return await _northwndDbContext.Products.ToListAsync();`
  - **Affects ALL services** (Products, Categories, Regions)
  - **Your fix strategy**: Add `.Async()` to all EF calls
- **DTO separation**: Use `ProductRequestModel` for POST/PUT inputs; `Product` entity for responses

### 4. **DbContext & Models** → `Test.DAL/`
- **Single context**: `NorthwndDbContext` manages all 3 DbSets
- **Models live here**: `Test.DAL/Models/{Entity}.cs`
- **Each model has entity + RequestModel** (DTO twin)
- **Database typo preserved**: Connection string key is `"NorthwndConntectionString"` (not "ConnectionString") - DO NOT FIX (backward compat)

### 5. **Tests** → `Northwnd.UnitTest/`
- **Framework**: xUnit (not MSTest, despite legacy packages)
- **Pattern**: Mock DbContext → inject into service → assert
- **Mock setup complexity**: Must mock DbSet + IQueryable interface for tests to work
- **Example**: See `ProductsTests.cs` lines 19-45 for correct mock pattern

---

## Startup & Debugging Commands

| Task | Command |
|------|---------|
| Build solution | `dotnet build Northwnd.sln` |
| Run API (Swagger on https://localhost:7123) | `./start-api.sh` OR `dotnet run --project Test.API/Northwnd.API.csproj` |
| Run tests | `dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj --configuration Release` |
| Create migration | `dotnet ef migrations add MigrationName -p Test.DAL/Northwnd.DAL.csproj -s Test.API/Northwnd.API.csproj` |
| Update database | `dotnet ef database update -p Test.DAL/Northwnd.DAL.csproj -s Test.API/Northwnd.API.csproj` |

---

## Common Development Tasks

### Adding a New Endpoint
1. Create method in `Interfaces/IEntity.cs` (e.g., `Task<T> SearchByName(string name)`)
2. Implement in `Northwnd.BLL/Entity.cs` (remember: **use `.ToListAsync()` not `.ToList()`**)
3. Add controller action in `Test.API/Controllers/EntityController.cs`
4. Register in `Program.cs` if it's a NEW service (not needed for existing ones)
5. Test via Swagger UI at `https://localhost:7123/swagger`

### Modifying an Entity Model
1. Edit `Test.DAL/Models/Entity.cs` and `EntityRequestModel`
2. Create migration: `dotnet ef migrations add PropertyName`
3. Update database: `dotnet ef database update`
4. Verify SQL Server schema matches

### Running Specific Tests
```bash
# Single test class
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj -k ProductsTests

# Single test method
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj -k "GetProducts_ShouldReturnAllProducts"
```

---

## Project Structure (Map to Domains)

| Folder/File | Purpose | Key Files |
|---|---|---|
| `Test.API/` | REST API + Swagger | `Program.cs` (DI), `Controllers/*.cs` (endpoints) |
| `Northwnd.BLL/` | Business logic | `Products.cs`, `Categories.cs`, `Regions.cs`, `Interfaces/*.cs` |
| `Test.DAL/` | Data access | `NorthwndDbContext.cs`, `Models/*.cs` |
| `Test.Web/` | Razor Pages UI | Calls API endpoints (separate frontend) |
| `Northwnd.UnitTest/` | Tests | xUnit + Moq mocks; test against mocked DbContext |

---

## Performance & Known Issues

### High-Priority Fixes (Blocking)
1. **Missing .Async()**: All `.ToList()` → `.ToListAsync()` in BLL
2. **No .Include()**: Related entities not loaded (N+1 query risk)
   - Example: Loading Products should `.Include(p => p.Category)` 
3. **No CORS configured**: UI may fail if on different port

### Architecture Debt
- Monolithic design prevents independent scaling
- Single database limits isolation between domains
- **Modernization plan**: Split into 3 microservices (ProductAPI, CategoryAPI, RegionAPI) with separate databases + API Gateway

---

## Testing Strategy

**Current approach**: Mock the `NorthwndDbContext`, inject into BLL service, verify calls and returns.

```csharp
// Mock setup pattern (from ProductsTests.cs)
var mockSet = new Mock<DbSet<Product>>();
mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.AsQueryable().Provider);
// ... set Expression, ElementType, GetEnumerator similarly

var mockDbContext = new Mock<NorthwndDbContext>();
mockDbContext.Setup(d => d.Products).Returns(mockSet.Object);

var service = new Products(mockDbContext.Object);
var result = await service.GetProducts();
Assert.NotNull(result);
```

---

## Interfaces You'll Encounter

| Interface | Location | Implementations |
|---|---|---|
| `IProduct` | `Northwnd.BLL/Interfaces/IProduct.cs` | `Products` class |
| `ICategory` | `Northwnd.BLL/Interfaces/ICategory.cs` | `Categories` class |
| `IRegion` | `Northwnd.BLL/Interfaces/IRegion.cs` | `Regions` class |

Each interface defines CRUD + query methods (GetAll, GetById, Add, Edit, Delete patterns).

---

## Namespace Organization

- **API namespace**: `Test.API.Controllers`
- **BLL namespaces**: `Northwnd.API.Interfaces` (interfaces), `Northwnd.BLL` (implementations)
- **DAL namespaces**: `Test.DAL` (context), `Northwnd.DAL.Models` (entities)
- **Note**: Namespace names don't perfectly align with folder structure (tech debt from legacy refactoring)

---

## Before You Code: Checklist

- [ ] Read `Program.cs` to understand DI registration
- [ ] Check if feature already exists in a service (copy pattern)
- [ ] **Use `.ToListAsync()` + `await` in BLL** (not sync `.ToList()`)
- [ ] Add XML comments to new public methods (Swagger docs)
- [ ] Register new services in `Program.cs` before using
- [ ] Test via Swagger UI after deployment
- [ ] Run unit tests: `dotnet test` before committing

---

## Version Info & Future Roadmap

- **Current**: .NET 10.0 monolith on SQL Server
- **Target**: Microservices (3 independent services + API Gateway)
- **Key files tracking modernization**: `MICROSERVICE_MODERNIZATION_STRATEGY.md`, `QUICK_START_IMPLEMENTATION.md`
- **CI/CD**: GitHub Actions (workflows in `.github/workflows/`)

---

**Last updated**: April 2026 | **Maintained by**: Northwnd Development Team

