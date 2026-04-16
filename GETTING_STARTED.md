# Northwnd API - Getting Started Guide

## Prerequisites

Before running the Northwnd API, ensure you have the following installed:

- **.NET 10.0 SDK** or later ([Download](https://dotnet.microsoft.com/download/dotnet/10.0)) 🆕
  - See [DOTNET_10_INSTALLATION.md](./DOTNET_10_INSTALLATION.md) for detailed installation instructions
- **SQL Server** (Express or higher) or connection to remote instance
- **Git** (for version control)

## Installation Steps

### 1. Verify .NET Installation
```bash
dotnet --version
# Should output: 10.0.xxx or higher
```

### 2. Clone or Navigate to Project
```bash
cd /Users/lashadokvadze/Desktop/Northwnd
```

### 3. Restore Dependencies
```bash
dotnet restore Northwnd.sln
```

### 4. Build Solution
```bash
dotnet build Northwnd.sln --configuration Release
```

or for development:
```bash
dotnet build Northwnd.sln
```

## Starting the API Server

### Development Mode (Recommended)
```bash
# Start the API with Swagger UI at root
dotnet run --project Test.API/Northwnd.API.csproj

# Output will show:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7123
#       Now listening on: http://localhost:5123
#       Application started. Press Ctrl+C to exit.
```

### Production Mode
```bash
dotnet run --project Test.API/Northwnd.API.csproj --configuration Release
```

### With Watch Mode (Auto-reload on code changes)
```bash
dotnet watch --project Test.API/Northwnd.API.csproj run
```

## Accessing Swagger UI

Once the API is running:

### 1. Local Development
- **Swagger UI**: https://localhost:7123 (or http://localhost:5123)
- **Swagger JSON**: https://localhost:7123/swagger/v1/swagger.json
- **API Root**: https://localhost:7123/api/

### 2. API Endpoints

#### Products
- `GET /api/GetProducts` - Retrieve all products
- `GET /api/GetProductById/{productId}` - Get product by ID
- `POST /api/AddProduct` - Create new product
- `PUT /api/UpdateProduct` - Update existing product
- `DELETE /api/DeleteProduct` - Delete product

#### Categories
- `GET /api/GetCategories` - Retrieve all categories
- `GET /api/GetCategoryById/{categoryId}` - Get category by ID
- `POST /api/AddCategory` - Create new category
- `PUT /api/UpdateCategory` - Update category
- `DELETE /api/DeleteCategory` - Delete category

#### Regions
- `GET /api/GetRegions` - Retrieve all regions
- `GET /api/GetRegionById/{regionId}` - Get region by ID
- `POST /api/AddRegion` - Create new region
- `PUT /api/UpdateRegion` - Update region
- `DELETE /api/DeleteRegion` - Delete region

## Swagger UI Features

### What's Enabled
✅ **API Documentation** - Full endpoint descriptions with request/response models  
✅ **Try It Out** - Test endpoints directly from the browser  
✅ **Parameter Validation** - See required/optional fields  
✅ **Response Examples** - View success and error responses  
✅ **XML Comments** - Auto-generated from code documentation  

### Navigation
1. **Models** - View request/response data structures
2. **Authorize** - Add authentication tokens (if configured)
3. **Try it out** - Expand endpoint and modify request
4. **Execute** - Send request and view response
5. **Responses** - See status codes and response bodies

## Testing API Endpoints

### Using Swagger UI (Browser)
1. Navigate to https://localhost:7123
2. Click "Try it out" on an endpoint
3. Modify parameters if needed
4. Click "Execute"
5. View response below

### Using curl (Command Line)
```bash
# Get all products
curl -X GET "https://localhost:7123/api/GetProducts" -H "accept: application/json"

# Create a product
curl -X POST "https://localhost:7123/api/AddProduct" \
  -H "Content-Type: application/json" \
  -d '{
    "productName": "New Product",
    "supplierID": 1,
    "categoryID": 1,
    "quantityPerUnit": "10 units",
    "unitPrice": 19.99,
    "unitsInStock": 100,
    "unitsOnOrder": 0,
    "discontinued": false
  }'
```

### Using Postman
1. Open Postman
2. Import Swagger URL: `https://localhost:7123/swagger/v1/swagger.json`
3. Collections are auto-created with all endpoints
4. Test each endpoint with sample data

## Configuration

### appsettings.json
Located at: `Test.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "NorthwndConntectionString": "Server=.;Database=Northwind;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### appsettings.Development.json
Located at: `Test.API/appsettings.Development.json`

Use this for local development settings (overrides appsettings.json)

## Database Setup

### SQL Server Connection String Format
```
Server=SERVERNAME;Database=Northwind;Trusted_Connection=true;
Server=SERVERNAME\SQLEXPRESS;Database=Northwind;Trusted_Connection=true;
Server=localhost;Database=Northwind;User Id=sa;Password=YourPassword;
```

### Create Database with Migrations
```bash
# Add migration (if needed)
dotnet ef migrations add InitialCreate \
  -p Test.DAL/Northwnd.DAL.csproj \
  -s Test.API/Northwnd.API.csproj

# Update database
dotnet ef database update \
  -p Test.DAL/Northwnd.DAL.csproj \
  -s Test.API/Northwnd.API.csproj
```

## Troubleshooting

### Port Already in Use
```
Error: The port 7123 is already in use
```

**Solution:**
```bash
# Use a different port
dotnet run --project Test.API/Northwnd.API.csproj -- --urls="https://localhost:7124;http://localhost:5124"
```

### SSL Certificate Error
```
error: System.InvalidOperationException: Unable to configure HTTPS endpoint
```

**Solution:**
```bash
# Generate development certificate
dotnet dev-certs https --trust

# Then run normally
dotnet run --project Test.API/Northwnd.API.csproj
```

### Database Connection Error
```
Error: A network-related or instance-specific error occurred while establishing a connection to SQL Server
```

**Solution:**
1. Verify SQL Server is running
2. Check connection string in appsettings.json
3. Validate server name and credentials
4. Ensure database exists

### Swagger Not Loading
```
Swagger UI loads but shows no endpoints
```

**Solution:**
1. Verify API is running (console shows "Now listening on...")
2. Clear browser cache (Ctrl+Shift+Delete)
3. Hard refresh (Ctrl+F5)
4. Check browser console for CORS errors

## Running Tests

### Run All Unit Tests
```bash
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj
```

### Run Specific Test Class
```bash
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj -k "ProductsTests"
```

### Run with Code Coverage
```bash
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj --collect:"XPlat Code Coverage"
```

## Running UI (Razor Pages)

In a separate terminal:
```bash
dotnet run --project Test.Web/Northwnd.UI.csproj
```

Access at: https://localhost:7124 (or port shown in console)

## Useful Commands

```bash
# List all available endpoints
dotnet run --project Test.API/Northwnd.API.csproj -- --help-headers

# Run with detailed logging
dotnet run --project Test.API/Northwnd.API.csproj --environment Development

# Kill process on port (macOS/Linux)
lsof -ti:7123 | xargs kill -9

# Kill process on port (Windows)
netstat -ano | findstr :7123
taskkill /PID <PID> /F
```

## API Response Format

### Success Response (200 OK)
```json
[
  {
    "productID": 1,
    "productName": "Chai",
    "supplierID": 1,
    "categoryID": 1,
    "quantityPerUnit": "10 boxes x 20 bags",
    "unitPrice": 18.00,
    "unitsInStock": 39,
    "unitsOnOrder": 0,
    "reorderLevel": 10,
    "discontinued": false,
    "uniqueId": "550e8400-e29b-41d4-a716-446655440000"
  }
]
```

### Error Response (400/500)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid product ID",
  "instance": "/api/GetProductById/-1"
}
```

## Next Steps

1. **Test endpoints** in Swagger UI
2. **Review documentation** at `/swagger`
3. **Run unit tests** to ensure functionality
4. **Configure database** with production data
5. **Set up CI/CD** using GitHub Actions (see `.github/workflows/`)

## Documentation References

- [Swagger/OpenAPI Documentation](https://swagger.io/docs/)
- [ASP.NET Core Swagger Documentation](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [SQL Server Connection Strings](https://www.connectionstrings.com/sql-server/)

## Support

For issues or questions:
1. Check [TEST_DOCUMENTATION.md](./TEST_DOCUMENTATION.md) for test setup
2. Review [copilot-instructions.md](./.github/copilot-instructions.md) for architecture details
3. Check GitHub Issues for reported problems
4. Consult team documentation

---

**Last Updated**: April 2026  
**Status**: Ready for local development and testing
