namespace InventorySaaSBackend.Application.DTOs;

public class AjusteStockRequest
{
    public int IdProducto { get; set; }
    public int IdAlmacen { get; set; }
    public int NuevaCantidad { get; set; }
    public string Motivo { get; set; } = string.Empty;
}
