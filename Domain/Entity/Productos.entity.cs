using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Models;

[Table("Productos")]
public class Productos
{
    [Key][Column("id_producto")] public Guid IdProducto { get; set; } = Guid.NewGuid();
    [Column("id_empresa")] public Guid IdEmpresa { get; set; }
    [Column("sku")] public string Sku { get; set; } = string.Empty;
    [Column("nombre")] public string Nombre { get; set; } = string.Empty;
    [Column("id_categoria")] public Guid IdCategoria { get; set; }
    [Column("id_unidad")] public Guid? IdUnidad { get; set; }
    [Column("precio_venta", TypeName = "numeric(18,2)")] public decimal PrecioVenta { get; set; } = 0m;
    [Column("agotado_86")] public bool Agotado86 { get; set; } = false;
    [Column("activo")] public bool Activo { get; set; } = true;
    [Column("estacion")] public string? Estacion { get; set; }
}