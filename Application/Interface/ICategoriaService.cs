using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Application.Interface;

public interface ICategoriaService
{
    Task<IEnumerable<Categoria>> ObtenerTodos(Guid idEmpresa);
    Task<Categoria?> ObtenerPorId(Guid id, Guid idEmpresa);
    Task<(bool exito, string mensaje, Categoria? categoria)> Crear(Categoria categoria, Guid idEmpresa);
    Task<(bool exito, string mensaje, Categoria? categoria)> Actualizar(Guid id, Categoria categoria, Guid idEmpresa);
}
