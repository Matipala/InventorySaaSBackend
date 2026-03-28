namespace InventorySaaSBackend.Application.DTOs;

public class ValidarStockRequest
{
    public int ProductoId { get; set; }
    public int AlmacenId { get; set; }
    public decimal Cantidad { get; set; }
}
