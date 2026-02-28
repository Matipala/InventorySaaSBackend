using Microsoft.EntityFrameworkCore;
using InventorySaaSBackend.Data;
using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Services;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    public ProductoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Productos>> ObtenerTodos(int idEmpresa)
    {
        return await _context.Productos
            .Where(p => p.IdEmpresa == idEmpresa)
            .ToListAsync();
    }

    public async Task<Productos?> ObtenerPorId(int id, int idEmpresa)
    {
        return await _context.Productos
            .FirstOrDefaultAsync(p => p.IdProducto == id && p.IdEmpresa == idEmpresa);
    }

    public async Task<(bool exito, string mensaje, Productos? producto)> Crear(Productos producto, int idEmpresa)
    {
        producto.IdEmpresa = idEmpresa;

        bool existeSku = await _context.Productos
            .AnyAsync(p => p.Sku == producto.Sku && p.IdEmpresa == idEmpresa);

        if (existeSku)
            return (false, "El SKU ya está registrado en esta empresa", null);

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return (true, "Producto creado exitosamente", producto);
    }

    public async Task<(bool exito, string mensaje, Productos? producto)> Actualizar(int id, Productos productoActualizado, int idEmpresa)
    {
        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.IdProducto == id && p.IdEmpresa == idEmpresa);

        if (producto == null)
            return (false, "Producto no encontrado", null);

        bool skuEnUso = await _context.Productos
            .AnyAsync(p => p.Sku == productoActualizado.Sku &&
                          p.IdEmpresa == idEmpresa &&
                          p.IdProducto != id);

        if (skuEnUso)
            return (false, "El SKU ya está en uso por otro producto", null);

        producto.Nombre = productoActualizado.Nombre;
        producto.Sku = productoActualizado.Sku;
        producto.IdCategoria = productoActualizado.IdCategoria;
        producto.Activo = productoActualizado.Activo;

        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();

        return (true, "Producto actualizado exitosamente", producto);
    }

    public async Task<Productos?> CambiarEstado(int id, bool activo, int idEmpresa)
    {
        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.IdProducto == id && p.IdEmpresa == idEmpresa);

        if (producto == null)
            return null;

        producto.Activo = activo;
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();

        return producto;
    }
}
