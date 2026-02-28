namespace InventorySaaSBackend.DTOs;

public class CrearMovimientoRequest
{
    public int IdProducto { get; set; }
    public int IdAlmacen { get; set; }
    public int Cantidad { get; set; }
    /// <summary>
    /// Tipos válidos: ENTRADA, SALIDA, TRANSFERENCIA
    /// </summary>
    public string Tipo { get; set; } = string.Empty;
    /// <summary>
    /// Requerido solo cuando Tipo = TRANSFERENCIA
    /// </summary>
    public int? IdAlmacenDestino { get; set; }
}
