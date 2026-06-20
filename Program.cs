using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using InventorySaaSBackend.Infrastructure.Data;

using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Application.DTOs;

using InventorySaaSBackend.Infrastructure.Services;
using InventorySaaSBackend.Services;


DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IAlmacenService, AlmacenService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IUnidadService, UnidadService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IExportService, ExportService>();

builder.Services.AddSingleton(Channel.CreateUnbounded<RestockEvent>());

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
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("FrontendPolicy");
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "inventory" }));

app.Run();