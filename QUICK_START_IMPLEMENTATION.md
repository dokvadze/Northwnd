# Microservice Modernization - Quick Start Guide

## 1. Environment Setup (Phase 1)

### Create New Solution Structure
```bash
cd /Users/lashadokvadze/Desktop/Northwnd

# Create service directories
mkdir -p Services/ProductService/ProductService.{API,BLL,DAL,Models,Tests}
mkdir -p Services/CategoryService/CategoryService.{API,BLL,DAL,Models,Tests}
mkdir -p Services/RegionService/RegionService.{API,BLL,DAL,Models,Tests}
mkdir -p Gateway/APIGateway.API
mkdir -p Shared
mkdir -p Infrastructure
```

### Create New Solution File
```bash
dotnet new sln -n Northwnd.Microservices
```

### Add Service Projects
```bash
# Product Service
dotnet new webapi -n ProductService.API -o Services/ProductService/ProductService.API
dotnet new classlib -n ProductService.BLL -o Services/ProductService/ProductService.BLL
dotnet new classlib -n ProductService.DAL -o Services/ProductService/ProductService.DAL
dotnet new classlib -n ProductService.Models -o Services/ProductService/ProductService.Models
dotnet new xunit -n ProductService.Tests -o Services/ProductService/ProductService.Tests

# Add to solution
dotnet sln Northwnd.Microservices.sln add Services/ProductService/**/*.csproj
```

---

## 2. Shared Libraries (Phase 1)

### Create Common Package
```bash
dotnet new classlib -n Northwnd.Common -o Shared/Northwnd.Common
dotnet sln Northwnd.Microservices.sln add Shared/Northwnd.Common/Northwnd.Common.csproj
```

### Common/Result.cs - Unified Response Pattern
```csharp
namespace Northwnd.Common;

public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }

    public static Result<T> Ok(T data) => new() { Success = true, Data = data };
    public static Result<T> Fail(string error) => new() { Success = false, Error = error };
}

public class ProblemDetails
{
    public int Status { get; set; }
    public string Title { get; set; }
    public string Detail { get; set; }
    public string Instance { get; set; }
}
```

---

## 3. Product Service Implementation (Phase 2)

### ProjectFile Dependencies
```xml
<!-- ProductService.API.csproj -->
<ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.4.0" />
    <PackageReference Include="Polly" Version="8.2.0" />
</ItemGroup>

<ItemGroup>
    <ProjectReference Include="../ProductService.BLL/ProductService.BLL.csproj" />
    <ProjectReference Include="../../Shared/Northwnd.Common/Northwnd.Common.csproj" />
</ItemGroup>
```

### ProductService/Models/Product.cs
```csharp
namespace ProductService.Models;

public class Product
{
    public int ProductID { get; set; }
    public string ProductName { get; set; }
    public int CategoryID { get; set; }  // Reference only, not a foreign key
    public int SupplierID { get; set; }
    public string? QuantityPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public short UnitsInStock { get; set; }
    public short UnitsOnOrder { get; set; }
    public short ReorderLevel { get; set; }
    public bool Discontinued { get; set; }
    public Guid UniqueId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ProductCreateRequest
{
    public string ProductName { get; set; }
    public int CategoryID { get; set; }
    public int SupplierID { get; set; }
    public string? QuantityPerUnit { get; set; }
    public decimal UnitPrice { get; set; }
    public short UnitsInStock { get; set; }
}
```

### ProductService/DAL/ProductDbContext.cs
```csharp
using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.DAL;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductID);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
            entity.HasIndex(e => e.UniqueId).IsUnique();
        });
    }
}
```

### ProductService/BLL/IProductService.cs
```csharp
using ProductService.Models;
using Northwnd.Common;

namespace ProductService.BLL;

public interface IProductService
{
    Task<Result<List<Product>>> GetAllProductsAsync();
    Task<Result<Product>> GetProductByIdAsync(int productId);
    Task<Result<Product>> CreateProductAsync(ProductCreateRequest request);
    Task<Result<Product>> UpdateProductAsync(int productId, ProductCreateRequest request);
    Task<Result<bool>> DeleteProductAsync(int productId);
}
```

### ProductService/BLL/ProductService.cs
```csharp
using Microsoft.EntityFrameworkCore;
using ProductService.Models;
using ProductService.DAL;
using Northwnd.Common;

namespace ProductService.BLL;

public class ProductServiceImpl : IProductService
{
    private readonly ProductDbContext _context;
    private readonly ILogger<ProductServiceImpl> _logger;

    public ProductServiceImpl(ProductDbContext context, ILogger<ProductServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<Product>>> GetAllProductsAsync()
    {
        var products = await _context.Products.ToListAsync();
        return Result<List<Product>>.Ok(products);
    }

    public async Task<Result<Product>> GetProductByIdAsync(int productId)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productId);
        if (product == null)
            return Result<Product>.Fail($"Product with ID {productId} not found");
        
        return Result<Product>.Ok(product);
    }

    public async Task<Result<Product>> CreateProductAsync(ProductCreateRequest request)
    {
        var product = new Product
        {
            ProductName = request.ProductName,
            CategoryID = request.CategoryID,
            SupplierID = request.SupplierID,
            QuantityPerUnit = request.QuantityPerUnit,
            UnitPrice = request.UnitPrice,
            UnitsInStock = request.UnitsInStock,
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Product created: {ProductId}", product.ProductID);
        
        return Result<Product>.Ok(product);
    }

    public async Task<Result<Product>> UpdateProductAsync(int productId, ProductCreateRequest request)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productId);
        if (product == null)
            return Result<Product>.Fail($"Product with ID {productId} not found");

        product.ProductName = request.ProductName;
        product.UnitPrice = request.UnitPrice;
        product.UnitsInStock = request.UnitsInStock;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Product updated: {ProductId}", productId);
        
        return Result<Product>.Ok(product);
    }

    public async Task<Result<bool>> DeleteProductAsync(int productId)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productId);
        if (product == null)
            return Result<bool>.Fail($"Product with ID {productId} not found");

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Product deleted: {ProductId}", productId);
        
        return Result<bool>.Ok(true);
    }
}
```

### ProductService/API/Program.cs
```csharp
using Microsoft.EntityFrameworkCore;
using ProductService.BLL;
using ProductService.DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductConnection")));

// Business Logic
builder.Services.AddScoped<IProductService, ProductServiceImpl>();

// Logging
builder.Services.AddLogging(config => config.AddConsole());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### ProductService/API/Controllers/ProductsController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using ProductService.BLL;
using ProductService.Models;

namespace ProductService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var result = await _productService.GetAllProductsAsync();
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return result.Success ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ProductCreateRequest request)
    {
        var result = await _productService.CreateProductAsync(request);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.ProductID }, result.Data) : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] ProductCreateRequest request)
    {
        var result = await _productService.UpdateProductAsync(id, request);
        return result.Success ? Ok(result.Data) : NotFound(result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        return result.Success ? NoContent() : NotFound(result.Error);
    }
}
```

### appsettings.json
```json
{
  "ConnectionStrings": {
    "ProductConnection": "Server=localhost;Database=ProductDb;User Id=sa;Password=YourPassword123!;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

---

## 4. Docker Setup (Phase 1)

### Dockerfile (per service)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Services/ProductService/ProductService.API/ProductService.API.csproj", "ProductService.API/"]
RUN dotnet restore "ProductService.API/ProductService.API.csproj"
COPY . .
RUN dotnet build "Services/ProductService/ProductService.API/ProductService.API.csproj" -c Release

FROM build AS publish
RUN dotnet publish "Services/ProductService/ProductService.API/ProductService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.API.dll"]
```

### docker-compose.yml
```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: YourPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  product-service:
    build:
      context: .
      dockerfile: Services/ProductService/ProductService.API/Dockerfile
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__ProductConnection: "Server=sqlserver;Database=ProductDb;User Id=sa;Password=YourPassword123!;"
    ports:
      - "5001:5000"
    depends_on:
      - sqlserver

  category-service:
    build:
      context: .
      dockerfile: Services/CategoryService/CategoryService.API/Dockerfile
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__CategoryConnection: "Server=sqlserver;Database=CategoryDb;User Id=sa;Password=YourPassword123!;"
    ports:
      - "5002:5000"
    depends_on:
      - sqlserver

  region-service:
    build:
      context: .
      dockerfile: Services/RegionService/RegionService.API/Dockerfile
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__RegionConnection: "Server=sqlserver;Database=RegionDb;User Id=sa;Password=YourPassword123!;"
    ports:
      - "5003:5000"
    depends_on:
      - sqlserver

volumes:
  sqlserver_data:
```

### Run Locally
```bash
docker-compose up --build
```

---

## 5. Testing Strategy

### Unit Test Example
```csharp
using Xunit;
using Moq;
using ProductService.BLL;
using ProductService.DAL;
using ProductService.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductService.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateProduct_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
        var context = new ProductDbContext(options);
        var logger = new Mock<ILogger<ProductServiceImpl>>();
        var service = new ProductServiceImpl(context, logger.Object);

        var request = new ProductCreateRequest 
        { 
            ProductName = "Test Product",
            CategoryID = 1,
            SupplierID = 1,
            UnitPrice = 10.0m
        };

        // Act
        var result = await service.CreateProductAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Test Product", result.Data.ProductName);
    }
}
```

---

## 6. Running Migration Commands

```bash
# Add migration
dotnet ef migrations add InitialCreate -p Services/ProductService/ProductService.DAL -s Services/ProductService/ProductService.API

# Update database
dotnet ef database update -p Services/ProductService/ProductService.DAL -s Services/ProductService/ProductService.API
```

---

## 7. Immediate Next Steps

1. ✅ Review MICROSERVICE_MODERNIZATION_STRATEGY.md
2. Create new solution structure per instructions above
3. Implement ProductService following the templates
4. Test locally with docker-compose
5. Repeat for CategoryService and RegionService
6. Build API Gateway

**Estimated Timeline**: 4-6 weeks for full implementation

---

## Key Differences from Current Monolith

| Aspect | Monolith | Microservice |
|--------|----------|------------|
| Database | Single shared DB | Separate DB per service |
| Deployment | Full app deployed | Individual services |
| Scaling | Scale entire app | Scale specific service |
| Tech Stack | All same | Can vary per service |
| Testing | Hard to isolate | Independent unit tests |
| Debugging | Single process | Distributed tracing |
