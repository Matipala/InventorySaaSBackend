namespace InventorySaaSBackend.Application.DTOs;

public class RestockEvent
{
    public string CompanyCen { get; set; } = string.Empty;
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string WarehouseCen { get; set; } = string.Empty;
    public string EventType { get; set; } = "RESTOCK";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
