using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Modules.Inventario.Domain.Entities;

[Table("Unidades")]
public class UnidadMedida
{
    [Key][Column("id_unidad")] public int IdUnidad { get; set; }
    [Column("id_empresa")] public int IdEmpresa { get; set; }
    [Column("nombre")] public string Nombre { get; set; } = string.Empty;
    [Column("abreviatura")] public string Abreviatura { get; set; } = string.Empty;
    [Column("activo")] public bool Activo { get; set; } = true;
}
