using InventorySaaSBackend.Modules.Ventas.Application.DTOs;

namespace InventorySaaSBackend.Modules.Ventas.Application;

public interface ICuentaTicketService
{
    Task<(bool exito, string mensaje, CuentaTicketResponse? cuenta)> CrearCuentaAsync(CrearCuentaTicketRequest request, int idEmpresa);
    Task<(bool exito, string mensaje, CuentaTicketResponse? cuenta)> AgregarItemAsync(int idCuentaTicket, AgregarCuentaTicketItemRequest request, int idEmpresa);
    Task<(bool exito, string mensaje, CuentaTicketResponse? cuenta)> PagarCuentaAsync(int idCuentaTicket, PagarCuentaTicketRequest request, int idEmpresa);
    Task<CuentaTicketResponse?> ObtenerCuentaAsync(int idCuentaTicket, int idEmpresa);
}
