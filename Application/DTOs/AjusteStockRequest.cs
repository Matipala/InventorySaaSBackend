namespace InventorySaaSBackend.Application.DTOs;

public class AjusteStockRequest
{
    public Guid IdProducto { get; set; }
    public Guid IdAlmacen { get; set; }
    public int NuevaCantidad { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
