using Microsoft.AspNetCore.Mvc;

namespace InventorySaaSBackend.Controllers;

public class BaseController : ControllerBase
{
    protected Guid GetEmpresaId()
    {
        if (RouteData.Values.TryGetValue("companyCen", out var companyCenObj)
            && companyCenObj is string companyCen
            && Guid.TryParse(companyCen, out var empresaId)
            && empresaId != Guid.Empty)
        {
            return empresaId;
        }

        if (Request.Headers.TryGetValue("x-empresa-id", out var headerValue))
        {
            if (Guid.TryParse(headerValue, out var empresaIdFromHeader) && empresaIdFromHeader != Guid.Empty)
                return empresaIdFromHeader;
        }

        throw new BadHttpRequestException("Es obligatorio enviar el companyCen en la ruta o el header 'x-empresa-id'.");
    }
}
