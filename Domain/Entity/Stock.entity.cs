using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Models;

[Table("Stock")]

public class Stock
{
    [Key][Column("id_stock")] public Guid IdStock { get; set; } = Guid.NewGuid();
    [Column("id_empresa")] public Guid IdEmpresa { get; set; }
    [Column("id_producto")] public Guid IdProducto { get; set; }
    [Column("id_almacen")] public Guid IdAlmacen { get; set; }
    [Column("cantidad")] public int Cantidad { get; set; } = 0;
}