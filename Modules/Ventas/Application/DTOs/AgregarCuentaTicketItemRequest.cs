namespace InventorySaaSBackend.Modules.Ventas.Application.DTOs;

public class AgregarCuentaTicketItemRequest
{
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public string? Nota { get; set; }
}
