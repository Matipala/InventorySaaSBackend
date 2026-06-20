namespace InventorySaaSBackend.Application.DTOs.Contract;

public class StockAdjustmentContractRequest
{
    public string WarehouseCen { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public List<StockAdjustmentLineContractDto> Lines { get; set; } = new();
}

public class StockAdjustmentLineContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string AdjustmentType { get; set; } = "INCREASE"; // INCREASE, DECREASE, SET
}

public class StockValidationContractRequest
{
    public string WarehouseCen { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? ReferenceCen { get; set; }
    public List<StockValidationItemContractDto> Items { get; set; } = new();
}

public class StockValidationItemContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public double Quantity { get; set; }
}

public class StockValidationContractResponse
{
    public bool IsValid { get; set; }
    public List<StockRequirementContractDto> Requirements { get; set; } = new();
}

public class StockRequirementContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string WarehouseCen { get; set; } = string.Empty;
    public double RequestedQuantity { get; set; }
    public double AvailableQuantity { get; set; }
    public double MissingQuantity { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string Reason { get; set; } = "INSUFFICIENT_STOCK";
}

public class StockConsumeContractRequest
{
    public string WarehouseCen { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ReferenceCen { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public List<StockValidationItemContractDto> Items { get; set; } = new();
}

public class StockConsumeContractResponse
{
    public bool Success { get; set; }
    public string? DocumentCen { get; set; }
    public string? DocumentType { get; set; }
    public List<string> GeneratedMovementCens { get; set; } = new();
    public List<StockRequirementContractDto> Requirements { get; set; } = new();
}

public class StockIncreaseContractRequest
{
    public string WarehouseCen { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ReferenceCen { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public List<StockValidationItemContractDto> Items { get; set; } = new();
}

public class InventoryDocumentContractRequest
{
    public string DocumentType { get; set; } = "ENTRY"; // ENTRY, EXIT, SALE_EXIT
    public string WarehouseCen { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? ExternalReference { get; set; }
    public List<InventoryDocumentLineContractDto> Lines { get; set; } = new();
}

public class InventoryDocumentLineContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public decimal? UnitCost { get; set; }
}

public class InventoryAdjustmentContractRequest
{
    public string WarehouseCen { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public List<InventoryAdjustmentLineContractRequest> Lines { get; set; } = new();
}

public class InventoryAdjustmentLineContractRequest
{
    public string ProductCen { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string AdjustmentType { get; set; } = "SET"; // INCREASE, DECREASE, SET
}

public class InventoryAdjustmentContractResponse
{
    public string AdjustmentCen { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<GeneratedMovementContractDto> GeneratedMovements { get; set; } = new();
}

public class GeneratedMovementContractDto
{
    public string MovementCen { get; set; } = string.Empty;
    public string ProductCen { get; set; } = string.Empty;
    public string WarehouseCen { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string MovementType { get; set; } = string.Empty;
}

public class CreateCategoryContractRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateUnitContractRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
}
