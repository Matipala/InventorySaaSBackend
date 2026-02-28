using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Services;

public interface IProductoService
{
    Task<IEnumerable<Productos>> ObtenerTodos(int idEmpresa);
    Task<Productos?> ObtenerPorId(int id, int idEmpresa);
    Task<(bool exito, string mensaje, Productos? producto)> Crear(Productos producto, int idEmpresa);
    Task<(bool exito, string mensaje, Productos? producto)> Actualizar(int id, Productos producto, int idEmpresa);
    Task<Productos?> CambiarEstado(int id, bool activo, int idEmpresa);
}
