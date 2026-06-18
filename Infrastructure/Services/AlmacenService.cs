using Microsoft.EntityFrameworkCore;
using InventorySaaSBackend.Infrastructure.Data;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Infrastructure.Services;

public class AlmacenService : IAlmacenService
{
    private readonly ApplicationDbContext _context;

    public AlmacenService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Almacenes>> ObtenerTodos(Guid idEmpresa)
    {
        return await _context.Almacenes
            .Where(a => a.IdEmpresa == idEmpresa)
            .ToListAsync();
    }

    public async Task<Almacenes?> ObtenerPorId(Guid id, Guid idEmpresa)
    {
        return await _context.Almacenes
            .FirstOrDefaultAsync(a => a.IdAlmacen == id && a.IdEmpresa == idEmpresa);
    }

    public async Task<Almacenes> Crear(Almacenes almacen, Guid idEmpresa)
    {
        almacen.IdEmpresa = idEmpresa;
        almacen.IdAlmacen = Guid.NewGuid();
        _context.Almacenes.Add(almacen);
        await _context.SaveChangesAsync();
        return almacen;
    }

    public async Task<Almacenes?> Actualizar(Guid id, Almacenes almacenActualizado, Guid idEmpresa)
    {
        var almacen = await _context.Almacenes
            .FirstOrDefaultAsync(a => a.IdAlmacen == id && a.IdEmpresa == idEmpresa);

        if (almacen == null)
            return null;

        almacen.Nombre = almacenActualizado.Nombre;

        _context.Almacenes.Update(almacen);
        await _context.SaveChangesAsync();

        return almacen;
    }

    public async Task<(bool exito, string mensaje)> Eliminar(Guid id, Guid idEmpresa)
    {
        var almacen = await _context.Almacenes
            .FirstOrDefaultAsync(a => a.IdAlmacen == id && a.IdEmpresa == idEmpresa);

        if (almacen == null)
            return (false, "Almacén no encontrado");

        bool tieneStock = await _context.Stock
            .AnyAsync(s => s.IdAlmacen == id && s.IdEmpresa == idEmpresa && s.Cantidad > 0);

        if (tieneStock)
            return (false, "No se puede eliminar el almacén porque tiene productos con stock");

        bool tieneMovimientos = await _context.Movimientos
            .AnyAsync(m => m.IdAlmacen == id && m.IdEmpresa == idEmpresa);

        if (tieneMovimientos)
            return (false, "No se puede eliminar el almacén porque tiene movimientos registrados");

        _context.Almacenes.Remove(almacen);
        await _context.SaveChangesAsync();

        return (true, "Almacén eliminado exitosamente");
    }
}
