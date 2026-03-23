using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Models;

[Table("Movimientos")]

public class Movimientos
{
    [Key][Column("id_movimiento")] public int IdMovimiento { get; set; }
    [Column("id_empresa")] public int IdEmpresa { get; set; }
    [Column("id_producto")] public int IdProducto { get; set; }
    [Column("id_almacen")] public int IdAlmacen { get; set; }
    [Column("tipo")] public string Tipo { get; set; } = string.Empty;
    [Column("fecha")] public DateTime Fecha { get; set; } = DateTime.Now;
    [Column("cantidad")] public int Cantidad { get; set; } = 0;
}