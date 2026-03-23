using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Modules.Inventario.Application.Interfaces;

public interface IAlmacenService
{
    Task<IEnumerable<Almacenes>> ObtenerTodos(int idEmpresa);
    Task<Almacenes?> ObtenerPorId(int id, int idEmpresa);
    Task<Almacenes> Crear(Almacenes almacen, int idEmpresa);
    Task<Almacenes?> Actualizar(int id, Almacenes almacen, int idEmpresa);
    Task<(bool exito, string mensaje)> Eliminar(int id, int idEmpresa);
}
