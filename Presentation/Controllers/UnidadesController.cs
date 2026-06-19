using Microsoft.AspNetCore.Mvc;
using InventorySaaSBackend.Controllers;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Domain.Entity;

namespace InventorySaaSBackend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UnidadesController : BaseController
{
    private readonly IUnidadService _unidadService;

    public UnidadesController(IUnidadService unidadService)
    {
        _unidadService = unidadService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        Guid empresaId = GetEmpresaId();
        var unidades = await _unidadService.ObtenerTodos(empresaId);
        return Ok(unidades);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        Guid empresaId = GetEmpresaId();
        var unidad = await _unidadService.ObtenerPorId(id, empresaId);

        if (unidad == null)
            return NotFound("Unidad no encontrada");

        return Ok(unidad);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UnidadMedida nuevaUnidad)
    {
        Guid empresaId = GetEmpresaId();
        var resultado = await _unidadService.Crear(nuevaUnidad, empresaId);

        if (!resultado.exito)
            return BadRequest(resultado.mensaje);

        return CreatedAtAction(nameof(GetById), new { id = resultado.unidad!.IdUnidad }, resultado.unidad);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UnidadMedida unidadActualizada)
    {
        Guid empresaId = GetEmpresaId();
        var resultado = await _unidadService.Actualizar(id, unidadActualizada, empresaId);

        if (!resultado.exito)
        {
            if (resultado.mensaje == "Unidad no encontrada.")
                return NotFound(resultado.mensaje);

            return BadRequest(resultado.mensaje);
        }

        return Ok(resultado.unidad);
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] bool activo)
    {
        Guid empresaId = GetEmpresaId();
        var unidad = await _unidadService.CambiarEstado(id, activo, empresaId);

        if (unidad == null)
            return NotFound("Unidad no encontrada");

        return Ok(new { mensaje = $"Unidad {(activo ? "activada" : "desactivada")} exitosamente", unidad });
    }
}
