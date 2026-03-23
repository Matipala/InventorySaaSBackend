using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using InventorySaaSBackend.Data;
using InventorySaaSBackend.Modules.Inventario.Application;
using InventorySaaSBackend.Modules.Shared.Configuration;
using InventorySaaSBackend.Modules.Ventas.Application;
using InventorySaaSBackend.Modules.Ventas.Application.DTOs;
using InventorySaaSBackend.Modules.Ventas.Domain.Entities;

namespace InventorySaaSBackend.Modules.Ventas.Infrastructure;

public class CuentaTicketService : ICuentaTicketService
{
    private readonly ApplicationDbContext _context;
    private readonly IStockGateway _stockGateway;
    private readonly SalesOptions _salesOptions;

    public CuentaTicketService(
        ApplicationDbContext context,
        IStockGateway stockGateway,
        IOptions<SalesOptions> salesOptions)
    {
        _context = context;
        _stockGateway = stockGateway;
        _salesOptions = salesOptions.Value;
    }

    public async Task<(bool exito, string mensaje, CuentaTicketResponse? cuenta)> CrearCuentaAsync(
        CrearCuentaTicketRequest request,
        int idEmpresa)
    {
        if (request.IdAlmacen <= 0)
            return (false, "El almacen es obligatorio.", null);

        if (string.IsNullOrWhiteSpace(request.Mesero))
            return (false, "El mesero es obligatorio.", null);

        var almacenExiste = await _context.Almacenes
            .AnyAsync(a => a.IdAlmacen == request.IdAlmacen && a.IdEmpresa == idEmpresa);

        if (!almacenExiste)
            return (false, "Almacen no encontrado para la empresa.", null);

        var ultimoNumero = await _context.CuentasTickets
            .Where(c => c.IdEmpresa == idEmpresa)
            .Select(c => (int?)c.Numero)
            .MaxAsync() ?? 0;

        var cuenta = new CuentaTicket
        {
            IdEmpresa = idEmpresa,
            IdAlmacen = request.IdAlmacen,
            IdCliente = request.IdCliente,
            Numero = ultimoNumero + 1,
            Mesero = request.Mesero.Trim(),
            Estado = "ABIERTO",
            Subtotal = 0m,
            Impuesto = 0m,
            Total = 0m,
            FechaCreacion = DateTime.UtcNow
        };

        _context.CuentasTickets.Add(cuenta);
        await _context.SaveChangesAsync();

        return (true, "Cuenta creada exitosamente.", MapCuenta(cuenta));
    }

    public async Task<(bool exito, string mensaje, CuentaTicketResponse? cuenta)> AgregarItemAsync(
        int idCuentaTicket,
        AgregarCuentaTicketItemRequest request,
        int idEmpresa)
    {
        if (request.Cantidad <= 0)
            return (false, "La cantidad debe ser mayor a cero.", null);

        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null)
            return (false, "Cuenta no encontrada.", null);

        if (cuenta.Estado != "ABIERTO")
            return (false, "Solo se pueden agregar items en cuentas abiertas.", null);

        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.IdProducto == request.IdProducto && p.IdEmpresa == idEmpresa);

        if (producto == null)
            return (false, "Producto no encontrado.", null);

        if (!producto.Activo)
            return (false, "No se puede agregar un producto inactivo.", null);

        if (producto.Agotado86)
            return (false, "El producto esta marcado como agotado (86).", null);

        var precioUnitario = request.PrecioUnitario ?? producto.PrecioVenta;
        if (precioUnitario < 0)
            return (false, "El precio unitario no puede ser negativo.", null);

        var item = new CuentaTicketItem
        {
            IdCuentaTicket = cuenta.IdCuentaTicket,
            IdProducto = producto.IdProducto,
            Cantidad = request.Cantidad,
            PrecioUnitario = precioUnitario,
            Subtotal = precioUnitario * request.Cantidad,
            Nota = request.Nota,
            ComandaEnviada = false
        };

        _context.CuentasTicketItems.Add(item);
        await _context.SaveChangesAsync();

        await RecalcularTotalesAsync(cuenta);

        return (true, "Item agregado exitosamente.", MapCuenta(cuenta));
    }

    public async Task<(bool exito, string mensaje, CuentaTicketResponse? cuenta)> PagarCuentaAsync(
        int idCuentaTicket,
        PagarCuentaTicketRequest request,
        int idEmpresa)
    {
        var metodosValidos = new[] { "EFECTIVO", "QR", "TARJETA" };
        var metodo = (request.MetodoPago ?? string.Empty).Trim().ToUpperInvariant();

        if (!metodosValidos.Contains(metodo))
            return (false, "Metodo de pago invalido. Use: EFECTIVO, QR o TARJETA.", null);

        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        if (cuenta == null)
            return (false, "Cuenta no encontrada.", null);

        if (cuenta.Estado != "ABIERTO")
            return (false, "Solo las cuentas abiertas pueden pagarse.", null);

        var items = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == idCuentaTicket)
            .ToListAsync();

        if (items.Count == 0)
            return (false, "No se puede pagar una cuenta sin items.", null);

        var agrupado = items
            .GroupBy(i => i.IdProducto)
            .Select(g => new { IdProducto = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .ToList();

        foreach (var linea in agrupado)
        {
            var hayStock = await _stockGateway.ValidarStockDisponibleAsync(
                linea.IdProducto,
                cuenta.IdAlmacen,
                linea.Cantidad,
                idEmpresa);

            if (!hayStock)
                return (false, $"Stock insuficiente para el producto {linea.IdProducto}.", null);
        }

        foreach (var linea in agrupado)
        {
            var descuento = await _stockGateway.DescontarStockVentaAsync(
                linea.IdProducto,
                cuenta.IdAlmacen,
                linea.Cantidad,
                idEmpresa);

            if (!descuento.exito)
                return (false, descuento.mensaje, null);
        }

        var pago = new Pago
        {
            IdEmpresa = idEmpresa,
            IdCuentaTicket = cuenta.IdCuentaTicket,
            MetodoPago = metodo,
            Monto = cuenta.Total,
            FechaPago = DateTime.UtcNow
        };

        cuenta.Estado = "PAGADO";
        cuenta.FechaPago = DateTime.UtcNow;

        _context.Pagos.Add(pago);
        _context.CuentasTickets.Update(cuenta);
        await _context.SaveChangesAsync();

        return (true, "Pago registrado exitosamente.", MapCuenta(cuenta));
    }

    public async Task<CuentaTicketResponse?> ObtenerCuentaAsync(int idCuentaTicket, int idEmpresa)
    {
        var cuenta = await _context.CuentasTickets
            .FirstOrDefaultAsync(c => c.IdCuentaTicket == idCuentaTicket && c.IdEmpresa == idEmpresa);

        return cuenta == null ? null : MapCuenta(cuenta);
    }

    private async Task RecalcularTotalesAsync(CuentaTicket cuenta)
    {
        var subtotal = await _context.CuentasTicketItems
            .Where(i => i.IdCuentaTicket == cuenta.IdCuentaTicket)
            .SumAsync(i => i.Subtotal);

        var impuesto = Math.Round(subtotal * (_salesOptions.GlobalTaxPercent / 100m), 2, MidpointRounding.AwayFromZero);

        cuenta.Subtotal = subtotal;
        cuenta.Impuesto = impuesto;
        cuenta.Total = subtotal + impuesto;

        _context.CuentasTickets.Update(cuenta);
        await _context.SaveChangesAsync();
    }

    private static CuentaTicketResponse MapCuenta(CuentaTicket cuenta)
    {
        return new CuentaTicketResponse
        {
            IdCuentaTicket = cuenta.IdCuentaTicket,
            Numero = cuenta.Numero,
            Estado = cuenta.Estado,
            IdAlmacen = cuenta.IdAlmacen,
            Mesero = cuenta.Mesero,
            Subtotal = cuenta.Subtotal,
            Impuesto = cuenta.Impuesto,
            Total = cuenta.Total,
            FechaCreacion = cuenta.FechaCreacion,
            FechaPago = cuenta.FechaPago
        };
    }
}
