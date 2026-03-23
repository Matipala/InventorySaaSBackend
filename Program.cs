using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using InventorySaaSBackend.Data;
using InventorySaaSBackend.Modules.Inventario.Application;
using InventorySaaSBackend.Modules.Inventario.Application.Interfaces;
using InventorySaaSBackend.Modules.Inventario.Infrastructure;
using InventorySaaSBackend.Modules.Inventario.Infrastructure.Services;
using InventorySaaSBackend.Modules.Shared.Configuration;
using InventorySaaSBackend.Modules.Ventas.Application;
using InventorySaaSBackend.Modules.Ventas.Infrastructure;
using InventorySaaSBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<SalesOptions>(builder.Configuration.GetSection(SalesOptions.SectionName));

builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IStockGateway, InventarioStockGateway>();
builder.Services.AddScoped<IAlmacenService, AlmacenService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IUnidadService, UnidadService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<ICuentaTicketService, CuentaTicketService>();

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problems = new ValidationProblemDetails(context.ModelState)
        {
            Title = "Validation Failed",
            Status = 400
        };
        return new BadRequestObjectResult(problems);
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();