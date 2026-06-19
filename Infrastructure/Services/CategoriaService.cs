using Microsoft.EntityFrameworkCore;
using InventorySaaSBackend.Infrastructure.Data;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Infrastructure.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ApplicationDbContext _context;

    public CategoriaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Categoria>> ObtenerTodos(Guid idEmpresa)
    {
        return await _context.Categorias
            .Where(c => c.IdEmpresa == idEmpresa)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<Categoria?> ObtenerPorId(Guid id, Guid idEmpresa)
    {
        return await _context.Categorias
            .FirstOrDefaultAsync(c => c.IdCategoria == id && c.IdEmpresa == idEmpresa);
    }

    public async Task<(bool exito, string mensaje, Categoria? categoria)> Crear(Categoria categoria, Guid idEmpresa)
    {
        if (string.IsNullOrWhiteSpace(categoria.Nombre))
            return (false, "El nombre de la categoría es obligatorio", null);

        categoria.IdEmpresa = idEmpresa;
        categoria.IdCategoria = Guid.NewGuid();

        bool existe = await _context.Categorias
            .AnyAsync(c => c.Nombre == categoria.Nombre && c.IdEmpresa == idEmpresa);

        if (existe)
            return (false, "Ya existe una categoría con ese nombre", null);

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return (true, "Categoría creada exitosamente", categoria);
    }

    public async Task<(bool exito, string mensaje, Categoria? categoria)> Actualizar(Guid id, Categoria categoriaActualizada, Guid idEmpresa)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.IdCategoria == id && c.IdEmpresa == idEmpresa);

        if (categoria == null)
            return (false, "Categoría no encontrada", null);

        if (string.IsNullOrWhiteSpace(categoriaActualizada.Nombre))
            return (false, "El nombre de la categoría es obligatorio", null);

        bool nombreEnUso = await _context.Categorias
            .AnyAsync(c => c.Nombre == categoriaActualizada.Nombre &&
                          c.IdEmpresa == idEmpresa &&
                          c.IdCategoria != id);

        if (nombreEnUso)
            return (false, "Ya existe otra categoría con ese nombre", null);

        categoria.Nombre = categoriaActualizada.Nombre;

        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();

        return (true, "Categoría actualizada exitosamente", categoria);
    }
}
