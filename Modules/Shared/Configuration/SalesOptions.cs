namespace InventorySaaSBackend.Modules.Shared.Configuration;

public class SalesOptions
{
    public const string SectionName = "Sales";
    public decimal GlobalTaxPercent { get; set; } = 0m;
}
