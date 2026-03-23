using InventorySaaSBackend.Modules.Inventario.Domain.Entities;

namespace InventorySaaSBackend.Modules.Inventario.Application.Interfaces;

public interface IUnidadService
{
    Task<IEnumerable<UnidadMedida>> ObtenerTodos(int idEmpresa);
    Task<UnidadMedida?> ObtenerPorId(int idUnidad, int idEmpresa);
    Task<(bool exito, string mensaje, UnidadMedida? unidad)> Crear(UnidadMedida unidad, int idEmpresa);
    Task<(bool exito, string mensaje, UnidadMedida? unidad)> Actualizar(int idUnidad, UnidadMedida unidad, int idEmpresa);
    Task<UnidadMedida?> CambiarEstado(int idUnidad, bool activo, int idEmpresa);
}
