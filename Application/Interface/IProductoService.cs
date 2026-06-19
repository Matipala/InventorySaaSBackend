using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Application.Interface;

public interface IProductoService
{
    Task<IEnumerable<Productos>> ObtenerTodos(Guid idEmpresa);
    Task<IEnumerable<Productos>> BuscarFiltrados(Guid idEmpresa, string? q, Guid? idCategoria, Guid? idUnidad, bool? activo);
    Task<Productos?> ObtenerPorId(Guid id, Guid idEmpresa);
    Task<(bool exito, string mensaje, Productos? producto)> Crear(Productos producto, Guid idEmpresa);
    Task<(bool exito, string mensaje, Productos? producto)> Actualizar(Guid id, Productos producto, Guid idEmpresa);
    Task<Productos?> CambiarEstado(Guid id, bool activo, Guid idEmpresa);
    Task<Productos?> CambiarAgotado(Guid id, bool agotado, Guid idEmpresa);
}
