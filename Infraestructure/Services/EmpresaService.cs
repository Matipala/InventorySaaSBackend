using Microsoft.EntityFrameworkCore;
using InventorySaaSBackend.Data;
using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Services;

public class EmpresaService : IEmpresaService
{
    private readonly ApplicationDbContext _context;

    public EmpresaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Empresas>> ObtenerTodos()
    {
        return await _context.Empresas.ToListAsync();
    }

    public async Task<Empresas?> ObtenerPorId(int id)
    {
        return await _context.Empresas.FindAsync(id);
    }

    public async Task<Empresas> Crear(Empresas empresa)
    {
        empresa.Activo = true;
        _context.Empresas.Add(empresa);
        await _context.SaveChangesAsync();
        return empresa;
    }

    public async Task<Empresas?> Actualizar(int id, Empresas empresaActualizada)
    {
        var empresa = await _context.Empresas.FindAsync(id);

        if (empresa == null)
            return null;

        empresa.Nombre = empresaActualizada.Nombre;
        empresa.Activo = empresaActualizada.Activo;

        _context.Empresas.Update(empresa);
        await _context.SaveChangesAsync();

        return empresa;
    }

    public async Task<Empresas?> CambiarEstado(int id, bool activo)
    {
        var empresa = await _context.Empresas.FindAsync(id);

        if (empresa == null)
            return null;

        empresa.Activo = activo;
        _context.Empresas.Update(empresa);
        await _context.SaveChangesAsync();

        return empresa;
    }
}
