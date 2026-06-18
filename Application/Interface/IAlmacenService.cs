using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Application.Interface;

public interface IAlmacenService
{
    Task<IEnumerable<Almacenes>> ObtenerTodos(Guid idEmpresa);
    Task<Almacenes?> ObtenerPorId(Guid id, Guid idEmpresa);
    Task<Almacenes> Crear(Almacenes almacen, Guid idEmpresa);
    Task<Almacenes?> Actualizar(Guid id, Almacenes almacen, Guid idEmpresa);
    Task<(bool exito, string mensaje)> Eliminar(Guid id, Guid idEmpresa);
}
