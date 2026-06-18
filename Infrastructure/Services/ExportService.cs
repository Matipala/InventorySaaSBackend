using Microsoft.EntityFrameworkCore;
using InventorySaaSBackend.Infrastructure.Data;
using OfficeOpenXml;

namespace InventorySaaSBackend.Services;

public class ExportService : IExportService
{
    private readonly ApplicationDbContext _context;

    public ExportService(ApplicationDbContext context)
    {
        _context = context;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<byte[]> ExportarProductosExcel(Guid idEmpresa)
    {
        var productos = await _context.Productos
            .Where(p => p.IdEmpresa == idEmpresa)
            .Join(_context.Categorias,
                p => p.IdCategoria,
                c => c.IdCategoria,
                (p, c) => new
                {
                    p.IdProducto,
                    p.Sku,
                    p.Nombre,
                    Categoria = c.Nombre,
                    p.Activo
                })
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Productos");

        worksheet.Cells[1, 1].Value = "ID";
        worksheet.Cells[1, 2].Value = "SKU";
        worksheet.Cells[1, 3].Value = "Nombre";
        worksheet.Cells[1, 4].Value = "Categoría";
        worksheet.Cells[1, 5].Value = "Estado";

        using (var range = worksheet.Cells[1, 1, 1, 5])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        int row = 2;
        foreach (var producto in productos)
        {
            worksheet.Cells[row, 1].Value = producto.IdProducto;
            worksheet.Cells[row, 2].Value = producto.Sku;
            worksheet.Cells[row, 3].Value = producto.Nombre;
            worksheet.Cells[row, 4].Value = producto.Categoria;
            worksheet.Cells[row, 5].Value = producto.Activo ? "Activo" : "Inactivo";
            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        using var stream = new MemoryStream();
        package.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportarStockExcel(Guid idEmpresa)
    {
        var stock = await _context.Stock
            .Where(s => s.IdEmpresa == idEmpresa)
            .Join(_context.Productos,
                s => s.IdProducto,
                p => p.IdProducto,
                (s, p) => new { Stock = s, Producto = p })
            .Join(_context.Almacenes,
                sp => sp.Stock.IdAlmacen,
                a => a.IdAlmacen,
                (sp, a) => new { sp.Stock, sp.Producto, Almacen = a })
            .Join(_context.Categorias,
                spa => spa.Producto.IdCategoria,
                c => c.IdCategoria,
                (spa, c) => new
                {
                    ProductoSku = spa.Producto.Sku,
                    ProductoNombre = spa.Producto.Nombre,
                    Categoria = c.Nombre,
                    AlmacenNombre = spa.Almacen.Nombre,
                    Cantidad = spa.Stock.Cantidad,
                    Estado = spa.Producto.Activo ? "Activo" : "Inactivo"
                })
            .OrderBy(s => s.ProductoNombre)
            .ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Inventario");

        worksheet.Cells[1, 1].Value = "SKU";
        worksheet.Cells[1, 2].Value = "Producto";
        worksheet.Cells[1, 3].Value = "Categoría";
        worksheet.Cells[1, 4].Value = "Almacén";
        worksheet.Cells[1, 5].Value = "Cantidad";
        worksheet.Cells[1, 6].Value = "Estado";

        using (var range = worksheet.Cells[1, 1, 1, 6])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        }

        int row = 2;
        foreach (var item in stock)
        {
            worksheet.Cells[row, 1].Value = item.ProductoSku;
            worksheet.Cells[row, 2].Value = item.ProductoNombre;
            worksheet.Cells[row, 3].Value = item.Categoria;
            worksheet.Cells[row, 4].Value = item.AlmacenNombre;
            worksheet.Cells[row, 5].Value = item.Cantidad;
            worksheet.Cells[row, 6].Value = item.Estado;

            if (item.Cantidad < 10)
            {
                worksheet.Cells[row, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
            }

            if (item.Cantidad == 0)
            {
                worksheet.Cells[row, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[row, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                worksheet.Cells[row, 5].Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        int totalRow = row;
        worksheet.Cells[totalRow, 4].Value = "TOTAL:";
        worksheet.Cells[totalRow, 4].Style.Font.Bold = true;
        worksheet.Cells[totalRow, 5].Formula = $"SUM(E2:E{row - 1})";
        worksheet.Cells[totalRow, 5].Style.Font.Bold = true;

        using var stream = new MemoryStream();
        package.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportarMovimientosExcel(Guid idEmpresa, DateTime? fechaInicio, DateTime? fechaFin)
    {
        var query = _context.Movimientos
            .Where(m => m.IdEmpresa == idEmpresa);

        if (fechaInicio.HasValue)
            query = query.Where(m => m.Fecha >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(m => m.Fecha <= fechaFin.Value);

        var movimientos = await query
            .Join(_context.Productos,
                m => m.IdProducto,
                p => p.IdProducto,
                (m, p) => new { Movimiento = m, Producto = p })
            .Join(_context.Almacenes,
                mp => mp.Movimiento.IdAlmacen,
                a => a.IdAlmacen,
                (mp, a) => new
                {
                    mp.Movimiento.Fecha,
                    ProductoSku = mp.Producto.Sku,
                    ProductoNombre = mp.Producto.Nombre,
                    AlmacenNombre = a.Nombre,
                    mp.Movimiento.Tipo,
                    mp.Movimiento.Cantidad
                })
            .OrderByDescending(m => m.Fecha)
            .ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Movimientos");

        worksheet.Cells[1, 1].Value = "REPORTE DE MOVIMIENTOS";
        worksheet.Cells[1, 1].Style.Font.Size = 14;
        worksheet.Cells[1, 1].Style.Font.Bold = true;

        int infoRow = 2;
        if (fechaInicio.HasValue || fechaFin.HasValue)
        {
            worksheet.Cells[infoRow, 1].Value = "Período:";
            worksheet.Cells[infoRow, 2].Value = $"{fechaInicio?.ToString("dd/MM/yyyy") ?? "Inicio"} - {fechaFin?.ToString("dd/MM/yyyy") ?? "Hoy"}";
            infoRow++;
        }

        int headerRow = infoRow + 1;
        worksheet.Cells[headerRow, 1].Value = "Fecha";
        worksheet.Cells[headerRow, 2].Value = "SKU";
        worksheet.Cells[headerRow, 3].Value = "Producto";
        worksheet.Cells[headerRow, 4].Value = "Almacén";
        worksheet.Cells[headerRow, 5].Value = "Tipo";
        worksheet.Cells[headerRow, 6].Value = "Cantidad";

        using (var range = worksheet.Cells[headerRow, 1, headerRow, 6])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        }

        int row = headerRow + 1;
        foreach (var mov in movimientos)
        {
            worksheet.Cells[row, 1].Value = mov.Fecha.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cells[row, 2].Value = mov.ProductoSku;
            worksheet.Cells[row, 3].Value = mov.ProductoNombre;
            worksheet.Cells[row, 4].Value = mov.AlmacenNombre;
            worksheet.Cells[row, 5].Value = mov.Tipo;
            worksheet.Cells[row, 6].Value = mov.Cantidad;

            if (mov.Tipo == "ENTRADA")
            {
                worksheet.Cells[row, 6].Style.Font.Color.SetColor(System.Drawing.Color.Green);
            }
            else if (mov.Tipo == "SALIDA")
            {
                worksheet.Cells[row, 6].Style.Font.Color.SetColor(System.Drawing.Color.Red);
            }

            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        using var stream = new MemoryStream();
        package.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportarKardexExcel(Guid idProducto, Guid idAlmacen, Guid idEmpresa, DateTime? fechaInicio, DateTime? fechaFin)
    {
        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.IdProducto == idProducto && p.IdEmpresa == idEmpresa);

        if (producto == null)
            throw new Exception("Producto no encontrado");

        var query = _context.Movimientos
            .Where(m => m.IdProducto == idProducto && m.IdEmpresa == idEmpresa);

        if (idAlmacen != Guid.Empty)
            query = query.Where(m => m.IdAlmacen == idAlmacen);

        if (fechaInicio.HasValue)
            query = query.Where(m => m.Fecha >= fechaInicio.Value);

        if (fechaFin.HasValue)
            query = query.Where(m => m.Fecha <= fechaFin.Value);

        var movimientos = await query
            .Join(_context.Almacenes,
                m => m.IdAlmacen,
                a => a.IdAlmacen,
                (m, a) => new
                {
                    m.Fecha,
                    AlmacenNombre = a.Nombre,
                    m.Tipo,
                    m.Cantidad,
                    Entrada = m.Tipo == "ENTRADA" || m.Tipo == "AJUSTE_POSITIVO" ? m.Cantidad : 0,
                    Salida = m.Tipo == "SALIDA" || m.Tipo == "AJUSTE_NEGATIVO" ? m.Cantidad : 0
                })
            .OrderBy(m => m.Fecha)
            .ToListAsync();

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Kardex");

        worksheet.Cells[1, 1].Value = "KARDEX DE INVENTARIO";
        worksheet.Cells[1, 1].Style.Font.Size = 16;
        worksheet.Cells[1, 1].Style.Font.Bold = true;

        worksheet.Cells[2, 1].Value = "Producto:";
        worksheet.Cells[2, 2].Value = producto.Nombre;
        worksheet.Cells[2, 2].Style.Font.Bold = true;

        worksheet.Cells[3, 1].Value = "SKU:";
        worksheet.Cells[3, 2].Value = producto.Sku;

        worksheet.Cells[4, 1].Value = "Fecha de reporte:";
        worksheet.Cells[4, 2].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        int headerRow = 6;
        worksheet.Cells[headerRow, 1].Value = "Fecha";
        worksheet.Cells[headerRow, 2].Value = "Almacén";
        worksheet.Cells[headerRow, 3].Value = "Tipo";
        worksheet.Cells[headerRow, 4].Value = "Entradas";
        worksheet.Cells[headerRow, 5].Value = "Salidas";
        worksheet.Cells[headerRow, 6].Value = "Saldo";

        using (var range = worksheet.Cells[headerRow, 1, headerRow, 6])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thick);
        }

        int row = headerRow + 1;
        int saldo = 0;
        foreach (var mov in movimientos)
        {
            saldo += mov.Entrada - mov.Salida;

            worksheet.Cells[row, 1].Value = mov.Fecha.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cells[row, 2].Value = mov.AlmacenNombre;
            worksheet.Cells[row, 3].Value = mov.Tipo;
            worksheet.Cells[row, 4].Value = mov.Entrada;
            worksheet.Cells[row, 5].Value = mov.Salida;
            worksheet.Cells[row, 6].Value = saldo;

            worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0";
            worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0";
            worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0";

            if (saldo < 10)
            {
                worksheet.Cells[row, 6].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                worksheet.Cells[row, 6].Style.Font.Bold = true;
            }

            row++;
        }

        if (movimientos.Any())
        {
            int totalRow = row;
            worksheet.Cells[totalRow, 3].Value = "TOTALES:";
            worksheet.Cells[totalRow, 3].Style.Font.Bold = true;
            worksheet.Cells[totalRow, 4].Formula = $"SUM(D{headerRow + 1}:D{row - 1})";
            worksheet.Cells[totalRow, 5].Formula = $"SUM(E{headerRow + 1}:E{row - 1})";
            worksheet.Cells[totalRow, 6].Value = saldo;

            using (var range = worksheet.Cells[totalRow, 3, totalRow, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        using var stream = new MemoryStream();
        package.SaveAs(stream);
        return stream.ToArray();
    }
}
