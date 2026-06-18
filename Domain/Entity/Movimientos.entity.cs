using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Models;

[Table("Movimientos")]

public class Movimientos
{
    [Key][Column("id_movimiento")] public Guid IdMovimiento { get; set; } = Guid.NewGuid();
    [Column("id_empresa")] public Guid IdEmpresa { get; set; }
    [Column("id_producto")] public Guid IdProducto { get; set; }
    [Column("id_almacen")] public Guid IdAlmacen { get; set; }
    [Column("tipo")] public string Tipo { get; set; } = string.Empty;
    [Column("fecha")] public DateTime Fecha { get; set; } = DateTime.Now;
    [Column("cantidad")] public int Cantidad { get; set; } = 0;
    [Column("motivo")] public string? Motivo { get; set; }
}