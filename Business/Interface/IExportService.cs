namespace InventorySaaSBackend.Services;

public interface IExportService
{
    Task<byte[]> ExportarProductosExcel(int idEmpresa);
    Task<byte[]> ExportarStockExcel(int idEmpresa);
    Task<byte[]> ExportarMovimientosExcel(int idEmpresa, DateTime? fechaInicio, DateTime? fechaFin);
    Task<byte[]> ExportarKardexExcel(int idProducto, int idAlmacen, int idEmpresa, DateTime? fechaInicio, DateTime? fechaFin);
}
