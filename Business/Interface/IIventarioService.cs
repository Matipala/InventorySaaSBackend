namespace InventorySaaSBackend.Business.Interface;

public interface IInventarioService
{
    Task<(bool exito, string mensaje)> CrearMovimiento(int idProducto, int idAlmacen, int cantidad, string tipo, int idEmpresa, int? idAlmacenDestino = null);
    Task<int> ObtenerStockActual(int idProducto, int idAlmacen, int idEmpresa);
    Task<bool> ValidarStockDisponible(int idProducto, int idAlmacen, int cantidad, int idEmpresa);
    Task ActualizarStock(int idProducto, int idAlmacen, int cantidad, int idEmpresa);
    Task<(bool exito, string mensaje)> AjusteManualStock(int idProducto, int idAlmacen, int nuevaCantidad, string motivo, int idEmpresa);
}
