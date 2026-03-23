namespace InventorySaaSBackend.Modules.Inventario.Application;

public interface IStockGateway
{
    Task<bool> ValidarStockDisponibleAsync(int idProducto, int idAlmacen, int cantidad, int idEmpresa);
    Task<(bool exito, string mensaje)> DescontarStockVentaAsync(int idProducto, int idAlmacen, int cantidad, int idEmpresa);
}
