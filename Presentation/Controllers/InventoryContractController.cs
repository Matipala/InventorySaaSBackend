using InventorySaaSBackend.Application.DTOs.Contract;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Models;
using InventorySaaSBackend.Services;
using InventorySaaSBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySaaSBackend.Presentation.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryContractController : ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly IInventarioService _inventarioService;
    private readonly IEmpresaService _empresaService;
    private readonly IAlmacenService _almacenService;
    private readonly ICategoriaService _categoriaService;
    private readonly IUnidadService _unidadService;
    private readonly ApplicationDbContext _context;

    public InventoryContractController(
        IProductoService productoService,
        IInventarioService inventarioService,
        IEmpresaService empresaService,
        IAlmacenService almacenService,
        ICategoriaService categoriaService,
        IUnidadService unidadService,
        ApplicationDbContext context)
    {
        _productoService = productoService;
        _inventarioService = inventarioService;
        _empresaService = empresaService;
        _almacenService = almacenService;
        _categoriaService = categoriaService;
        _unidadService = unidadService;
        _context = context;
    }

    #region Helpers
    private Guid ResolveCompanyId(string companyCen)
    {
        if (Guid.TryParse(companyCen, out Guid id)) return id;
        throw new BadHttpRequestException($"Invalid CompanyCen: {companyCen}. Must be a valid UUID.");
    }

    private Guid ResolveWarehouseId(string warehouseCen)
    {
        if (Guid.TryParse(warehouseCen, out Guid id)) return id;
        throw new BadHttpRequestException($"Invalid WarehouseCen: {warehouseCen}. Must be a valid UUID.");
    }

    private async Task<Guid> ResolveProductId(string productCen, Guid idEmpresa)
    {
        if (Guid.TryParse(productCen, out Guid id)) return id;
        var productos = await _productoService.BuscarFiltrados(idEmpresa, productCen, null, null, null);
        var prod = productos.FirstOrDefault(p => string.Equals(p.Sku, productCen, StringComparison.OrdinalIgnoreCase));
        if (prod != null) return prod.IdProducto;
        throw new BadHttpRequestException($"Invalid ProductCen: {productCen}. Not found as UUID or SKU.");
    }
    #endregion

    [HttpGet("companies")]
    public async Task<ActionResult<IEnumerable<CompanyContractDto>>> GetCompanies()
    {
        var empresas = await _empresaService.ObtenerTodos();
        return Ok(empresas.Select(e => new CompanyContractDto
        {
            CompanyCen = e.IdEmpresa.ToString(),
            Name = e.Nombre,
            IsActive = e.Activo
        }));
    }

    [HttpGet("companies/{companyCen}/dashboard")]
    public async Task<ActionResult<InventoryDashboardContractDto>> GetDashboard(string companyCen)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var productos = await _productoService.ObtenerTodos(idEmpresa);
        
        return Ok(new InventoryDashboardContractDto
        {
            CompanyCen = companyCen,
            TotalProducts = productos.Count(),
            TotalStockQuantity = 0,
            LowStockCount = 0,
            OutOfStockCount = productos.Count(p => p.Agotado86)
        });
    }

    [HttpGet("companies/{companyCen}/products")]
    public async Task<ActionResult<IEnumerable<ProductContractDto>>> GetProducts(string companyCen, [FromQuery] string? search, [FromQuery] string? categoryCen)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        Guid? idCategoria = string.IsNullOrEmpty(categoryCen) ? null : (Guid.TryParse(categoryCen, out Guid catId) ? catId : null);

        var productos = await _productoService.BuscarFiltrados(idEmpresa, search, idCategoria, null, true);
        
        var result = new List<ProductContractDto>();
        foreach (var p in productos)
        {
            result.Add(new ProductContractDto
            {
                ProductCen = p.IdProducto.ToString(),
                Sku = p.Sku,
                Name = p.Nombre,
                CategoryCen = p.IdCategoria.ToString(),
                CategoryName = "Category",
                UnitCen = p.IdUnidad?.ToString() ?? "",
                UnitName = "Unit",
                SalePrice = p.PrecioVenta,
                Status = p.Activo ? (p.Agotado86 ? "OUT_OF_STOCK" : "ACTIVE") : "INACTIVE",
                ReorderLevel = 10
            });
        }
        return Ok(result);
    }

    [HttpPost("companies/{companyCen}/stock/validate")]
    public async Task<ActionResult<StockValidationContractResponse>> ValidateStock(string companyCen, [FromBody] StockValidationContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        Guid idAlmacen = ResolveWarehouseId(request.WarehouseCen);
        
        var response = new StockValidationContractResponse { IsValid = true };

        foreach (var item in request.Items)
        {
            Guid idProducto = await ResolveProductId(item.ProductCen, idEmpresa);
            bool isAvailable = await _inventarioService.ValidarStockDisponible(idProducto, idAlmacen, item.Quantity, idEmpresa);
            
            if (!isAvailable)
            {
                response.IsValid = false;
                int actual = await _inventarioService.ObtenerStockActual(idProducto, idAlmacen, idEmpresa);
                response.Requirements.Add(new StockRequirementContractDto
                {
                    ProductCen = item.ProductCen,
                    ProductName = "Product",
                    WarehouseCen = request.WarehouseCen,
                    RequestedQuantity = item.Quantity,
                    AvailableQuantity = actual,
                    MissingQuantity = item.Quantity - actual,
                    UnitName = "Unit"
                });
            }
        }

        return Ok(response);
    }

    [HttpPost("companies/{companyCen}/stock/consume")]
    public async Task<ActionResult> ConsumeStock(string companyCen, [FromBody] StockConsumeContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        Guid idAlmacen = ResolveWarehouseId(request.WarehouseCen);

        foreach (var item in request.Items)
        {
            Guid idProducto = await ResolveProductId(item.ProductCen, idEmpresa);
            var result = await _inventarioService.CrearMovimiento(idProducto, idAlmacen, item.Quantity, "SALIDA", idEmpresa, null, request.Reason);
            if (!result.exito) return Conflict(new { message = result.mensaje });
        }

        return Ok(new { success = true });
    }

    [HttpPost("companies/{companyCen}/stock/adjustments")]
    public async Task<ActionResult> AdjustStock(string companyCen, [FromBody] StockAdjustmentContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        Guid idAlmacen = ResolveWarehouseId(request.WarehouseCen);

        foreach (var line in request.Lines)
        {
            Guid idProducto = await ResolveProductId(line.ProductCen, idEmpresa);
            if (line.AdjustmentType == "SET")
            {
                await _inventarioService.AjusteManualStock(idProducto, idAlmacen, line.Quantity, request.Reason, idEmpresa);
            }
            else
            {
                string tipo = line.AdjustmentType == "INCREASE" ? "ENTRADA" : "SALIDA";
                await _inventarioService.CrearMovimiento(idProducto, idAlmacen, line.Quantity, tipo, idEmpresa, null, request.Reason);
            }
        }

        return Ok(new { success = true });
    }

    [HttpGet("companies/{companyCen}/categories")]
    public async Task<ActionResult<IEnumerable<CategoryContractDto>>> GetCategories(string companyCen)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var categorias = await _categoriaService.ObtenerTodos(idEmpresa);
        return Ok(categorias.Select(c => new CategoryContractDto
        {
            CategoryCen = c.IdCategoria.ToString(),
            Name = c.Nombre,
            Description = "",
            IsActive = true
        }));
    }

    [HttpGet("companies/{companyCen}/units")]
    public async Task<ActionResult<IEnumerable<UnitContractDto>>> GetUnits(string companyCen)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var unidades = await _unidadService.ObtenerTodos(idEmpresa);
        return Ok(unidades.Select(u => new UnitContractDto
        {
            UnitCen = u.IdUnidad.ToString(),
            Name = u.Nombre,
            Abbreviation = u.Abreviatura,
            IsActive = u.Activo
        }));
    }

    [HttpGet("companies/{companyCen}/warehouses")]
    public async Task<ActionResult<IEnumerable<WarehouseContractDto>>> GetWarehouses(string companyCen)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var almacenes = await _almacenService.ObtenerTodos(idEmpresa);
        return Ok(almacenes.Select(a => new WarehouseContractDto
        {
            WarehouseCen = a.IdAlmacen.ToString(),
            Name = a.Nombre,
            IsActive = true
        }));
    }

    [HttpGet("companies/{companyCen}/stock")]
    public async Task<ActionResult<IEnumerable<StockContractDto>>> GetStock(
        string companyCen, 
        [FromQuery] string? productCen, 
        [FromQuery] string? warehouseCen)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        Guid? idProducto = string.IsNullOrEmpty(productCen) ? null : await ResolveProductId(productCen, idEmpresa);
        Guid? idAlmacen = string.IsNullOrEmpty(warehouseCen) ? null : ResolveWarehouseId(warehouseCen);

        var stock = await _context.Stock
            .Where(s => s.IdEmpresa == idEmpresa)
            .ToListAsync();

        if (idProducto.HasValue) stock = stock.Where(s => s.IdProducto == idProducto).ToList();
        if (idAlmacen.HasValue) stock = stock.Where(s => s.IdAlmacen == idAlmacen).ToList();

        var productos = await _productoService.ObtenerTodos(idEmpresa);
        var almacenes = await _almacenService.ObtenerTodos(idEmpresa);

        var result = stock.Select(s => {
            var prod = productos.FirstOrDefault(p => p.IdProducto == s.IdProducto);
            var alm = almacenes.FirstOrDefault(a => a.IdAlmacen == s.IdAlmacen);
            return new StockContractDto
            {
                ProductCen = s.IdProducto.ToString(),
                ProductName = prod?.Nombre ?? "Unknown",
                WarehouseCen = s.IdAlmacen.ToString(),
                WarehouseName = alm?.Nombre ?? "Unknown",
                AvailableQuantity = s.Cantidad,
                ReservedQuantity = 0,
                UnitName = "Unit",
                ReorderLevel = 10,
                IsLowStock = s.Cantidad < 10
            };
        });

        return Ok(result);
    }

    [HttpPost("companies/{companyCen}/documents")]
    public async Task<ActionResult> CreateDocument(string companyCen, [FromBody] InventoryDocumentContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        Guid idAlmacen = ResolveWarehouseId(request.WarehouseCen);

        string tipoMovimiento = request.DocumentType switch
        {
            "ENTRY" => "ENTRADA",
            "EXIT" => "SALIDA",
            "SALE_EXIT" => "SALIDA",
            _ => throw new BadHttpRequestException($"Invalid DocumentType: {request.DocumentType}")
        };

        foreach (var line in request.Lines)
        {
            Guid idProducto = await ResolveProductId(line.ProductCen, idEmpresa);
            var result = await _inventarioService.CrearMovimiento(idProducto, idAlmacen, line.Quantity, tipoMovimiento, idEmpresa, null, request.Reason);
            if (!result.exito) return Conflict(new { message = result.mensaje });
        }

        return Ok(new { success = true });
    }

    [HttpGet("companies/{companyCen}/documents")]
    public async Task<ActionResult<IEnumerable<object>>> GetDocuments(string companyCen)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var movimientos = await _context.Movimientos
            .Where(m => m.IdEmpresa == idEmpresa)
            .OrderByDescending(m => m.Fecha)
            .Take(100)
            .ToListAsync();

        return Ok(movimientos.Select(m => new
        {
            DocumentCen = m.IdMovimiento.ToString(),
            Type = m.Tipo,
            Date = m.Fecha,
            WarehouseCen = m.IdAlmacen.ToString(),
            Lines = new[] { new { ProductCen = m.IdProducto.ToString(), Quantity = m.Cantidad } }
        }));
    }
}
