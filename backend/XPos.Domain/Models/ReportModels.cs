namespace XPos.Domain.Models;

public class SalesReportDto
{
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

public class PurchaseReportDto
{
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class StockReportDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal StockAlert { get; set; }
}

public class ProfitLossReportDto
{
    public decimal TotalSales { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal NetProfit => TotalSales - TotalPurchases - TotalExpenses - TotalReturns;
}

public class RecentSaleDto
{
    public string Ref { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DashboardSummaryDto
{
    public decimal TodaySales { get; set; }
    public int TodaySalesCount { get; set; }
    public decimal MonthlySales { get; set; }
    public decimal MonthlyPurchases { get; set; }
    public List<RecentSaleDto> RecentSales { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
}

public class TopProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Total { get; set; }
}

public class ProductMovementReportDto
{
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // 'Sale', 'Purchase', 'Transfer', 'Adjustment'
    public decimal Quantity { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
}

public class StockAlertReportDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal StockAlert { get; set; }
}

public class ActivityReportDto
{
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
}


public class ClientReportDto
{
    public long ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalSales { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal DueAmount => TotalAmount - TotalPaid; // Deuda pendiente
}

public class ProviderReportDto
{
    public long ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalPurchases { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal DueAmount => TotalAmount - TotalPaid; // Deuda pendiente
}
