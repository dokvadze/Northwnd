using Microsoft.EntityFrameworkCore;
using Northwnd.API.Interfaces;
using Northwnd.BLL;
using Test.DAL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});
builder.Services.AddDbContext<NorthwndDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("NorthwndConntectionString"));
});
builder.Services.AddScoped<IProduct, Products>();
builder.Services.AddScoped<ICategory, Categories>();
builder.Services.AddScoped<IRegion, Regions>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Northwnd API v1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "Northwnd API Documentation";
        c.DefaultModelsExpandDepth(1);
    });
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
