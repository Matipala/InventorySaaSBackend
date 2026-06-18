namespace InventorySaaSBackend.Application.Interface;

public interface IInventarioService
{
    Task<(bool exito, string mensaje)> CrearMovimiento(Guid idProducto, Guid idAlmacen, int cantidad, string tipo, Guid idEmpresa, Guid? idAlmacenDestino = null, string? motivo = null);
    Task<int> ObtenerStockActual(Guid idProducto, Guid idAlmacen, Guid idEmpresa);
    Task<bool> ValidarStockDisponible(Guid idProducto, Guid idAlmacen, int cantidad, Guid idEmpresa);
    Task ActualizarStock(Guid idProducto, Guid idAlmacen, int cantidad, Guid idEmpresa);
    Task<(bool exito, string mensaje)> AjusteManualStock(Guid idProducto, Guid idAlmacen, int nuevaCantidad, string motivo, Guid idEmpresa);
}
