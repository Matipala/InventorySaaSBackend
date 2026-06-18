namespace InventorySaaSBackend.Application.DTOs;

public class CrearMovimientoRequest
{
    public Guid IdProducto { get; set; }
    public Guid IdAlmacen { get; set; }
    public int Cantidad { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public Guid? IdAlmacenDestino { get; set; }
}
