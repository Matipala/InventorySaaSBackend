namespace InventorySaaSBackend.Application.DTOs;

public class ValidarStockRequest
{
    public Guid ProductoId { get; set; }
    public Guid AlmacenId { get; set; }
    public decimal Cantidad { get; set; }
}
