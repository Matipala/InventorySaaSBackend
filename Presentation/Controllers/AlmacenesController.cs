using Microsoft.AspNetCore.Mvc;
using InventorySaaSBackend.Controllers;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlmacenesController : BaseController
{
    private readonly IAlmacenService _almacenService;

    public AlmacenesController(IAlmacenService almacenService)
    {
        _almacenService = almacenService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        int empresaId = GetEmpresaId();
        var almacenes = await _almacenService.ObtenerTodos(empresaId);
        return Ok(almacenes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        int empresaId = GetEmpresaId();
        var almacen = await _almacenService.ObtenerPorId(id, empresaId);

        if (almacen == null)
            return NotFound("Almacén no encontrado");

        return Ok(almacen);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Almacenes nuevoAlmacen)
    {
        int empresaId = GetEmpresaId();
        var almacen = await _almacenService.Crear(nuevoAlmacen, empresaId);
        return CreatedAtAction(nameof(GetById), new { id = almacen.IdAlmacen }, almacen);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Almacenes almacenActualizado)
    {
        int empresaId = GetEmpresaId();
        var almacen = await _almacenService.Actualizar(id, almacenActualizado, empresaId);

        if (almacen == null)
            return NotFound("Almacén no encontrado");

        return Ok(almacen);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        int empresaId = GetEmpresaId();
        var resultado = await _almacenService.Eliminar(id, empresaId);

        if (!resultado.exito)
            return BadRequest(resultado.mensaje);

        return Ok(new { mensaje = resultado.mensaje });
    }
}