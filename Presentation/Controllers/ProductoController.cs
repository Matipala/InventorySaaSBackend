using Microsoft.AspNetCore.Mvc;
using InventorySaaSBackend.Controllers;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Models;
using InventorySaaSBackend.Services;

namespace InventorySaaSBackend.Presentation.Controllers;

[ApiController]
[Route("api/{companyCen}/[controller]")]
[Route("api/[controller]")]
public class ProductosController : BaseController
{
    private readonly IProductoService _productoService;
    private readonly IExportService _exportService;

    public ProductosController(IProductoService productoService, IExportService exportService)
    {
        _productoService = productoService;
        _exportService = exportService;
    }

    [HttpGet()]
    public async Task<IActionResult> GetAll([FromQuery] string? q, [FromQuery] Guid? idCategoria, [FromQuery] Guid? idUnidad, [FromQuery] bool? activo)
    {
        Guid empresaId = GetEmpresaId();
        var productos = await _productoService.BuscarFiltrados(empresaId, q, idCategoria, idUnidad, activo);
        return Ok(productos);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Productos nuevoProducto)
    {
        Guid empresaId = GetEmpresaId();
        var resultado = await _productoService.Crear(nuevoProducto, empresaId);

        if (!resultado.exito)
            return BadRequest(resultado.mensaje);

        return Ok(resultado.producto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        Guid empresaId = GetEmpresaId();
        var producto = await _productoService.ObtenerPorId(id, empresaId);

        if (producto == null)
            return NotFound("Producto no encontrado o no pertenece a su empresa.");

        return Ok(producto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Productos productoActualizado)
    {
        Guid empresaId = GetEmpresaId();
        var resultado = await _productoService.Actualizar(id, productoActualizado, empresaId);

        if (!resultado.exito)
        {
            if (resultado.mensaje == "Producto no encontrado")
                return NotFound(resultado.mensaje);

            return BadRequest(resultado.mensaje);
        }

        return Ok(resultado.producto);
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] bool activo)
    {
        Guid empresaId = GetEmpresaId();
        var producto = await _productoService.CambiarEstado(id, activo, empresaId);

        if (producto == null)
            return NotFound("Producto no encontrado");

        return Ok(new { mensaje = $"Producto {(activo ? "activado" : "desactivado")} exitosamente", producto });
    }

    [HttpPatch("{id}/agotado")]
    public async Task<IActionResult> CambiarAgotado(Guid id, [FromBody] bool agotado)
    {
        Guid empresaId = GetEmpresaId();
        var producto = await _productoService.CambiarAgotado(id, agotado, empresaId);

        if (producto == null)
            return NotFound("Producto no encontrado");

        return Ok(new { mensaje = $"Producto marcado como {(agotado ? "agotado (86)" : "disponible")}", producto });
    }

    [HttpGet("exportar/excel")]
    public async Task<IActionResult> ExportarExcel()
    {
        Guid empresaId = GetEmpresaId();
        var fileBytes = await _exportService.ExportarProductosExcel(empresaId);
        var fileName = $"Productos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
