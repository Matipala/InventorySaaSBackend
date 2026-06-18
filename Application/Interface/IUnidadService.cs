using InventorySaaSBackend.Domain.Entity;

namespace InventorySaaSBackend.Application.Interface;

public interface IUnidadService
{
    Task<IEnumerable<UnidadMedida>> ObtenerTodos(Guid idEmpresa);
    Task<UnidadMedida?> ObtenerPorId(Guid idUnidad, Guid idEmpresa);
    Task<(bool exito, string mensaje, UnidadMedida? unidad)> Crear(UnidadMedida unidad, Guid idEmpresa);
    Task<(bool exito, string mensaje, UnidadMedida? unidad)> Actualizar(Guid idUnidad, UnidadMedida unidad, Guid idEmpresa);
    Task<UnidadMedida?> CambiarEstado(Guid idUnidad, bool activo, Guid idEmpresa);
}
