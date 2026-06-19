using Microsoft.AspNetCore.Mvc;
using InventorySaaSBackend.Models;
using InventorySaaSBackend.Services;
using InventorySaaSBackend.Controllers;

namespace InventorySaaSBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmpresaController : BaseController
{
    private readonly IEmpresaService _empresaService;

    public EmpresaController(IEmpresaService empresaService)
    {
        _empresaService = empresaService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var lista = await _empresaService.ObtenerTodos();
        return Ok(lista);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var empresa = await _empresaService.ObtenerPorId(id);

        if (empresa == null)
            return NotFound("Empresa no encontrada");

        return Ok(empresa);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Empresas nuevaEmpresa)
    {
        var empresa = await _empresaService.Crear(nuevaEmpresa);
        return CreatedAtAction(nameof(GetById), new { id = empresa.IdEmpresa }, empresa);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, Empresas empresaActualizada)
    {
        var empresa = await _empresaService.Actualizar(id, empresaActualizada);

        if (empresa == null)
            return NotFound("Empresa no encontrada");

        return Ok(empresa);
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] bool activo)
    {
        var empresa = await _empresaService.CambiarEstado(id, activo);

        if (empresa == null)
            return NotFound("Empresa no encontrada");

        return Ok(new { mensaje = $"Empresa {(activo ? "activada" : "desactivada")} exitosamente", empresa });
    }
}
