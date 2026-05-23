namespace InventorySaaSBackend.Application.DTOs.Contract;

public class CompanyContractDto
{
    public string CompanyCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class InventoryDashboardContractDto
{
    public string CompanyCen { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int TotalStockQuantity { get; set; }
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
    public string CategoryCen { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string UnitCen { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public int ReorderLevel { get; set; }
}

public class StockContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseCen { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int ReorderLevel { get; set; }
    public bool IsLowStock { get; set; }
}
