namespace InventorySaaSBackend.Application.DTOs;

public class IncreaseStockItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Guid AlmacenId { get; set; }
    public Guid EmpresaId { get; set; }
}
