namespace InventorySaaSBackend.Services;

public interface IExportService
{
    Task<byte[]> ExportarProductosExcel(Guid idEmpresa);
    Task<byte[]> ExportarStockExcel(Guid idEmpresa);
    Task<byte[]> ExportarMovimientosExcel(Guid idEmpresa, DateTime? fechaInicio, DateTime? fechaFin);
    Task<byte[]> ExportarKardexExcel(Guid idProducto, Guid idAlmacen, Guid idEmpresa, DateTime? fechaInicio, DateTime? fechaFin);
}
