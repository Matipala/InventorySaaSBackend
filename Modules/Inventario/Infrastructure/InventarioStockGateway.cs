using InventorySaaSBackend.Modules.Inventario.Application;
using InventorySaaSBackend.Modules.Inventario.Application.Interfaces;

namespace InventorySaaSBackend.Modules.Inventario.Infrastructure;

public class InventarioStockGateway : IStockGateway
{
    private readonly IInventarioService _inventarioService;

    public InventarioStockGateway(IInventarioService inventarioService)
    {
        _inventarioService = inventarioService;
    }

    public Task<bool> ValidarStockDisponibleAsync(int idProducto, int idAlmacen, int cantidad, int idEmpresa)
    {
        return _inventarioService.ValidarStockDisponible(idProducto, idAlmacen, cantidad, idEmpresa);
    }

    public Task<(bool exito, string mensaje)> DescontarStockVentaAsync(int idProducto, int idAlmacen, int cantidad, int idEmpresa)
    {
        return _inventarioService.CrearMovimiento(idProducto, idAlmacen, cantidad, "SALIDA", idEmpresa);
    }
}
