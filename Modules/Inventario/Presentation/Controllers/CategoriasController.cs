using Microsoft.AspNetCore.Mvc;
using InventorySaaSBackend.Controllers;
using InventorySaaSBackend.Modules.Inventario.Application.Interfaces;
using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Modules.Inventario.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : BaseController
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        int empresaId = GetEmpresaId();
        var categorias = await _categoriaService.ObtenerTodos(empresaId);
        return Ok(categorias);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        int empresaId = GetEmpresaId();
        var categoria = await _categoriaService.ObtenerPorId(id, empresaId);

        if (categoria == null)
            return NotFound("Categoría no encontrada");

        return Ok(categoria);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Categoria nuevaCategoria)
    {
        int empresaId = GetEmpresaId();
        var resultado = await _categoriaService.Crear(nuevaCategoria, empresaId);

        if (!resultado.exito)
            return BadRequest(resultado.mensaje);

        return CreatedAtAction(nameof(GetById), new { id = resultado.categoria!.IdCategoria }, resultado.categoria);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Categoria categoriaActualizada)
    {
        int empresaId = GetEmpresaId();
        var resultado = await _categoriaService.Actualizar(id, categoriaActualizada, empresaId);

        if (!resultado.exito)
        {
            if (resultado.mensaje == "Categoría no encontrada")
                return NotFound(resultado.mensaje);

            return BadRequest(resultado.mensaje);
        }

        return Ok(resultado.categoria);
    }

}
