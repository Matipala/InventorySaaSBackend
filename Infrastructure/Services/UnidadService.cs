using Microsoft.EntityFrameworkCore;
using InventorySaaSBackend.Infrastructure.Data;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Domain.Entity;

namespace InventorySaaSBackend.Infrastructure.Services;

public class UnidadService : IUnidadService
{
    private readonly ApplicationDbContext _context;

    public UnidadService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UnidadMedida>> ObtenerTodos(Guid idEmpresa)
    {
        return await _context.Unidades
            .Where(u => u.IdEmpresa == idEmpresa)
            .OrderBy(u => u.Nombre)
            .ToListAsync();
    }

    public async Task<UnidadMedida?> ObtenerPorId(Guid idUnidad, Guid idEmpresa)
    {
        return await _context.Unidades
            .FirstOrDefaultAsync(u => u.IdUnidad == idUnidad && u.IdEmpresa == idEmpresa);
    }

    public async Task<(bool exito, string mensaje, UnidadMedida? unidad)> Crear(UnidadMedida unidad, Guid idEmpresa)
    {
        if (string.IsNullOrWhiteSpace(unidad.Nombre))
            return (false, "El nombre de la unidad es obligatorio.", null);

        if (string.IsNullOrWhiteSpace(unidad.Abreviatura))
            return (false, "La abreviatura de la unidad es obligatoria.", null);

        var nombreNormalizado = unidad.Nombre.Trim();
        var abreviaturaNormalizada = unidad.Abreviatura.Trim();

        var existeNombre = await _context.Unidades
            .AnyAsync(u => u.IdEmpresa == idEmpresa && u.Nombre.ToLower() == nombreNormalizado.ToLower());

        if (existeNombre)
            return (false, "Ya existe una unidad con ese nombre.", null);

        unidad.IdEmpresa = idEmpresa;
        unidad.IdUnidad = Guid.NewGuid();
        unidad.Nombre = nombreNormalizado;
        unidad.Abreviatura = abreviaturaNormalizada;

        _context.Unidades.Add(unidad);
        await _context.SaveChangesAsync();

        return (true, "Unidad creada exitosamente.", unidad);
    }

    public async Task<(bool exito, string mensaje, UnidadMedida? unidad)> Actualizar(Guid idUnidad, UnidadMedida unidad, Guid idEmpresa)
    {
        var unidadDb = await _context.Unidades
            .FirstOrDefaultAsync(u => u.IdUnidad == idUnidad && u.IdEmpresa == idEmpresa);

        if (unidadDb == null)
            return (false, "Unidad no encontrada.", null);

        if (string.IsNullOrWhiteSpace(unidad.Nombre))
            return (false, "El nombre de la unidad es obligatorio.", null);

        if (string.IsNullOrWhiteSpace(unidad.Abreviatura))
            return (false, "La abreviatura de la unidad es obligatoria.", null);

        var nombreNormalizado = unidad.Nombre.Trim();
        var abreviaturaNormalizada = unidad.Abreviatura.Trim();

        var nombreEnUso = await _context.Unidades
            .AnyAsync(u => u.IdEmpresa == idEmpresa && u.IdUnidad != idUnidad && u.Nombre.ToLower() == nombreNormalizado.ToLower());

        if (nombreEnUso)
            return (false, "Ya existe otra unidad con ese nombre.", null);

        unidadDb.Nombre = nombreNormalizado;
        unidadDb.Abreviatura = abreviaturaNormalizada;
        unidadDb.Activo = unidad.Activo;

        _context.Unidades.Update(unidadDb);
        await _context.SaveChangesAsync();

        return (true, "Unidad actualizada exitosamente.", unidadDb);
    }

    public async Task<UnidadMedida?> CambiarEstado(Guid idUnidad, bool activo, Guid idEmpresa)
    {
        var unidad = await _context.Unidades
            .FirstOrDefaultAsync(u => u.IdUnidad == idUnidad && u.IdEmpresa == idEmpresa);

        if (unidad == null)
            return null;

        unidad.Activo = activo;
        _context.Unidades.Update(unidad);
        await _context.SaveChangesAsync();

        return unidad;
    }
}
