using InventorySaaSBackend.Application.DTOs.Contract;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Models;
using InventorySaaSBackend.Services;
using Microsoft.AspNetCore.Mvc;

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

    public InventoryContractController(
        IProductoService productoService,
        IInventarioService inventarioService,
        IEmpresaService empresaService,
        IAlmacenService almacenService,
        ICategoriaService categoriaService,
        IUnidadService unidadService)
    {
        _productoService = productoService;
        _inventarioService = inventarioService;
        _empresaService = empresaService;
        _almacenService = almacenService;
        _categoriaService = categoriaService;
        _unidadService = unidadService;
    }

    #region Helpers
    private int ResolveCompanyId(string companyCen)
    {
        if (int.TryParse(companyCen, out int id)) return id;
        throw new BadHttpRequestException($"Invalid CompanyCen: {companyCen}. Must be an integer ID.");
    }

    private int ResolveWarehouseId(string warehouseCen)
    {
        if (int.TryParse(warehouseCen, out int id)) return id;
        throw new BadHttpRequestException($"Invalid WarehouseCen: {warehouseCen}. Must be an integer ID.");
    }

    private async Task<int> ResolveProductId(string productCen, int idEmpresa)
    {
        // Try as ID first
        if (int.TryParse(productCen, out int id)) return id;

        // Try as SKU
        var productos = await _productoService.BuscarFiltrados(idEmpresa, productCen, null, null, null);
        var prod = productos.FirstOrDefault(p => string.Equals(p.Sku, productCen, StringComparison.OrdinalIgnoreCase));
        if (prod != null) return prod.IdProducto;

        throw new BadHttpRequestException($"Invalid ProductCen: {productCen}. Not found as ID or SKU.");
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
        int idEmpresa = ResolveCompanyId(companyCen);
        var productos = await _productoService.ObtenerTodos(idEmpresa);
        
        // This is a simplified dashboard logic
        return Ok(new InventoryDashboardContractDto
        {
            CompanyCen = companyCen,
            TotalProducts = productos.Count(),
            TotalStockQuantity = 0, // Need stock service for total sum if required
            LowStockCount = 0,
            OutOfStockCount = productos.Count(p => p.Agotado86)
        });
    }

    [HttpGet("companies/{companyCen}/products")]
    public async Task<ActionResult<IEnumerable<ProductContractDto>>> GetProducts(string companyCen, [FromQuery] string? search, [FromQuery] string? categoryCen)
    {
        int idEmpresa = ResolveCompanyId(companyCen);
        int? idCategoria = string.IsNullOrEmpty(categoryCen) ? null : int.Parse(categoryCen);

        var productos = await _productoService.BuscarFiltrados(idEmpresa, search, idCategoria, null, true);
        
        var result = new List<ProductContractDto>();
        foreach (var p in productos)
        {
            result.Add(new ProductContractDto
            {
                ProductCen = p.Sku, // Using SKU as CEN for products
                Sku = p.Sku,
                Name = p.Nombre,
                CategoryCen = p.IdCategoria.ToString(),
                CategoryName = "Category", // Would need to join if name is required
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
        int idEmpresa = ResolveCompanyId(companyCen);
        int idAlmacen = ResolveWarehouseId(request.WarehouseCen);
        
        var response = new StockValidationContractResponse { IsValid = true };

        foreach (var item in request.Items)
        {
            int idProducto = await ResolveProductId(item.ProductCen, idEmpresa);
            bool isAvailable = await _inventarioService.ValidarStockDisponible(idProducto, idAlmacen, item.Quantity, idEmpresa);
            
            if (!isAvailable)
            {
                response.IsValid = false;
                int actual = await _inventarioService.ObtenerStockActual(idProducto, idAlmacen, idEmpresa);
                response.Requirements.Add(new StockRequirementContractDto
                {
                    ProductCen = item.ProductCen,
                    ProductName = "Product", // Should fetch name
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
        int idEmpresa = ResolveCompanyId(companyCen);
        int idAlmacen = ResolveWarehouseId(request.WarehouseCen);

        foreach (var item in request.Items)
        {
            int idProducto = await ResolveProductId(item.ProductCen, idEmpresa);
            var result = await _inventarioService.CrearMovimiento(idProducto, idAlmacen, item.Quantity, "SALIDA", idEmpresa, null, request.Reason);
            if (!result.exito) return Conflict(new { message = result.mensaje });
        }

        return Ok(new { success = true });
    }

    [HttpGet("companies/{companyCen}/categories")]
    public async Task<ActionResult<IEnumerable<CategoryContractDto>>> GetCategories(string companyCen)
    {
        int idEmpresa = ResolveCompanyId(companyCen);
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
        int idEmpresa = ResolveCompanyId(companyCen);
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
        int idEmpresa = ResolveCompanyId(companyCen);
        var almacenes = await _almacenService.ObtenerTodos(idEmpresa);
        return Ok(almacenes.Select(a => new WarehouseContractDto
        {
            WarehouseCen = a.IdAlmacen.ToString(),
            Name = a.Nombre,
            IsActive = true
        }));
    }

    [HttpPost("companies/{companyCen}/documents")]
    public async Task<ActionResult> CreateDocument(string companyCen, [FromBody] InventoryDocumentContractRequest request)
    {
        int idEmpresa = ResolveCompanyId(companyCen);
        int idAlmacen = ResolveWarehouseId(request.WarehouseCen);

        string tipoMovimiento = request.DocumentType == "ENTRY" ? "ENTRADA" : "SALIDA";

        foreach (var line in request.Lines)
        {
            int idProducto = await ResolveProductId(line.ProductCen, idEmpresa);
            var result = await _inventarioService.CrearMovimiento(idProducto, idAlmacen, line.Quantity, tipoMovimiento, idEmpresa, null, request.Reason);
            if (!result.exito) return Conflict(new { message = result.mensaje });
        }

        return Ok(new { documentCen = Guid.NewGuid().ToString(), status = "REGISTERED" });
    }
}
