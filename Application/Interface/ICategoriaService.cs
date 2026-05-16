using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Application.Interface;

public interface ICategoriaService
{
    Task<IEnumerable<Categoria>> ObtenerTodos(int idEmpresa);
    Task<Categoria?> ObtenerPorId(int id, int idEmpresa);
    Task<(bool exito, string mensaje, Categoria? categoria)> Crear(Categoria categoria, int idEmpresa);
    Task<(bool exito, string mensaje, Categoria? categoria)> Actualizar(int id, Categoria categoria, int idEmpresa);
}
