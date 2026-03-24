using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Models;

[Table("Categorias")]
public class Categoria
{
    [Key][Column("id_categoria")] public int IdCategoria { get; set; }
    [Column("id_empresa")] public int IdEmpresa { get; set; }
    [Column("nombre")] public string Nombre { get; set; } = string.Empty;
}