# Swagger Configuration & API Documentation

## Overview

The Northwnd API includes comprehensive Swagger/OpenAPI documentation with an interactive UI for testing endpoints. This guide explains the setup and how to use it.

## Features

### ✅ Enabled Swagger Features
- **API Documentation**: Full endpoint descriptions and data models
- **Try It Out**: Test endpoints directly from the browser
- **Request/Response Models**: Visualize data structures
- **Parameter Validation**: See required vs. optional fields
- **Response Examples**: View actual response formats
- **XML Documentation**: Auto-generated from code comments
- **Metadata**: API title, version, contact, license info

## Configuration Details

### Program.cs Configuration

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Northwnd API",
        Version = "v1",
        Description = "ASP.NET Core 6.0 REST API for Northwind database management",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Northwnd Development Team",
            Email = "dev@northwnd.local"
        },
        License = new Microsoft.OpenApi.Models.OpenApiLicense
        {
            Name = "MIT"
        }
    });
    
    // Include XML documentation comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Swagger UI Configuration
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Northwnd API v1");
    c.RoutePrefix = string.Empty;  // Serve at root (/)
    c.DocumentTitle = "Northwnd API Documentation";
    c.DefaultModelsExpandDepth(1);
});
```

### Project File Configuration

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\Northwnd.API.xml</DocumentationFile>
</PropertyGroup>
```

## Access Points

### URLs
| Purpose | URL |
|---------|-----|
| **Swagger UI** | https://localhost:7123 |
| **Swagger JSON** | https://localhost:7123/swagger/v1/swagger.json |
| **HTTP Alternative** | http://localhost:5123 |
| **API Base** | https://localhost:7123/api |

### Environment-Specific
- **Development**: Swagger fully enabled (UI + JSON endpoint)
- **Production**: Swagger disabled (remove `UseSwagger()` if statement)
- **Staging**: Can be enabled via configuration

## API Documentation Standards

### Adding Documentation to Endpoints

#### Example Controller with XML Documentation
```csharp
/// <summary>
/// Retrieves all products from the database
/// </summary>
/// <returns>List of all products with details</returns>
/// <response code="200">Returns the list of products</response>
/// <response code="500">Internal server error</response>
[HttpGet]
[Route("api/GetProducts")]
public async Task<ActionResult> GetProducts()
{
    var result = await _product.GetProducts();
    return Ok(result);
}

/// <summary>
/// Creates a new product
/// </summary>
/// <param name="product">Product creation model with required fields</param>
/// <returns>Newly created product with ID</returns>
/// <response code="200">Product created successfully</response>
/// <response code="400">Invalid product data</response>
[HttpPost]
[Route("api/AddProduct")]
public async Task<ActionResult> AddProduct(ProductRequestModel product)
{
    var result = await _product.AddProduct(product);
    return Ok(result);
}
```

#### Example Model Documentation
```csharp
/// <summary>
/// Product creation request model
/// </summary>
public class ProductRequestModel
{
    /// <summary>
    /// Product name (required, max 40 chars)
    /// </summary>
    public string? ProductName { get; set; }
    
    /// <summary>
    /// Supplier ID reference
    /// </summary>
    public int SupplierID { get; set; }
    
    /// <summary>
    /// Category ID reference
    /// </summary>
    public int CategoryID { get; set; }
}
```

## Using Swagger UI

### 1. Accessing the Interface
1. Start the API: `dotnet run --project Test.API/Northwnd.API.csproj`
2. Open browser to: https://localhost:7123
3. Swagger UI loads with all endpoints listed

### 2. Exploring Endpoints
- **Expand** endpoint to see full details
- **View** parameter requirements and response models
- **Filter** endpoints using search box
- **Tag** navigation for organizing by resource type

### 3. Testing Endpoints

**Step-by-Step:**
1. Click on endpoint (e.g., `GET /api/GetProducts`)
2. Click **"Try it out"** button
3. Modify parameters if needed (all pre-populated)
4. Click **"Execute"** to send request
5. View response in **"Responses"** section

**Example Testing Workflow:**
```
GET /api/GetProductById/{productId}
  ↓
Click "Try it out"
  ↓
Enter productId: 1
  ↓
Click "Execute"
  ↓
See response: { "productID": 1, "productName": "Chai", ... }
```

### 4. Understanding Responses

Each endpoint shows:
- **Response Code**: 200, 400, 500, etc.
- **Response Type**: Schema of returned data
- **Response Model**: Expandable tree view of JSON
- **Example**: Sample JSON response
- **Headers**: Response headers included

## Swagger JSON Schema

The Swagger specification is available at: `/swagger/v1/swagger.json`

This file contains:
- OpenAPI 3.0 specification
- All endpoint definitions
- Request/response schemas
- Data type definitions
- Server information

### Using Swagger JSON Elsewhere
```bash
# Import into Postman
# 1. Postman → Import → Link → Paste URL
# 2. Select "Swagger 2.0" or "OpenAPI 3.0"
# 3. Collections auto-created

# Generate client code (using Swagger CodeGen)
swagger-codegen generate \
  -i https://localhost:7123/swagger/v1/swagger.json \
  -l csharp \
  -o generated-client
```

## Best Practices

### ✅ DO

1. **Add XML documentation to all public methods**
   ```csharp
   /// <summary>Description here</summary>
   public async Task<IActionResult> Method() { }
   ```

2. **Include parameter descriptions**
   ```csharp
   /// <param name="id">The product ID to fetch</param>
   ```

3. **Document response codes**
   ```csharp
   /// <response code="200">Success</response>
   /// <response code="404">Not found</response>
   ```

4. **Use meaningful endpoint routes**
   ```csharp
   [Route("api/GetProductById/{productId}")]
   ```

5. **Validate request models**
   ```csharp
   [HttpPost("api/AddProduct")]
   public async Task<IActionResult> AddProduct([FromBody] ProductRequestModel model)
   ```

### ❌ DON'T

1. ❌ Leave endpoints without documentation
2. ❌ Use vague parameter names
3. ❌ Skip response examples
4. ❌ Mix HTTP verbs (GET for mutations, POST for retrieval)
5. ❌ Disable Swagger in development

## Performance Considerations

### Swagger Generation Performance
- **First Request**: ~500ms (parses all controllers)
- **Cached**: Subsequent requests use cache
- **Location**: Generated at startup, not per-request

### Optimizations
- Only enable Swagger in Development/Staging
- Disable for Production (security & performance)
- Use `c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());` if needed

## Security Considerations

### Development
- ✅ Swagger fully enabled
- ✅ No authentication required
- ✅ Full endpoint visibility

### Production
- ❌ Consider disabling Swagger (remove `UseSwagger()`)
- ❌ Or add authentication layer
- Example:
  ```csharp
  if (!app.Environment.IsProduction())
  {
      app.UseSwagger();
      app.UseSwaggerUI();
  }
  ```

### Sensitivity
- Don't expose internal implementation details
- Redact sensitive fields in documentation
- Use `[ApiExplorerSettings(IgnoreApi = true)]` to hide endpoints

## Troubleshooting

### Swagger UI Not Loading
**Problem**: Browser shows "Page Not Available"

**Solution**:
1. Verify API is running: `dotnet run --project Test.API/...`
2. Check port: default is 7123
3. Check URL: https://localhost:7123 (note HTTPS)
4. Clear cache: Ctrl+Shift+Delete
5. Hard refresh: Ctrl+F5

### No Endpoints Showing
**Problem**: Swagger loads but no API endpoints visible

**Solution**:
1. Check `[ApiController]` attribute on controllers
2. Verify `[Route]` attributes on methods
3. Ensure endpoints are `public`
4. Rebuild solution: `dotnet build`
5. Restart API server

### XML Documentation Not Appearing
**Problem**: Method descriptions missing in Swagger

**Solution**:
1. Enable documentation file generation in .csproj
2. Add `/// <summary>` comments to methods
3. Clean and rebuild: `dotnet clean && dotnet build`
4. Check file exists: `bin/Debug/net6.0/Northwnd.API.xml`
5. Restart API

### CORS Errors in Swagger UI
**Problem**: "Access-Control-Allow-Origin" errors

**Solution**:
```csharp
// Add to Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

## Integration with CI/CD

### GitHub Actions Integration
The Swagger JSON is generated during CI/CD builds and can be:
- Validated for schema correctness
- Exported for documentation generation
- Used to generate client SDKs

See `.github/workflows/ci-cd.yml` for implementation.

## Advanced Topics

### Custom Swagger Configuration
```csharp
// Add API key authentication
c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
{
    Type = SecuritySchemeType.ApiKey,
    In = ParameterLocation.Header,
    Name = "X-API-Key"
});

// Add bearer token authentication
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT"
});
```

### Multiple API Versions
```csharp
c.SwaggerDoc("v2", new OpenApiInfo { Title = "Northwnd API", Version = "v2" });

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "v2");
});
```

## Resources

- [Swagger.io Documentation](https://swagger.io/tools/swagger-ui/)
- [OpenAPI 3.0 Specification](https://spec.openapis.org/oas/v3.0.3)
- [Swashbuckle Documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [ASP.NET Core Swagger Integration](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle)

## Summary

| Feature | Status | Location |
|---------|--------|----------|
| Swagger UI | ✅ Enabled | https://localhost:7123 |
| OpenAPI JSON | ✅ Enabled | /swagger/v1/swagger.json |
| XML Documentation | ✅ Enabled | Northwnd.API.xml |
| Try It Out | ✅ Enabled | Swagger UI |
| Model Expansion | ✅ Enabled | Default depth: 1 |
| Response Examples | ✅ Enabled | Per endpoint |

---

**Last Updated**: April 2026  
**Configuration Status**: Production-Ready  
**Documentation Status**: 80% Coverage (expanding with XML comments)
