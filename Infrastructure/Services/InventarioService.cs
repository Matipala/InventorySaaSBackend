using Microsoft.EntityFrameworkCore;
using InventorySaaSBackend.Infrastructure.Data;
using InventorySaaSBackend.Application.Interface;
using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Infrastructure.Services;

public class InventarioService : IInventarioService
{
    private readonly ApplicationDbContext _context;

    public InventarioService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool exito, string mensaje)> CrearMovimiento(
        Guid idProducto, Guid idAlmacen, int cantidad, string tipo, Guid idEmpresa, Guid? idAlmacenDestino = null, string? motivo = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == idProducto && p.IdEmpresa == idEmpresa);
            if (producto == null)
                return (false, "Producto no encontrado");

            var almacen = await _context.Almacenes
                .FirstOrDefaultAsync(a => a.IdAlmacen == idAlmacen && a.IdEmpresa == idEmpresa);
            if (almacen == null)
                return (false, "Almacen no encontrado");

            if (cantidad < 0)
                return (false, "La cantidad no puede ser negativa");

            var tiposValidos = new[] { "ENTRADA", "SALIDA", "TRANSFERENCIA" };
            if (!tiposValidos.Contains(tipo))
                return (false, "Tipo de movimiento invalido. Use: ENTRADA, SALIDA o TRANSFERENCIA");

            if (tipo == "SALIDA" || tipo == "TRANSFERENCIA")
            {
                var stockOk = await ValidarStockDisponible(idProducto, idAlmacen, cantidad, idEmpresa);
                if (!stockOk)
                    return (false, $"Stock insuficiente para el producto {producto.Nombre} (SKU: {producto.Sku})");
            }

            if (tipo == "TRANSFERENCIA")
            {
                if (!idAlmacenDestino.HasValue)
                    return (false, "Debe especificar el almacen destino para una transferencia");

                var almacenDestino = await _context.Almacenes
                    .FirstOrDefaultAsync(a => a.IdAlmacen == idAlmacenDestino.Value && a.IdEmpresa == idEmpresa);
                if (almacenDestino == null)
                    return (false, "Almacen destino no encontrado");

                _context.Movimientos.Add(new Movimientos
                {
                    IdEmpresa = idEmpresa,
                    IdProducto = idProducto,
                    IdAlmacen = idAlmacen,
                    Tipo = "SALIDA",
                    Fecha = DateTime.UtcNow,
                    Cantidad = cantidad,
                    Motivo = motivo
                });
                await ActualizarStock(idProducto, idAlmacen, -cantidad, idEmpresa);

                _context.Movimientos.Add(new Movimientos
                {
                    IdEmpresa = idEmpresa,
                    IdProducto = idProducto,
                    IdAlmacen = idAlmacenDestino.Value,
                    Tipo = "ENTRADA",
                    Fecha = DateTime.UtcNow,
                    Cantidad = cantidad,
                    Motivo = motivo
                });
                await ActualizarStock(idProducto, idAlmacenDestino.Value, cantidad, idEmpresa);
            }
            else
            {
                int impacto = tipo == "ENTRADA" ? cantidad : -cantidad;
                _context.Movimientos.Add(new Movimientos
                {
                    IdEmpresa = idEmpresa,
                    IdProducto = idProducto,
                    IdAlmacen = idAlmacen,
                    Tipo = tipo,
                    Fecha = DateTime.UtcNow,
                    Cantidad = cantidad,
                    Motivo = motivo
                });
                await ActualizarStock(idProducto, idAlmacen, impacto, idEmpresa);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, "Movimiento registrado exitosamente");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al crear movimiento: {ex.Message}");
        }
    }

    public async Task<int> ObtenerStockActual(Guid idProducto, Guid idAlmacen, Guid idEmpresa)
    {
        var stock = await _context.Stock
            .FirstOrDefaultAsync(s =>
                s.IdProducto == idProducto &&
                s.IdAlmacen == idAlmacen &&
                s.IdEmpresa == idEmpresa);

        return stock?.Cantidad ?? 0;
    }

    public async Task<bool> ValidarStockDisponible(Guid idProducto, Guid idAlmacen, int cantidad, Guid idEmpresa)
    {
        var stockActual = await ObtenerStockActual(idProducto, idAlmacen, idEmpresa);
        return stockActual >= cantidad;
    }

    public async Task ActualizarStock(Guid idProducto, Guid idAlmacen, int cantidad, Guid idEmpresa)
    {
        var stock = await _context.Stock
            .FirstOrDefaultAsync(s =>
                s.IdProducto == idProducto &&
                s.IdAlmacen == idAlmacen &&
                s.IdEmpresa == idEmpresa);

        if (stock == null)
        {
            stock = new Stock
            {
                IdEmpresa = idEmpresa,
                IdProducto = idProducto,
                IdAlmacen = idAlmacen,
                Cantidad = cantidad > 0 ? cantidad : 0
            };
            _context.Stock.Add(stock);
        }
        else
        {
            stock.Cantidad += cantidad;

            if (stock.Cantidad < 0)
                throw new InvalidOperationException("El stock no puede ser negativo");

            _context.Stock.Update(stock);
        }
    }

    public async Task<(bool exito, string mensaje)> AjusteManualStock(
        Guid idProducto, Guid idAlmacen, int nuevaCantidad, string motivo, Guid idEmpresa)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var stockActual = await ObtenerStockActual(idProducto, idAlmacen, idEmpresa);

            if (nuevaCantidad < 0)
                return (false, "La nueva cantidad no puede ser negativa");

            int diferencia = nuevaCantidad - stockActual;

            if (diferencia == 0)
                return (true, "No hay cambios sugeridos, el stock ya coincide.");

            _context.Movimientos.Add(new Movimientos
            {
                IdEmpresa = idEmpresa,
                IdProducto = idProducto,
                IdAlmacen = idAlmacen,
                Tipo = diferencia > 0 ? "AJUSTE_POSITIVO" : "AJUSTE_NEGATIVO",
                Fecha = DateTime.UtcNow,
                Cantidad = Math.Abs(diferencia),
                Motivo = motivo
            });

            await ActualizarStock(idProducto, idAlmacen, diferencia, idEmpresa);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (true, $"Ajuste realizado. Diferencia: {diferencia} unidades. Motivo: {motivo}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al realizar ajuste: {ex.Message}");
        }
    }
}
