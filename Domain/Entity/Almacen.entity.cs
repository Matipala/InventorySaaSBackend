using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Models;

[Table("Almacenes")]

public class Almacenes
{
    [Key][Column("id_almacen")] public Guid IdAlmacen { get; set; } = Guid.NewGuid();
    [Column("id_empresa")] public Guid IdEmpresa { get; set; }
    [Column("nombre")] public string Nombre { get; set; } = string.Empty;
}