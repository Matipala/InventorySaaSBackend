using Microsoft.AspNetCore.Mvc;

namespace InventorySaaSBackend.Controllers;

public class BaseController : ControllerBase
{
    protected Guid GetEmpresaId()
    {
        if (Request.Headers.TryGetValue("x-empresa-id", out var headerValue))
        {
            if (Guid.TryParse(headerValue, out Guid empresaId))
            {
                return empresaId;
            }
        }

        throw new BadHttpRequestException("Es obligatorio enviar el encabezado 'x-empresa-id'.");
    }
}
