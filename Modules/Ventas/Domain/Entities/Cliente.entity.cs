using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySaaSBackend.Modules.Ventas.Domain.Entities;

[Table("Ventas_Clientes")]
public class Cliente
{
    [Key][Column("id_cliente")] public int IdCliente { get; set; }
    [Column("id_empresa")] public int IdEmpresa { get; set; }
    [Column("nombre")] public string Nombre { get; set; } = string.Empty;
    [Column("telefono")] public string Telefono { get; set; } = string.Empty;
}
