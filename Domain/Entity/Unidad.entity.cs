using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Domain.Entity;

[Table("Unidades")]
public class UnidadMedida
{
    [Key][Column("id_unidad")] public Guid IdUnidad { get; set; } = Guid.NewGuid();
    [Column("id_empresa")] public Guid IdEmpresa { get; set; }
    [Column("nombre")] public string Nombre { get; set; } = string.Empty;
    [Column("abreviatura")] public string Abreviatura { get; set; } = string.Empty;
    [Column("activo")] public bool Activo { get; set; } = true;
}
