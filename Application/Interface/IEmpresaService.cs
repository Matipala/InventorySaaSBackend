using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Services;

public interface IEmpresaService
{
    Task<IEnumerable<Empresas>> ObtenerTodos();
    Task<Empresas?> ObtenerPorId(Guid id);
    Task<Empresas> Crear(Empresas empresa);
    Task<Empresas?> Actualizar(Guid id, Empresas empresa);
    Task<Empresas?> CambiarEstado(Guid id, bool activo);
}
