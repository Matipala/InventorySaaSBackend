using Microsoft.AspNetCore.Mvc;

namespace InventorySaaSBackend.Controllers;

public class BaseController : ControllerBase
{
    protected int GetEmpresaId()
    {
        // 1. Buscamos en los Headers de la petición un dato llamado "x-empresa-id"
        if (Request.Headers.TryGetValue("x-empresa-id", out var headerValue))
        {
            // 2. Si existe, intentamos convertir ese texto a un número entero
            if (int.TryParse(headerValue, out int empresaId))
            {
                return empresaId;
            }
        }


        throw new BadHttpRequestException("Es obligatorio enviar el encabezado 'x-empresa-id'.");
    }
}