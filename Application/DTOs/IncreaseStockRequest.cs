namespace InventorySaaSBackend.Application.DTOs;

public class IncreaseStockRequest
{
    public List<IncreaseStockItemRequest> Items { get; set; } = new();
}
