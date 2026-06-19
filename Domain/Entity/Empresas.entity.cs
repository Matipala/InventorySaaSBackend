using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Models;

[Table("Empresas")]

public class Empresas
{
    [Key][Column("id_empresa")] public Guid IdEmpresa { get; set; } = Guid.NewGuid();
    [Column("nombre")] public string Nombre { get; set; } = string.Empty;
    [Column("activo")] public bool Activo { get; set; } = true;
}