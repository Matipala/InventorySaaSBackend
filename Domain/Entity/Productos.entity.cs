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
    [Column("activo")] public bool Activo { get; set; } = true;
}