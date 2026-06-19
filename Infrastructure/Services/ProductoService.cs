using Microsoft.EntityFrameworkCore;
using InventorySaaSBackend.Infrastructure.Data;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Infrastructure.Services;

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    public ProductoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Productos>> ObtenerTodos(Guid idEmpresa)
    {
        return await _context.Productos
            .Where(p => p.IdEmpresa == idEmpresa)
            .ToListAsync();
    }

    public async Task<IEnumerable<Productos>> BuscarFiltrados(Guid idEmpresa, string? q, Guid? idCategoria, Guid? idUnidad, bool? activo)
    {
        var query = _context.Productos.Where(p => p.IdEmpresa == idEmpresa);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var criterio = q.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(criterio) || p.Sku.ToLower().Contains(criterio));
        }

        if (idCategoria.HasValue)
            query = query.Where(p => p.IdCategoria == idCategoria.Value);

        if (idUnidad.HasValue)
            query = query.Where(p => p.IdUnidad == idUnidad.Value);

        if (activo.HasValue)
            query = query.Where(p => p.Activo == activo.Value);

        return await query
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Productos?> ObtenerPorId(Guid id, Guid idEmpresa)
    {
        return await _context.Productos
            .FirstOrDefaultAsync(p => p.IdProducto == id && p.IdEmpresa == idEmpresa);
    }

    public async Task<(bool exito, string mensaje, Productos? producto)> Crear(Productos producto, Guid idEmpresa)
    {
        if (string.IsNullOrWhiteSpace(producto.Nombre))
            return (false, "El nombre del producto es obligatorio", null);

        if (string.IsNullOrWhiteSpace(producto.Sku))
            return (false, "El SKU del producto es obligatorio", null);

        if (producto.PrecioVenta <= 0)
            return (false, "El precio de venta debe ser mayor a cero", null);

        if (!producto.IdUnidad.HasValue)
            return (false, "La unidad es obligatoria", null);

        var categoriaExiste = await _context.Categorias
            .AnyAsync(c => c.IdCategoria == producto.IdCategoria && c.IdEmpresa == idEmpresa);

        if (!categoriaExiste)
            return (false, "La categoria seleccionada no existe", null);

        var unidadExiste = await _context.Unidades
            .AnyAsync(u => u.IdUnidad == producto.IdUnidad.Value && u.IdEmpresa == idEmpresa && u.Activo);

        if (!unidadExiste)
            return (false, "La unidad seleccionada no existe o esta inactiva", null);

        producto.IdEmpresa = idEmpresa;
        producto.IdProducto = Guid.NewGuid();
        producto.Nombre = producto.Nombre.Trim();
        producto.Sku = producto.Sku.Trim();

        bool existeSku = await _context.Productos
            .AnyAsync(p => p.Sku == producto.Sku && p.IdEmpresa == idEmpresa);

        if (existeSku)
            return (false, "El SKU ya está registrado en esta empresa", null);

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return (true, "Producto creado exitosamente", producto);
    }

    public async Task<(bool exito, string mensaje, Productos? producto)> Actualizar(Guid id, Productos productoActualizado, Guid idEmpresa)
    {
        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.IdProducto == id && p.IdEmpresa == idEmpresa);

        if (producto == null)
            return (false, "Producto no encontrado", null);

        if (string.IsNullOrWhiteSpace(productoActualizado.Nombre))
            return (false, "El nombre del producto es obligatorio", null);

        if (string.IsNullOrWhiteSpace(productoActualizado.Sku))
            return (false, "El SKU del producto es obligatorio", null);

        if (productoActualizado.PrecioVenta <= 0)
            return (false, "El precio de venta debe ser mayor a cero", null);

        if (!productoActualizado.IdUnidad.HasValue)
            return (false, "La unidad es obligatoria", null);

        var categoriaExiste = await _context.Categorias
            .AnyAsync(c => c.IdCategoria == productoActualizado.IdCategoria && c.IdEmpresa == idEmpresa);

        if (!categoriaExiste)
            return (false, "La categoria seleccionada no existe", null);

        var unidadExiste = await _context.Unidades
            .AnyAsync(u => u.IdUnidad == productoActualizado.IdUnidad.Value && u.IdEmpresa == idEmpresa && u.Activo);

        if (!unidadExiste)
            return (false, "La unidad seleccionada no existe o esta inactiva", null);

        bool skuEnUso = await _context.Productos
            .AnyAsync(p => p.Sku == productoActualizado.Sku &&
                          p.IdEmpresa == idEmpresa &&
                          p.IdProducto != id);

        if (skuEnUso)
            return (false, "El SKU ya está en uso por otro producto", null);

        producto.Nombre = productoActualizado.Nombre.Trim();
        producto.Sku = productoActualizado.Sku.Trim();
        producto.IdCategoria = productoActualizado.IdCategoria;
        producto.IdUnidad = productoActualizado.IdUnidad;
        producto.PrecioVenta = productoActualizado.PrecioVenta;
        producto.Agotado86 = productoActualizado.Agotado86;
        producto.Activo = productoActualizado.Activo;
        producto.Estacion = productoActualizado.Estacion;

        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();

        return (true, "Producto actualizado exitosamente", producto);
    }

    public async Task<Productos?> CambiarEstado(Guid id, bool activo, Guid idEmpresa)
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

    public async Task<Productos?> CambiarAgotado(Guid id, bool agotado, Guid idEmpresa)
    {
        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.IdProducto == id && p.IdEmpresa == idEmpresa);

        if (producto == null)
            return null;

        producto.Agotado86 = agotado;
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();

        return producto;
    }
}
