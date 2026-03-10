using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Models;

[Table("Empresas")]

public class Empresas
{
    [Key][Column("id_empresa")] public int IdEmpresa { get; set; }
    [Column("nombre")] public string Nombre { get; set; } = string.Empty;
    [Column("activo")] public bool Activo { get; set; } = true;
}