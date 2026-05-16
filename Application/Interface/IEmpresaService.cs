using InventorySaaSBackend.Models;

namespace InventorySaaSBackend.Services;

public interface IEmpresaService
{
    Task<IEnumerable<Empresas>> ObtenerTodos();
    Task<Empresas?> ObtenerPorId(int id);
    Task<Empresas> Crear(Empresas empresa);
    Task<Empresas?> Actualizar(int id, Empresas empresa);
    Task<Empresas?> CambiarEstado(int id, bool activo);
}
