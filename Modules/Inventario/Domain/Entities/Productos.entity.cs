using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Models;

[Table("Productos")]
public class Productos
{
    [Key][Column("id_producto")] public int IdProducto { get; set; }
    [Column("id_empresa")] public int IdEmpresa { get; set; }
    [Column("sku")] public string Sku { get; set; } = string.Empty;
    [Column("nombre")] public string Nombre { get; set; } = string.Empty;
    [Column("id_categoria")] public int IdCategoria { get; set; } = 0;
    [Column("id_unidad")] public int? IdUnidad { get; set; }
    [Column("precio_venta", TypeName = "numeric(18,2)")] public decimal PrecioVenta { get; set; } = 0m;
    [Column("agotado_86")] public bool Agotado86 { get; set; } = false;
    [Column("activo")] public bool Activo { get; set; } = true;
}