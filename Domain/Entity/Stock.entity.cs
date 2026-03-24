using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Models;

[Table("Stock")]

public class Stock
{
    [Key][Column("id_stock")] public int IdStock { get; set; }
    [Column("id_empresa")] public int IdEmpresa { get; set; }
    [Column("id_producto")] public int IdProducto { get; set; }
    [Column("id_almacen")] public int IdAlmacen { get; set; }
    [Column("cantidad")] public int Cantidad { get; set; } = 0;
}