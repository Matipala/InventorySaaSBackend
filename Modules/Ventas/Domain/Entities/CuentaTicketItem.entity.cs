using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Modules.Ventas.Domain.Entities;

[Table("Ventas_CuentasTicketItems")]
public class CuentaTicketItem
{
    [Key][Column("id_cuenta_ticket_item")] public int IdCuentaTicketItem { get; set; }
    [Column("id_cuenta_ticket")] public int IdCuentaTicket { get; set; }
    [Column("id_producto")] public int IdProducto { get; set; }
    [Column("cantidad")] public int Cantidad { get; set; }
    [Column("precio_unitario")] public decimal PrecioUnitario { get; set; }
    [Column("subtotal")] public decimal Subtotal { get; set; }
    [Column("nota")] public string? Nota { get; set; }
    [Column("comanda_enviada")] public bool ComandaEnviada { get; set; } = false;
}
