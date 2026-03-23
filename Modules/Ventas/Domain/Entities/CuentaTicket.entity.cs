using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Modules.Ventas.Domain.Entities;

[Table("Ventas_CuentasTickets")]
public class CuentaTicket
{
    [Key][Column("id_cuenta_ticket")] public int IdCuentaTicket { get; set; }
    [Column("id_empresa")] public int IdEmpresa { get; set; }
    [Column("id_almacen")] public int IdAlmacen { get; set; }
    [Column("id_cliente")] public int? IdCliente { get; set; }
    [Column("numero")] public int Numero { get; set; }
    [Column("mesero")] public string Mesero { get; set; } = string.Empty;
    [Column("estado")] public string Estado { get; set; } = "ABIERTO";
    [Column("subtotal")] public decimal Subtotal { get; set; } = 0m;
    [Column("impuesto")] public decimal Impuesto { get; set; } = 0m;
    [Column("total")] public decimal Total { get; set; } = 0m;
    [Column("fecha_creacion")] public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    [Column("fecha_pago")] public DateTime? FechaPago { get; set; }
}
