using Microsoft.AspNetCore.Mvc;
using InventorySaaSBackend.Controllers;
using InventorySaaSBackend.Modules.Ventas.Application;
using InventorySaaSBackend.Modules.Ventas.Application.DTOs;

namespace InventorySaaSBackend.Modules.Ventas.Presentation;

[ApiController]
[Route("api/ventas/cuentas")]
public class CuentasTicketsController : BaseController
{
    private readonly ICuentaTicketService _cuentaTicketService;

    public CuentasTicketsController(ICuentaTicketService cuentaTicketService)
    {
        _cuentaTicketService = cuentaTicketService;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearCuentaTicketRequest request)
    {
        int empresaId = GetEmpresaId();
        var result = await _cuentaTicketService.CrearCuentaAsync(request, empresaId);

        if (!result.exito)
            return BadRequest(new { mensaje = result.mensaje });

        return Ok(result.cuenta);
    }

    [HttpPost("{idCuentaTicket:int}/items")]
    public async Task<IActionResult> AgregarItem(int idCuentaTicket, [FromBody] AgregarCuentaTicketItemRequest request)
    {
        int empresaId = GetEmpresaId();
        var result = await _cuentaTicketService.AgregarItemAsync(idCuentaTicket, request, empresaId);

        if (!result.exito)
            return BadRequest(new { mensaje = result.mensaje });

        return Ok(result.cuenta);
    }

    [HttpPost("{idCuentaTicket:int}/pagar")]
    public async Task<IActionResult> Pagar(int idCuentaTicket, [FromBody] PagarCuentaTicketRequest request)
    {
        int empresaId = GetEmpresaId();
        var result = await _cuentaTicketService.PagarCuentaAsync(idCuentaTicket, request, empresaId);

        if (!result.exito)
            return BadRequest(new { mensaje = result.mensaje });

        return Ok(result.cuenta);
    }

    [HttpGet("{idCuentaTicket:int}")]
    public async Task<IActionResult> GetById(int idCuentaTicket)
    {
        int empresaId = GetEmpresaId();
        var cuenta = await _cuentaTicketService.ObtenerCuentaAsync(idCuentaTicket, empresaId);

        if (cuenta == null)
            return NotFound(new { mensaje = "Cuenta no encontrada." });

        return Ok(cuenta);
    }
}
