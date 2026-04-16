# Northwnd Microservice Modernization Strategy

## Executive Summary

This document outlines a clean redesign approach to migrate the monolithic Northwnd application into a distributed microservice architecture. The project will be decomposed into independent, scalable services while maintaining API compatibility and data consistency.

---

## Current Architecture Analysis

### Existing Monolith Structure
```
Northwnd.API (ASP.NET Core)
├── Controllers (ProductController, CategoryController, RegionController)
├── Dependencies: BLL + DAL
└── Database: Single SQL Server (NorthwndDbContext)

Northwnd.BLL (Business Logic)
├── Products.cs, Categories.cs, Regions.cs
└── Interfaces (IProduct, ICategory, IRegion)

Northwnd.DAL (Data Access)
├── Models (Product, Category, Region)
└── NorthwndDbContext (EF Core)

Northwnd.UI (Razor Pages)
└── Calls API endpoints

Northwnd.UnitTest
└── Basic test structure (MSTest)
```

### Problems with Monolithic Approach
- **Tight Coupling**: BLL and DAL are tightly coupled through shared models
- **Single Database**: All entities share one DB context - limits independent scaling
- **Deployment Risk**: Any change requires full application redeployment
- **Technology Lock-in**: All services use same tech stack
- **Testing Challenges**: Unit tests are limited due to hard dependencies
- **Scalability**: Cannot scale individual business domains independently

---

## Target Microservice Architecture

### Service Breakdown (3 Independent Services)

#### 1. **Product Service** (ProductAPI)
- **Responsibility**: Manage products, pricing, inventory
- **Models**: Product
- **Database**: Dedicated ProductDb (SQL Server or alternative)
- **API Endpoints**:
  - `GET /api/products`
  - `GET /api/products/{id}`
  - `POST /api/products`
  - `PUT /api/products/{id}`
  - `DELETE /api/products/{id}`

#### 2. **Category Service** (CategoryAPI)
- **Responsibility**: Manage product categories
- **Models**: Category
- **Database**: Dedicated CategoryDb
- **API Endpoints**:
  - `GET /api/categories`
  - `GET /api/categories/{id}`
  - `POST /api/categories`
  - `PUT /api/categories/{id}`
  - `DELETE /api/categories/{id}`

#### 3. **Region Service** (RegionAPI)
- **Responsibility**: Manage geographical regions
- **Models**: Region
- **Database**: Dedicated RegionDb
- **API Endpoints**:
  - `GET /api/regions`
  - `GET /api/regions/{id}`
  - `POST /api/regions`
  - `PUT /api/regions/{id}`
  - `DELETE /api/regions/{id}`

### API Gateway / BFF Layer (Optional but Recommended)
- **Purpose**: Single entry point for UI and external clients
- **Responsibilities**:
  - Route requests to appropriate microservices
  - Aggregate multi-service calls (e.g., Products with Categories)
  - Handle authentication/authorization centrally
  - Rate limiting, caching
- **Technology**: ASP.NET Core middleware or dedicated gateway (Kong, AWS API Gateway)

---

## Service Communication Patterns

### Synchronous Communication (Recommended for Initial Migration)
```
UI/Client → API Gateway → Individual Services (REST/gRPC)
                         → Service-to-Service calls (REST)
```

**Approach**: 
- Services call each other via HTTP REST APIs
- Product Service can query Category Service when needed
- Simple to implement and debug

### Asynchronous Communication (Phase 2)
```
Event Producer (Service A) → Message Broker (RabbitMQ/Azure Service Bus)
                           ↓
                        Event Subscriber (Service B)
```

**Use Cases**:
- Product updated → Publish event → Other services react
- Inventory changed → Audit service logs event

---

## Data Management Strategy

### Database Per Service Pattern
```
ProductAPI
└── ProductDB (dedicated SQL Server database)

CategoryAPI
└── CategoryDB (dedicated SQL Server database)

RegionAPI
└── RegionDB (dedicated SQL Server database)
```

### Data Consistency Approach
1. **Strong Consistency**: Use distributed transactions (Saga pattern) if cross-service transactions needed
2. **Eventual Consistency**: Event-driven updates between services
3. **Shared Read Model**: Optional - create denormalized views for complex queries

### Migration Data Management
- **Phase 1**: Keep single DB, deploy services pointwise
- **Phase 2**: Database per service with data replication
- **Phase 3**: Full separation with event synchronization

---

## Proposed Project Structure

```
Northwnd/
├── Services/
│   ├── ProductService/
│   │   ├── ProductService.API/
│   │   ├── ProductService.BLL/
│   │   ├── ProductService.DAL/
│   │   ├── ProductService.Models/
│   │   └── ProductService.Tests/
│   ├── CategoryService/
│   │   ├── CategoryService.API/
│   │   ├── CategoryService.BLL/
│   │   ├── CategoryService.DAL/
│   │   ├── CategoryService.Models/
│   │   └── CategoryService.Tests/
│   └── RegionService/
│       ├── RegionService.API/
│       ├── RegionService.BLL/
│       ├── RegionService.DAL/
│       ├── RegionService.Models/
│       └── RegionService.Tests/
├── Gateway/
│   ├── APIGateway.API/
│   └── APIGateway.Tests/
├── Shared/
│   ├── Northwnd.Common/  (DTOs, Constants, Utilities)
│   └── Northwnd.Events/  (Event contracts for async communication)
├── UI/
│   └── Northwnd.UI/
├── Tests/
│   ├── Integration.Tests/
│   └── E2E.Tests/
└── Infrastructure/
    ├── docker-compose.yml
    ├── kubernetes/
    └── ci-cd/
```

---

## Migration Roadmap

### Phase 1: Foundation (Weeks 1-2)
- [ ] Create shared NuGet packages (Northwnd.Common, Northwnd.Events)
- [ ] Set up Docker and docker-compose
- [ ] Create API Gateway scaffold
- [ ] Set up CI/CD pipeline
- [ ] Create database backup

**Deliverable**: Local dev environment with all services runnable via docker-compose

### Phase 2: Product Service (Weeks 3-4)
- [ ] Create ProductService project structure
- [ ] Implement ProductService.DAL with dedicated ProductDB
- [ ] Implement ProductService.BLL with IProduct interface
- [ ] Implement ProductService.API with controllers
- [ ] Add integration tests
- [ ] Migrate data from monolith to ProductDB
- [ ] Deploy to staging

**Deliverable**: Fully functional Product microservice

### Phase 3: Category & Region Services (Weeks 5-6)
- [ ] Repeat Phase 2 for CategoryService (same process)
- [ ] Repeat Phase 2 for RegionService (same process)
- [ ] Test inter-service communication (e.g., Product queries Category)

**Deliverable**: All three independent microservices running

### Phase 4: API Gateway & UI Integration (Week 7)
- [ ] Implement API Gateway
- [ ] Update Razor Pages UI to call gateway instead of monolith
- [ ] Add authentication to gateway
- [ ] Performance testing and optimization

**Deliverable**: UI fully integrated with microservices

### Phase 5: Advanced Features (Week 8+)
- [ ] Implement event-driven architecture (RabbitMQ/Service Bus)
- [ ] Add service discovery (Consul/Kubernetes)
- [ ] Implement distributed tracing (Application Insights, Jaeger)
- [ ] Add circuit breakers (Polly)
- [ ] Decommission monolith

**Deliverable**: Production-ready microservice architecture

---

## Technology Stack Decisions

### Core Framework
- **ASP.NET Core 8.0** (from current 6.0 - migrate gradually)
- Keep .NET/C# as primary language for consistency

### Data Access
- **Entity Framework Core 8.0** - per service
- **SQL Server** - continue current DB (phase 1), split later (phase 2+)
- Option: **Dapper** for read-heavy operations in Product Service

### Communication
- **HTTP/REST** - initial inter-service communication
- **gRPC** - optional for performance-critical services
- **RabbitMQ** or **Azure Service Bus** - future async messaging

### API Management
- **Swagger/OpenAPI** - maintain current approach
- **API Gateway**: ASP.NET middleware or **Azure API Management**

### Testing
- **xUnit** - upgrade from MSTest (industry standard)
- **Moq** - mocking framework
- **TestContainers** - for integration tests with real DB containers
- **SpecFlow** - BDD for acceptance tests

### Deployment
- **Docker** - containerize each service
- **Docker Compose** - local development
- **Kubernetes** (or Azure Container Instances) - production
- **GitHub Actions** - CI/CD pipeline

### Monitoring & Observability
- **Application Insights** - centralized logging
- **Distributed Tracing** - track requests across services
- **Health Checks** - ASP.NET Core health check endpoints

---

## Key Implementation Guidelines

### 1. API Design
```csharp
// Each service owns its domain models
// Example: ProductService/Models/Product.cs
public class Product 
{
    public int ProductID { get; set; }
    public string ProductName { get; set; }
    public int CategoryID { get; set; }  // Reference only, not FK
    public decimal UnitPrice { get; set; }
    // ... other properties
}

// DTOs for inter-service communication
public class CategoryReferenceDto
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; }
}
```

### 2. Database Strategy
```sql
-- ONE DATABASE PER SERVICE
-- ProductDB: Contains only Product table
-- CategoryDB: Contains only Category table
-- RegionDB: Contains only Region table

-- No foreign keys across databases
-- Services communicate via APIs to fetch related data
```

### 3. Error Handling
- Each service implements consistent error responses
- HTTP status codes: 200, 201, 400, 404, 500, etc.
- Structured error response format (problem details RFC 7231)

### 4. Unified Logging
```csharp
// Use structured logging in all services
var logger = LoggerFactory.Create(builder => 
    builder.AddApplicationInsights()
           .AddConsole())
    .CreateLogger<ProductService>();

logger.LogInformation("Product {ProductId} created by {UserId}", 
    productId, userId);
```

### 5. Dependency Injection
- Use Microsoft.Extensions.DependencyInjection
- Configure in Program.cs (ASP.NET Core 6+ pattern)
- Enable mocking for tests

---

## Critical Success Factors

1. **Database Independence**: Services MUST have separate databases by Phase 3
2. **API Contracts**: Define clear, stable API contracts early
3. **Testing Strategy**: Start with unit tests, progress to integration and e2e
4. **Backwards Compatibility**: API Gateway should support old endpoints during transition
5. **Documentation**: Keep OpenAPI/Swagger specs updated
6. **Team Alignment**: Clear ownership of each service (Product team, Category team, etc.)

---

## Risk Mitigation

| Risk | Mitigation Strategy |
|------|-------------------|
| Data inconsistency across services | Use Saga pattern for distributed transactions; implement eventual consistency |
| Service failures cascade | Implement circuit breakers (Polly), timeouts, retry logic |
| Performance degradation | API Gateway caching, database indexing, gRPC for hot paths |
| Deployment complexity | Full automation via CI/CD, canary deployments in prod |
| Debugging difficulties | Centralized logging, distributed tracing, correlation IDs |

---

## Rollback Strategy

1. **Keep monolith running in parallel** during Phase 1-4
2. **Blue-green deployment**: Run both versions simultaneously
3. **Feature flags**: Control which requests go to microservices vs. monolith
4. **Data replication**: Keep monolith DB in sync until Phase 5
5. **Quick-switch option**: Route all traffic back to monolith if issues detected

---

## Success Metrics

- ✅ All services deployable independently
- ✅ Service uptime > 99.5%
- ✅ Average API response time < 200ms
- ✅ Zero breaking changes to client contracts
- ✅ CI/CD pipeline with automated testing
- ✅ Centralized monitoring and alerting in place

---

## Next Steps

1. **Review & Approve**: Get stakeholder sign-off on this strategy
2. **Create Project Skeleton**: Set up folder structure and docker-compose
3. **Phase 1 Sprint**: Foundation work (1-2 weeks)
4. **Begin Phase 2**: Start with Product Service
5. **Iterate**: After each phase, collect feedback and adjust approach

---

## References & Resources

- [Microsoft: Microservices Architecture](https://docs.microsoft.com/en-us/azure/architecture/guide/architecture-styles/microservices)
- [Saga Pattern for Distributed Transactions](https://microservices.io/patterns/data/saga.html)
- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure)
- [Docker for .NET Developers](https://docs.docker.com/language/dotnet/)
- [Kubernetes Deployment Guide](https://kubernetes.io/docs/)
