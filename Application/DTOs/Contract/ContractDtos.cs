namespace InventorySaaSBackend.Application.DTOs.Contract;

public class CompanyContractDto
{
    public string CompanyCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CompanyLookupContractDto
{
    public int CompanyId { get; set; }
    public string CompanyCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class InventoryDashboardContractDto
{
    public string CompanyCen { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public double TotalStockQuantity { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
}

public class CategoryContractDto
{
    public string CategoryCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class UnitContractDto
{
    public string UnitCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
    public bool IsActive { get; set; }
}

public class WarehouseContractDto
{
    public string WarehouseCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class ProductContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryCen { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string UnitCen { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal ReorderLevel { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public string? StationCode { get; set; }
}

public class CreateProductContractRequest
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryCen { get; set; } = string.Empty;
    public string UnitCen { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal ReorderLevel { get; set; }
    public string? StationCode { get; set; }
}

public class CreateProductContractResponse
{
    public string ProductCen { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double InitialStock { get; set; }
}

public class UpdateProductContractRequest
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryCen { get; set; } = string.Empty;
    public string UnitCen { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal ReorderLevel { get; set; }
    public string? StationCode { get; set; }
}

public class UpdateProductStatusContractRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class ProductLookupContractRequest
{
    public List<string> ProductCens { get; set; } = new();
}

public class SellableProductContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CategoryCen { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public string? StationCode { get; set; }
}

public class StockContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseCen { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public double AvailableQuantity { get; set; }
    public double ReservedQuantity { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public double ReorderLevel { get; set; }
    public bool IsLowStock { get; set; }
}

public class KardexMovementContractDto
{
    public string MovementCen { get; set; } = string.Empty;
    public string? DocumentCen { get; set; }
    public string ProductCen { get; set; } = string.Empty;
    public string WarehouseCen { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public double? UnitCost { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InventoryDocumentContractDto
{
    public string DocumentCen { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int TotalItems { get; set; }
    public List<string> GeneratedMovementCens { get; set; } = new();
}
