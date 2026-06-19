namespace InventorySaaSBackend.Application.DTOs;

public class StockInicialRequest
{
    public Guid IdProducto { get; set; }
    public Guid IdAlmacen { get; set; }
    public int Cantidad { get; set; }
}
