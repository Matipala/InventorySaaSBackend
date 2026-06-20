using System.Text.Json;
using System.Threading.Channels;
using InventorySaaSBackend.Application.DTOs;
using InventorySaaSBackend.Application.DTOs.Contract;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Domain.Entity;
using InventorySaaSBackend.Infrastructure.Data;
using InventorySaaSBackend.Models;
using InventorySaaSBackend.Services;
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
    private readonly Channel<RestockEvent> _restockChannel;

    public InventoryContractController(
        IProductoService productoService,
        IInventarioService inventarioService,
        IEmpresaService empresaService,
        IAlmacenService almacenService,
        ICategoriaService categoriaService,
        IUnidadService unidadService,
        ApplicationDbContext context,
        Channel<RestockEvent> restockChannel)
    {
        _productoService = productoService;
        _inventarioService = inventarioService;
        _empresaService = empresaService;
        _almacenService = almacenService;
        _categoriaService = categoriaService;
        _unidadService = unidadService;
        _context = context;
        _restockChannel = restockChannel;
    }

    private Guid ResolveCompanyId(string companyCen)
    {
        if (Guid.TryParse(companyCen, out Guid id)) return id;
        throw new BadHttpRequestException($"Invalid CompanyCen: {companyCen}");
    }

    private async Task<Guid> ResolveProductId(string productCen, Guid idEmpresa)
    {
        if (Guid.TryParse(productCen, out Guid id)) return id;
        var productos = await _productoService.BuscarFiltrados(idEmpresa, productCen, null, null, null);
        var prod = productos.FirstOrDefault(p => string.Equals(p.Sku, productCen, StringComparison.OrdinalIgnoreCase));
        if (prod != null) return prod.IdProducto;
        throw new BadHttpRequestException($"Invalid ProductCen: {productCen}");
    }

    [HttpGet("companies/{companyCen}/restock-events")]
    public async Task StreamRestockEvents(string companyCen, CancellationToken ct)
    {
        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["Connection"] = "keep-alive";

        await foreach (var evento in _restockChannel.Reader.ReadAllAsync(ct))
        {
            if (evento.CompanyCen == companyCen)
            {
                var json = JsonSerializer.Serialize(evento);
                await Response.WriteAsync($"data: {json}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
    }

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

    [HttpGet("companies/{companyCen}")]
    public async Task<ActionResult<CompanyLookupContractDto>> GetCompany(string companyCen)
    {
        var idEmpresa = ResolveCompanyId(companyCen);
        var empresa = await _empresaService.ObtenerPorId(idEmpresa);
        if (empresa == null)
            return NotFound(new { mensaje = "Empresa no encontrada" });

        return Ok(new CompanyLookupContractDto
        {
            CompanyId = 0,
            CompanyCen = empresa.IdEmpresa.ToString(),
            Name = empresa.Nombre
        });
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
    public async Task<ActionResult<IEnumerable<ProductContractDto>>> GetProducts(
        string companyCen,
        [FromQuery] string? search,
        [FromQuery] string? categoryCen,
        [FromQuery] string? status)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        Guid? idCategoria = string.IsNullOrEmpty(categoryCen) ? null : (Guid.TryParse(categoryCen, out Guid catId) ? catId : null);

        bool? activo = status?.ToUpper() switch
        {
            "ACTIVE" => true,
            "INACTIVE" => false,
            _ => null
        };

        var productos = await _productoService.BuscarFiltrados(idEmpresa, search, idCategoria, null, activo);
        var productosList = productos.ToList();

        var categoriasIds = productosList.Select(p => p.IdCategoria).Distinct().ToList();
        var unidadesIds = productosList.Where(p => p.IdUnidad.HasValue).Select(p => p.IdUnidad!.Value).Distinct().ToList();

        var categorias = await _context.Categorias
            .Where(c => categoriasIds.Contains(c.IdCategoria))
            .ToDictionaryAsync(c => c.IdCategoria);
        var unidades = await _context.Unidades
            .Where(u => unidadesIds.Contains(u.IdUnidad))
            .ToDictionaryAsync(u => u.IdUnidad);

        var result = productosList.Select(p =>
        {
            categorias.TryGetValue(p.IdCategoria, out var cat);
            var unit = p.IdUnidad.HasValue && unidades.TryGetValue(p.IdUnidad.Value, out var u) ? u : null;

            return new ProductContractDto
            {
                ProductCen = p.IdProducto.ToString(),
                Sku = p.Sku,
                Name = p.Nombre,
                Description = null,
                CategoryCen = p.IdCategoria.ToString(),
                CategoryName = cat?.Nombre ?? "",
                UnitCen = p.IdUnidad?.ToString() ?? "",
                UnitName = unit?.Nombre ?? "",
                SalePrice = p.PrecioVenta,
                ReorderLevel = 0,
                Status = p.Activo ? p.Agotado86 ? "OUT_OF_STOCK" : "ACTIVE" : "INACTIVE"
            };
        });
        return Ok(result);
    }

    [HttpPost("companies/{companyCen}/products")]
    public async Task<ActionResult<CreateProductContractResponse>> CreateProduct(
        string companyCen, [FromBody] CreateProductContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);

        var producto = new Productos
        {
            Nombre = request.Name,
            Sku = request.Sku,
            IdCategoria = Guid.Parse(request.CategoryCen),
            IdUnidad = Guid.Parse(request.UnitCen),
            PrecioVenta = request.SalePrice,
            Activo = true
        };

        var result = await _productoService.Crear(producto, idEmpresa);
        if (!result.exito)
            return BadRequest(new { mensaje = result.mensaje });

        return CreatedAtAction(nameof(GetProducts), new { companyCen },
            new CreateProductContractResponse
            {
                ProductCen = result.producto!.IdProducto.ToString(),
                Sku = result.producto.Sku,
                Name = result.producto.Nombre,
                Status = "ACTIVE",
                InitialStock = 0
            });
    }

    [HttpPut("companies/{companyCen}/products/{productCen}")]
    public async Task<ActionResult<ProductContractDto>> UpdateProduct(
        string companyCen, string productCen, [FromBody] UpdateProductContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        if (!Guid.TryParse(productCen, out var idProducto))
            return BadRequest(new { mensaje = "productCen invalido" });

        var existente = await _productoService.ObtenerPorId(idProducto, idEmpresa);
        if (existente == null)
            return NotFound(new { mensaje = "Producto no encontrado" });

        existente.Nombre = !string.IsNullOrWhiteSpace(request.Name) ? request.Name : existente.Nombre;
        existente.Sku = !string.IsNullOrWhiteSpace(request.Sku) ? request.Sku : existente.Sku;
        if (Guid.TryParse(request.CategoryCen, out var idCat))
            existente.IdCategoria = idCat;
        if (Guid.TryParse(request.UnitCen, out var idUnidad))
            existente.IdUnidad = idUnidad;
        existente.PrecioVenta = request.SalePrice > 0 ? request.SalePrice : existente.PrecioVenta;

        var result = await _productoService.Actualizar(idProducto, existente, idEmpresa);
        if (!result.exito)
        {
            if (result.mensaje == "Producto no encontrado")
                return NotFound(new { mensaje = result.mensaje });
            return BadRequest(new { mensaje = result.mensaje });
        }

        return Ok(new ProductContractDto
        {
            ProductCen = result.producto!.IdProducto.ToString(),
            Sku = result.producto.Sku,
            Name = result.producto.Nombre,
            CategoryCen = result.producto.IdCategoria.ToString(),
            UnitCen = result.producto.IdUnidad?.ToString() ?? "",
            SalePrice = result.producto.PrecioVenta,
            Status = result.producto.Activo ? "ACTIVE" : "INACTIVE"
        });
    }

    [HttpPatch("companies/{companyCen}/products/{productCen}/status")]
    public async Task<ActionResult<ProductContractDto>> UpdateProductStatus(
        string companyCen, string productCen, [FromBody] UpdateProductStatusContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        if (!Guid.TryParse(productCen, out var idProducto))
            return BadRequest(new { mensaje = "productCen invalido" });

        bool activo = request.Status?.ToUpper() == "ACTIVE";
        var producto = await _productoService.CambiarEstado(idProducto, activo, idEmpresa);
        if (producto == null)
            return NotFound(new { mensaje = "Producto no encontrado" });

        return Ok(new ProductContractDto
        {
            ProductCen = producto.IdProducto.ToString(),
            Sku = producto.Sku,
            Name = producto.Nombre,
            CategoryCen = producto.IdCategoria.ToString(),
            UnitCen = producto.IdUnidad?.ToString() ?? "",
            SalePrice = producto.PrecioVenta,
            Status = producto.Activo ? "ACTIVE" : "INACTIVE"
        });
    }

    [HttpPost("companies/{companyCen}/products/lookup")]
    public async Task<ActionResult<IEnumerable<ProductContractDto>>> LookupProducts(
        string companyCen, [FromBody] ProductLookupContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var result = new List<ProductContractDto>();

        foreach (var pc in request.ProductCens)
        {
            if (Guid.TryParse(pc, out var pid))
            {
                var prod = await _productoService.ObtenerPorId(pid, idEmpresa);
                if (prod != null)
                {
                    result.Add(new ProductContractDto
                    {
                        ProductCen = prod.IdProducto.ToString(),
                        Sku = prod.Sku,
                        Name = prod.Nombre,
                        CategoryCen = prod.IdCategoria.ToString(),
                        SalePrice = prod.PrecioVenta,
                        Status = prod.Activo ? "ACTIVE" : "INACTIVE"
                    });
                }
            }
        }

        return Ok(result);
    }

    [HttpGet("companies/{companyCen}/products/{productCen}/kardex")]
    public async Task<ActionResult<IEnumerable<KardexMovementContractDto>>> GetProductKardex(
        string companyCen, string productCen,
        [FromQuery] string? warehouseCen,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var idProducto = await ResolveProductId(productCen, idEmpresa);

        var query = _context.Movimientos
            .Where(m => m.IdProducto == idProducto && m.IdEmpresa == idEmpresa)
            .AsQueryable();

        if (Guid.TryParse(warehouseCen, out var idAlmacen))
            query = query.Where(m => m.IdAlmacen == idAlmacen);

        if (from.HasValue)
            query = query.Where(m => m.Fecha >= from.Value);

        if (to.HasValue)
            query = query.Where(m => m.Fecha <= to.Value);

        var movimientos = await query
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

        return Ok(movimientos.Select(m => new KardexMovementContractDto
        {
            MovementCen = m.IdMovimiento.ToString(),
            ProductCen = m.IdProducto.ToString(),
            WarehouseCen = m.IdAlmacen.ToString(),
            MovementType = m.Tipo,
            Quantity = m.Cantidad,
            Reason = m.Motivo,
            CreatedAt = m.Fecha
        }));
    }

    [HttpGet("companies/{companyCen}/sellable-products")]
    public async Task<ActionResult<IEnumerable<SellableProductContractDto>>> GetSellableProducts(
        string companyCen,
        [FromQuery] string? search,
        [FromQuery] string? categoryCen,
        [FromQuery] string? warehouseCen,
        [FromQuery] bool onlyAvailable = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        Guid? idCategoria = string.IsNullOrEmpty(categoryCen) ? null :
            (Guid.TryParse(categoryCen, out Guid catId) ? catId : null);
        Guid? idAlmacen = string.IsNullOrEmpty(warehouseCen) ? null :
            (Guid.TryParse(warehouseCen, out var wid) ? wid : null);

        var productos = await _productoService.BuscarFiltrados(idEmpresa, search, idCategoria, null, true);
        var disponibles = productos.Where(p => !p.Agotado86).ToList();

        if (!disponibles.Any())
            return Ok(Array.Empty<SellableProductContractDto>());

        var productIds = disponibles.Select(p => p.IdProducto).ToList();
        var categoriaIds = disponibles.Select(p => p.IdCategoria).Distinct().ToList();

        var categorias = await _context.Categorias
            .Where(c => categoriaIds.Contains(c.IdCategoria))
            .ToDictionaryAsync(c => c.IdCategoria);

        var stockQuery = _context.Stock
            .Where(s => s.IdEmpresa == idEmpresa && productIds.Contains(s.IdProducto));

        if (idAlmacen.HasValue)
            stockQuery = stockQuery.Where(s => s.IdAlmacen == idAlmacen.Value);

        var stockMap = await stockQuery
            .GroupBy(s => s.IdProducto)
            .Select(g => new { ProductoId = g.Key, Total = g.Sum(s => s.Cantidad) })
            .ToDictionaryAsync(g => g.ProductoId, g => g.Total);

        var result = disponibles.Select(p =>
        {
            categorias.TryGetValue(p.IdCategoria, out var cat);
            stockMap.TryGetValue(p.IdProducto, out var qty);
            return new SellableProductContractDto
            {
                ProductCen = p.IdProducto.ToString(),
                Name = p.Nombre,
                CategoryCen = p.IdCategoria.ToString(),
                CategoryName = cat?.Nombre ?? "",
                SalePrice = p.PrecioVenta,
                AvailableQuantity = qty,
                IsAvailable = !p.Agotado86,
                StationCode = p.Estacion
            };
        });

        return Ok(result.Skip((page - 1) * pageSize).Take(pageSize));
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
            Description = null,
            IsActive = c.Activo
        }));
    }

    [HttpPost("companies/{companyCen}/categories")]
    public async Task<ActionResult<CategoryContractDto>> CreateCategory(
        string companyCen, [FromBody] CreateCategoryContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);

        var categoria = new Categoria
        {
            Nombre = request.Name,
            IdEmpresa = idEmpresa
        };

        var result = await _categoriaService.Crear(categoria, idEmpresa);
        if (!result.exito)
            return BadRequest(new { mensaje = result.mensaje });

        return CreatedAtAction(nameof(GetCategories), new { companyCen },
            new CategoryContractDto
            {
                CategoryCen = result.categoria!.IdCategoria.ToString(),
                Name = result.categoria.Nombre,
                IsActive = true
            });
    }

    [HttpPut("companies/{companyCen}/categories/{categoryCen}")]
    public async Task<ActionResult<CategoryContractDto>> UpdateCategory(
        string companyCen, string categoryCen, [FromBody] CreateCategoryContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        if (!Guid.TryParse(categoryCen, out var idCategoria))
            return BadRequest(new { mensaje = "categoryCen invalido" });

        var categoria = new Categoria { Nombre = request.Name };
        var result = await _categoriaService.Actualizar(idCategoria, categoria, idEmpresa);

        if (!result.exito)
        {
            if (result.mensaje == "Categoría no encontrada")
                return NotFound(new { mensaje = result.mensaje });
            return BadRequest(new { mensaje = result.mensaje });
        }

        return Ok(new CategoryContractDto
        {
            CategoryCen = result.categoria!.IdCategoria.ToString(),
            Name = result.categoria.Nombre,
            IsActive = true
        });
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

    [HttpPost("companies/{companyCen}/units")]
    public async Task<ActionResult<UnitContractDto>> CreateUnit(
        string companyCen, [FromBody] CreateUnitContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);

        var unidad = new UnidadMedida
        {
            Nombre = request.Name,
            Abreviatura = request.Abbreviation ?? ""
        };

        var result = await _unidadService.Crear(unidad, idEmpresa);
        if (!result.exito)
            return BadRequest(new { mensaje = result.mensaje });

        return CreatedAtAction(nameof(GetUnits), new { companyCen },
            new UnitContractDto
            {
                UnitCen = result.unidad!.IdUnidad.ToString(),
                Name = result.unidad.Nombre,
                Abbreviation = result.unidad.Abreviatura,
                IsActive = true
            });
    }

    [HttpPut("companies/{companyCen}/units/{unitCen}")]
    public async Task<ActionResult<UnitContractDto>> UpdateUnit(
        string companyCen, string unitCen, [FromBody] CreateUnitContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        if (!Guid.TryParse(unitCen, out var idUnidad))
            return BadRequest(new { mensaje = "unitCen invalido" });

        var unidad = new UnidadMedida
        {
            Nombre = request.Name,
            Abreviatura = request.Abbreviation ?? ""
        };

        var result = await _unidadService.Actualizar(idUnidad, unidad, idEmpresa);
        if (!result.exito)
        {
            if (result.mensaje == "Unidad no encontrada.")
                return NotFound(new { mensaje = result.mensaje });
            return BadRequest(new { mensaje = result.mensaje });
        }

        return Ok(new UnitContractDto
        {
            UnitCen = result.unidad!.IdUnidad.ToString(),
            Name = result.unidad.Nombre,
            Abbreviation = result.unidad.Abreviatura,
            IsActive = result.unidad.Activo
        });
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
        Guid? idAlmacen = string.IsNullOrEmpty(warehouseCen) ? null :
            (Guid.TryParse(warehouseCen, out var wid) ? wid : null);

        var stock = await _context.Stock
            .Where(s => s.IdEmpresa == idEmpresa)
            .ToListAsync();

        if (idProducto.HasValue) stock = stock.Where(s => s.IdProducto == idProducto).ToList();
        if (idAlmacen.HasValue) stock = stock.Where(s => s.IdAlmacen == idAlmacen).ToList();

        var productos = await _productoService.ObtenerTodos(idEmpresa);
        var almacenes = await _almacenService.ObtenerTodos(idEmpresa);

        var result = stock.Select(s =>
        {
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

    [HttpPost("companies/{companyCen}/stock/validate")]
    public async Task<ActionResult<StockValidationContractResponse>> ValidateStock(
        string companyCen, [FromBody] StockValidationContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var idAlmacen = Guid.Parse(request.WarehouseCen);

        var response = new StockValidationContractResponse { IsValid = true };

        foreach (var item in request.Items)
        {
            Guid idProducto = await ResolveProductId(item.ProductCen, idEmpresa);
            bool isAvailable = await _inventarioService.ValidarStockDisponible(idProducto, idAlmacen, (int)item.Quantity, idEmpresa);

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
    public async Task<ActionResult<StockConsumeContractResponse>> ConsumeStock(
        string companyCen, [FromBody] StockConsumeContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var idAlmacen = Guid.Parse(request.WarehouseCen);
        var movementCens = new List<string>();

        foreach (var item in request.Items)
        {
            Guid idProducto = await ResolveProductId(item.ProductCen, idEmpresa);
            var result = await _inventarioService.CrearMovimiento(
                idProducto, idAlmacen, (int)item.Quantity, "SALIDA", idEmpresa, null, request.Reason);
            if (!result.exito)
                return Conflict(new StockConsumeContractResponse
                {
                    Success = false,
                    Requirements = new List<StockRequirementContractDto>
                    {
                        new() { ProductCen = item.ProductCen, Reason = result.mensaje }
                    }
                });
            movementCens.Add(Guid.NewGuid().ToString());
        }

        return Ok(new StockConsumeContractResponse
        {
            Success = true,
            DocumentCen = Guid.NewGuid().ToString(),
            DocumentType = "SALE_EXIT",
            GeneratedMovementCens = movementCens
        });
    }

    [HttpPost("companies/{companyCen}/stock/increase")]
    public async Task<ActionResult> IncreaseStock(
        string companyCen, [FromBody] StockIncreaseContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var idAlmacen = Guid.Parse(request.WarehouseCen);

        foreach (var item in request.Items)
        {
            Guid idProducto = await ResolveProductId(item.ProductCen, idEmpresa);
            var result = await _inventarioService.CrearMovimiento(
                idProducto, idAlmacen, (int)item.Quantity, "ENTRADA", idEmpresa, null, request.Reason);
            if (!result.exito)
                return BadRequest(new { mensaje = result.mensaje });
        }

        await NotifyRestockAsync(companyCen, request.WarehouseCen, request.Items);

        return Ok("Stock incrementado exitosamente");
    }

    [HttpPost("companies/{companyCen}/stock/adjustments")]
    public async Task<ActionResult<InventoryAdjustmentContractResponse>> AdjustStock(
        string companyCen, [FromBody] InventoryAdjustmentContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var idAlmacen = Guid.Parse(request.WarehouseCen);
        var movements = new List<GeneratedMovementContractDto>();

        foreach (var line in request.Lines)
        {
            Guid idProducto = await ResolveProductId(line.ProductCen, idEmpresa);

            if (line.AdjustmentType == "SET")
            {
                await _inventarioService.AjusteManualStock(
                    idProducto, idAlmacen, (int)line.Quantity, request.Reason, idEmpresa);
            }
            else
            {
                string tipo = line.AdjustmentType == "INCREASE" ? "ENTRADA" : "SALIDA";
                await _inventarioService.CrearMovimiento(
                    idProducto, idAlmacen, (int)line.Quantity, tipo, idEmpresa, null, request.Reason);
            }

            movements.Add(new GeneratedMovementContractDto
            {
                MovementCen = Guid.NewGuid().ToString(),
                ProductCen = line.ProductCen,
                WarehouseCen = request.WarehouseCen,
                Quantity = line.Quantity,
                MovementType = line.AdjustmentType
            });
        }

        return Ok(new InventoryAdjustmentContractResponse
        {
            AdjustmentCen = Guid.NewGuid().ToString(),
            Status = "COMPLETED",
            GeneratedMovements = movements
        });
    }

    [HttpPost("companies/{companyCen}/documents")]
    public async Task<ActionResult> CreateDocument(
        string companyCen, [FromBody] InventoryDocumentContractRequest request)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        var idAlmacen = Guid.Parse(request.WarehouseCen);

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
            var result = await _inventarioService.CrearMovimiento(
                idProducto, idAlmacen, (int)line.Quantity, tipoMovimiento, idEmpresa, null, request.Reason);
            if (!result.exito)
                return Conflict(new { message = result.mensaje });
        }

        if (request.DocumentType == "ENTRY")
        {
            var items = request.Lines.Select(l => new StockValidationItemContractDto
            {
                ProductCen = l.ProductCen,
                Quantity = l.Quantity
            }).ToList();
            await NotifyRestockAsync(companyCen, request.WarehouseCen, items);
        }

        return Ok(new { success = true });
    }

    private async Task NotifyRestockAsync(string companyCen, string warehouseCen, List<StockValidationItemContractDto> items)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);
        foreach (var item in items)
        {
            string productName = "Producto";
            try
            {
                Guid idProducto = await ResolveProductId(item.ProductCen, idEmpresa);
                var producto = await _productoService.ObtenerPorId(idProducto, idEmpresa);
                if (producto != null) productName = producto.Nombre;
            }
            catch { }

            var restockEvent = new RestockEvent
            {
                CompanyCen = companyCen,
                ProductCen = item.ProductCen,
                ProductName = productName,
                Quantity = (int)item.Quantity,
                WarehouseCen = warehouseCen,
                EventType = "RESTOCK",
                Timestamp = DateTime.UtcNow
            };

            await _restockChannel.Writer.WriteAsync(restockEvent);
        }
    }

    [HttpGet("companies/{companyCen}/documents")]
    public async Task<ActionResult<IEnumerable<object>>> GetDocuments(
        string companyCen,
        [FromQuery] string? documentType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        Guid idEmpresa = ResolveCompanyId(companyCen);

        var query = _context.Movimientos
            .Where(m => m.IdEmpresa == idEmpresa)
            .AsQueryable();

        if (!string.IsNullOrEmpty(documentType))
            query = query.Where(m => m.Tipo == documentType);

        if (from.HasValue)
            query = query.Where(m => m.Fecha >= from.Value);

        if (to.HasValue)
            query = query.Where(m => m.Fecha <= to.Value);

        var movimientos = await query
            .OrderByDescending(m => m.Fecha)
            .Take(100)
            .ToListAsync();

        return Ok(movimientos.Select(m => new
        {
            DocumentCen = m.IdMovimiento.ToString(),
            DocumentType = m.Tipo,
            Status = "COMPLETED",
            Title = $"{m.Tipo} - {m.Motivo}",
            CreatedAt = m.Fecha,
            TotalItems = 1,
            GeneratedMovementCens = new[] { m.IdMovimiento.ToString() }
        }));
    }
}
