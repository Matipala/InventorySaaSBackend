using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventorySaaSBackend.Controllers;
using InventorySaaSBackend.Infrastructure.Data;
using InventorySaaSBackend.Application.DTOs;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Models;
using InventorySaaSBackend.Services;

namespace InventorySaaSBackend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IInventarioService _inventarioService;
    private readonly IExportService _exportService;

    public StockController(ApplicationDbContext context, IInventarioService inventarioService, IExportService exportService)
    {
        _context = context;
        _inventarioService = inventarioService;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        int empresaId = GetEmpresaId();

        var stock = await _context.Stock
            .Where(s => s.IdEmpresa == empresaId)
            .Join(_context.Productos,
                s => s.IdProducto,
                p => p.IdProducto,
                (s, p) => new { Stock = s, Producto = p })
            .Join(_context.Almacenes,
                sp => sp.Stock.IdAlmacen,
                a => a.IdAlmacen,
                (sp, a) => new
                {
                    sp.Stock.IdStock,
                    sp.Stock.IdProducto,
                    ProductoNombre = sp.Producto.Nombre,
                    ProductoSku = sp.Producto.Sku,
                    sp.Stock.IdAlmacen,
                    AlmacenNombre = a.Nombre,
                    sp.Stock.Cantidad
                })
            .OrderBy(s => s.ProductoNombre)
            .ToListAsync();

        return Ok(stock);
    }

    [HttpGet("producto/{idProducto}")]
    public async Task<IActionResult> GetByProducto(int idProducto)
    {
        int empresaId = GetEmpresaId();

        var stock = await _context.Stock
            .Where(s => s.IdProducto == idProducto && s.IdEmpresa == empresaId)
            .Join(_context.Almacenes,
                s => s.IdAlmacen,
                a => a.IdAlmacen,
                (s, a) => new
                {
                    s.IdStock,
                    s.IdAlmacen,
                    AlmacenNombre = a.Nombre,
                    s.Cantidad
                })
            .ToListAsync();

        if (!stock.Any())
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == idProducto && p.IdEmpresa == empresaId);

            if (producto == null)
                return NotFound("Producto no encontrado");

            return Ok(new { mensaje = "El producto no tiene stock registrado", stock = new List<object>() });
        }

        return Ok(stock);
    }

    [HttpGet("almacen/{idAlmacen}")]
    public async Task<IActionResult> GetByAlmacen(int idAlmacen)
    {
        int empresaId = GetEmpresaId();

        var stock = await _context.Stock
            .Where(s => s.IdAlmacen == idAlmacen && s.IdEmpresa == empresaId)
            .Join(_context.Productos,
                s => s.IdProducto,
                p => p.IdProducto,
                (s, p) => new
                {
                    s.IdStock,
                    s.IdProducto,
                    ProductoNombre = p.Nombre,
                    ProductoSku = p.Sku,
                    s.Cantidad
                })
            .OrderBy(s => s.ProductoNombre)
            .ToListAsync();

        return Ok(stock);
    }

    [HttpGet("producto/{idProducto}/almacen/{idAlmacen}")]
    public async Task<IActionResult> GetByProductoAlmacen(int idProducto, int idAlmacen)
    {
        int empresaId = GetEmpresaId();

        var cantidad = await _inventarioService.ObtenerStockActual(idProducto, idAlmacen, empresaId);

        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.IdProducto == idProducto && p.IdEmpresa == empresaId);

        var almacen = await _context.Almacenes
            .FirstOrDefaultAsync(a => a.IdAlmacen == idAlmacen && a.IdEmpresa == empresaId);

        if (producto == null || almacen == null)
            return NotFound("Producto o almacén no encontrado");

        return Ok(new
        {
            IdProducto = idProducto,
            ProductoNombre = producto.Nombre,
            ProductoSku = producto.Sku,
            IdAlmacen = idAlmacen,
            AlmacenNombre = almacen.Nombre,
            Cantidad = cantidad
        });
    }

    [HttpPost("ajuste")]
    public async Task<IActionResult> AjusteManual([FromBody] AjusteStockRequest request)
    {
        int empresaId = GetEmpresaId();

        var resultado = await _inventarioService.AjusteManualStock(
            request.IdProducto,
            request.IdAlmacen,
            request.NuevaCantidad,
            request.Motivo,
            empresaId
        );

        if (!resultado.exito)
            return BadRequest(resultado.mensaje);

        return Ok(new { mensaje = resultado.mensaje });
    }

    [HttpPost("inicial")]
    public async Task<IActionResult> RegistrarStockInicial([FromBody] StockInicialRequest request)
    {
        int empresaId = GetEmpresaId();

        if (request.Cantidad < 0)
            return (BadRequest("La cantidad inicial no puede ser negativa"));

        var resultado = await _inventarioService.CrearMovimiento(
            request.IdProducto,
            request.IdAlmacen,
            request.Cantidad,
            "ENTRADA",
            empresaId,
            null,
            "Stock Inicial"
        );

        if (!resultado.exito)
            return BadRequest(resultado.mensaje);

        return Ok(new { mensaje = "Stock inicial registrado exitosamente" });
    }

    [HttpGet("alertas/bajo")]
    public async Task<IActionResult> GetStockBajo([FromQuery] int umbral = 10)
    {
        int empresaId = GetEmpresaId();

        var stockBajo = await _context.Stock
            .Where(s => s.IdEmpresa == empresaId && s.Cantidad <= umbral && s.Cantidad > 0)
            .Join(_context.Productos,
                s => s.IdProducto,
                p => p.IdProducto,
                (s, p) => new { Stock = s, Producto = p })
            .Join(_context.Almacenes,
                sp => sp.Stock.IdAlmacen,
                a => a.IdAlmacen,
                (sp, a) => new
                {
                    sp.Stock.IdProducto,
                    ProductoNombre = sp.Producto.Nombre,
                    ProductoSku = sp.Producto.Sku,
                    sp.Stock.IdAlmacen,
                    AlmacenNombre = a.Nombre,
                    sp.Stock.Cantidad,
                    Estado = "Stock Bajo"
                })
            .OrderBy(s => s.Cantidad)
            .ToListAsync();

        return Ok(stockBajo);
    }

    [HttpGet("alertas/agotado")]
    public async Task<IActionResult> GetStockAgotado()
    {
        int empresaId = GetEmpresaId();

        var stockAgotado = await _context.Stock
            .Where(s => s.IdEmpresa == empresaId && s.Cantidad == 0)
            .Join(_context.Productos,
                s => s.IdProducto,
                p => p.IdProducto,
                (s, p) => new { Stock = s, Producto = p })
            .Join(_context.Almacenes,
                sp => sp.Stock.IdAlmacen,
                a => a.IdAlmacen,
                (sp, a) => new
                {
                    sp.Stock.IdProducto,
                    ProductoNombre = sp.Producto.Nombre,
                    ProductoSku = sp.Producto.Sku,
                    sp.Stock.IdAlmacen,
                    AlmacenNombre = a.Nombre,
                    sp.Stock.Cantidad,
                    Estado = "Agotado"
                })
            .ToListAsync();

        return Ok(stockAgotado);
    }

    [HttpPost("validar")]
    public async Task<IActionResult> Validar([FromBody] ValidarStockRequest request)
    {
        int empresaId = GetEmpresaId();
        bool disponible = await _inventarioService.ValidarStockDisponible(request.ProductoId, request.AlmacenId, (int)request.Cantidad, empresaId);
        return Ok(disponible);
    }

    [HttpPost("descontar")]
    public async Task<IActionResult> Descontar([FromBody] ValidarStockRequest request)
    {
        int empresaId = GetEmpresaId();
        try
        {
            await _inventarioService.ActualizarStock(request.ProductoId, request.AlmacenId, (int)-request.Cantidad, empresaId);
            return Ok(true);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = "No se pudo descontar el stock: " + ex.Message });
        }
    }

    [HttpGet("actual")]
    public async Task<IActionResult> GetActual([FromQuery] int productoId, [FromQuery] int almacenId)
    {
        int empresaId = GetEmpresaId();
        var cantidad = await _inventarioService.ObtenerStockActual(productoId, almacenId, empresaId);
        return Ok(cantidad);
    }

    [HttpGet("exportar/excel")]
    public async Task<IActionResult> ExportarExcel()
    {
        int empresaId = GetEmpresaId();
        var fileBytes = await _exportService.ExportarStockExcel(empresaId);
        var fileName = $"Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
