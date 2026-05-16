namespace InventorySaaSBackend.Application.DTOs;

public class StockInicialRequest
{
    public int IdProducto { get; set; }
    public int IdAlmacen { get; set; }
    public int Cantidad { get; set; }
}
