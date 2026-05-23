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
    public int Quantity { get; set; }
    public string AdjustmentType { get; set; } = "INCREASE"; // INCREASE, DECREASE, SET
}

public class StockValidationContractRequest
{
    public string WarehouseCen { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ReferenceCen { get; set; } = string.Empty;
    public List<StockValidationItemContractDto> Items { get; set; } = new();
}

public class StockValidationItemContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public int Quantity { get; set; }
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
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int MissingQuantity { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string Reason { get; set; } = "INSUFFICIENT_STOCK";
}

public class StockConsumeContractRequest
{
    public string WarehouseCen { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string ReferenceCen { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public List<StockValidationItemContractDto> Items { get; set; } = new();
}

public class InventoryDocumentContractRequest
{
    public string DocumentType { get; set; } = "ENTRY"; // ENTRY, EXIT, SALE_EXIT
    public string WarehouseCen { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public List<InventoryDocumentLineContractDto> Lines { get; set; } = new();
}

public class InventoryDocumentLineContractDto
{
    public string ProductCen { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
