using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Application.Interface;

public interface IProductoService
{
    Task<IEnumerable<Productos>> ObtenerTodos(int idEmpresa);
    Task<IEnumerable<Productos>> BuscarFiltrados(int idEmpresa, string? q, int? idCategoria, int? idUnidad, bool? activo);
    Task<Productos?> ObtenerPorId(int id, int idEmpresa);
    Task<(bool exito, string mensaje, Productos? producto)> Crear(Productos producto, int idEmpresa);
    Task<(bool exito, string mensaje, Productos? producto)> Actualizar(int id, Productos producto, int idEmpresa);
    Task<Productos?> CambiarEstado(int id, bool activo, int idEmpresa);
    Task<Productos?> CambiarAgotado(int id, bool agotado, int idEmpresa);
}
