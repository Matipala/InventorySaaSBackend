using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventorySaaSBackend.Controllers;
using InventorySaaSBackend.Data;
using InventorySaaSBackend.Application.DTOs;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Models;
using InventorySaaSBackend.Services;

namespace InventorySaaSBackend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly IExportService _exportService;
    private readonly IInventarioService _inventarioService;

    public MovimientosController(ApplicationDbContext context, IExportService exportService, IInventarioService inventarioService)
    {
        _context = context;
        _exportService = exportService;
        _inventarioService = inventarioService;
    }

    [HttpPost]
    public async Task<IActionResult> CrearMovimiento([FromBody] CrearMovimientoRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        int empresaId = GetEmpresaId();
        var resultado = await _inventarioService.CrearMovimiento(
            request.IdProducto, request.IdAlmacen, request.Cantidad,
            request.Tipo, empresaId, request.IdAlmacenDestino);

        if (!resultado.exito)
            return BadRequest(new { mensaje = resultado.mensaje });

        return Ok(new { mensaje = resultado.mensaje });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        [FromQuery] string? tipo)
    {
        int empresaId = GetEmpresaId();

        var query = _context.Movimientos
            .Where(m => m.IdEmpresa == empresaId);

        // Filtros opcionales
        if (fechaInicio.HasValue)
            query = query.Where(m => m.Fecha >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(m => m.Fecha <= fechaFin.Value);

        if (!string.IsNullOrEmpty(tipo))
            query = query.Where(m => m.Tipo == tipo);

        var movimientos = await query
            .Join(_context.Productos,
                m => m.IdProducto,
                p => p.IdProducto,
                (m, p) => new { Movimiento = m, Producto = p })
            .Join(_context.Almacenes,
                mp => mp.Movimiento.IdAlmacen,
                a => a.IdAlmacen,
                (mp, a) => new
                {
                    mp.Movimiento.IdMovimiento,
                    mp.Movimiento.IdProducto,
                    ProductoNombre = mp.Producto.Nombre,
                    ProductoSku = mp.Producto.Sku,
                    mp.Movimiento.IdAlmacen,
                    AlmacenNombre = a.Nombre,
                    mp.Movimiento.Tipo,
                    mp.Movimiento.Fecha,
                    mp.Movimiento.Cantidad
                })
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

        return Ok(movimientos);
    }

    [HttpGet("producto/{idProducto}")]
    public async Task<IActionResult> GetKardexProducto(
        int idProducto,
        [FromQuery] int? idAlmacen,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        int empresaId = GetEmpresaId();

        var query = _context.Movimientos
            .Where(m => m.IdProducto == idProducto && m.IdEmpresa == empresaId);

        if (idAlmacen.HasValue)
            query = query.Where(m => m.IdAlmacen == idAlmacen.Value);

        if (fechaInicio.HasValue)
            query = query.Where(m => m.Fecha >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(m => m.Fecha <= fechaFin.Value);

        var kardex = await query
            .Join(_context.Almacenes,
                m => m.IdAlmacen,
                a => a.IdAlmacen,
                (m, a) => new
                {
                    m.IdMovimiento,
                    m.IdAlmacen,
                    AlmacenNombre = a.Nombre,
                    m.Tipo,
                    m.Fecha,
                    m.Cantidad,
                    Entrada = m.Tipo == "ENTRADA" || m.Tipo == "AJUSTE_POSITIVO" ? m.Cantidad : 0,
                    Salida = m.Tipo == "SALIDA" || m.Tipo == "AJUSTE_NEGATIVO" ? m.Cantidad : 0
                })
            .OrderBy(m => m.Fecha)
            .ToListAsync();

        int saldo = 0;
        var kardexConSaldo = kardex.Select(k =>
        {
            saldo += k.Entrada - k.Salida;
            return new
            {
                k.IdMovimiento,
                k.IdAlmacen,
                k.AlmacenNombre,
                k.Tipo,
                k.Fecha,
                k.Cantidad,
                k.Entrada,
                k.Salida,
                Saldo = saldo
            };
        }).ToList();

        return Ok(kardexConSaldo);
    }

    [HttpGet("almacen/{idAlmacen}")]
    public async Task<IActionResult> GetByAlmacen(
        int idAlmacen,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        int empresaId = GetEmpresaId();

        var query = _context.Movimientos
            .Where(m => m.IdAlmacen == idAlmacen && m.IdEmpresa == empresaId);

        if (fechaInicio.HasValue)
            query = query.Where(m => m.Fecha >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(m => m.Fecha <= fechaFin.Value);

        var movimientos = await query
            .Join(_context.Productos,
                m => m.IdProducto,
                p => p.IdProducto,
                (m, p) => new
                {
                    m.IdMovimiento,
                    m.IdProducto,
                    ProductoNombre = p.Nombre,
                    ProductoSku = p.Sku,
                    m.Tipo,
                    m.Fecha,
                    m.Cantidad
                })
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

        return Ok(movimientos);
    }

    [HttpGet("alertas/sin-movimiento")]
    public async Task<IActionResult> GetProductosSinMovimiento([FromQuery] int dias = 30)
    {
        int empresaId = GetEmpresaId();
        var fechaLimite = DateTime.UtcNow.AddDays(-dias);

        var productosConStock = await _context.Stock
            .Where(s => s.IdEmpresa == empresaId && s.Cantidad > 0)
            .Select(s => s.IdProducto)
            .Distinct()
            .ToListAsync();

        var productosConMovimiento = await _context.Movimientos
            .Where(m => m.IdEmpresa == empresaId && m.Fecha >= fechaLimite)
            .Select(m => m.IdProducto)
            .Distinct()
            .ToListAsync();

        var productosSinMovimiento = productosConStock
            .Except(productosConMovimiento)
            .ToList();

        var resultado = await _context.Productos
            .Where(p => productosSinMovimiento.Contains(p.IdProducto) && p.IdEmpresa == empresaId)
            .Join(_context.Stock.Where(s => s.IdEmpresa == empresaId),
                p => p.IdProducto,
                s => s.IdProducto,
                (p, s) => new
                {
                    p.IdProducto,
                    p.Nombre,
                    p.Sku,
                    CantidadStock = s.Cantidad,
                    DiasSinMovimiento = dias,
                    Alerta = "Producto inactivo"
                })
            .ToListAsync();

        return Ok(resultado);
    }

    [HttpGet("exportar/kardex/{idProducto}")]
    public async Task<IActionResult> ExportarKardexExcel(
        int idProducto,
        [FromQuery] int? idAlmacen,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        int empresaId = GetEmpresaId();

        try
        {
            var fileBytes = await _exportService.ExportarKardexExcel(
                idProducto,
                idAlmacen ?? 0,
                empresaId,
                fechaInicio,
                fechaFin
            );

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == idProducto && p.IdEmpresa == empresaId);

            var fileName = $"Kardex_{producto?.Sku ?? idProducto.ToString()}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("exportar/excel")]
    public async Task<IActionResult> ExportarMovimientosExcel(
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
        int empresaId = GetEmpresaId();
        var fileBytes = await _exportService.ExportarMovimientosExcel(empresaId, fechaInicio, fechaFin);
        var fileName = $"Movimientos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
