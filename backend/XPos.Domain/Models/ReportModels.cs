namespace XPos.Domain.Models;

public class SalesReportDto
{
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public double GrandTotal { get; set; }
    public double PaidAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
}

public class PurchaseReportDto
{
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public double GrandTotal { get; set; }
    public double PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class StockReportDto
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public double StockAlert { get; set; }
}

public class ProfitLossReportDto
{
    public double TotalSales { get; set; }
    public double TotalPurchases { get; set; }
    public double TotalExpenses { get; set; }
    public double TotalReturns { get; set; }
    public double NetProfit => TotalSales - TotalPurchases - TotalExpenses - TotalReturns;
}

public class RecentSaleDto
{
    public string Ref { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public double GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DashboardSummaryDto
{
    public double TodaySales { get; set; }
    public int TodaySalesCount { get; set; }
    public double MonthlySales { get; set; }
    public double MonthlyPurchases { get; set; }
    public List<RecentSaleDto> RecentSales { get; set; } = new();
    public List<TopProductDto> TopProducts { get; set; } = new();
}

public class TopProductDto
{
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public double Total { get; set; }
}

public class ProductMovementReportDto
{
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // 'Sale', 'Purchase', 'Transfer', 'Adjustment'
    public double Quantity { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
}

public class StockAlertReportDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public double StockAlert { get; set; }
}

public class ActivityReportDto
{
    public DateTime Date { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Total { get; set; }
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
    public double TotalAmount { get; set; }
    public double TotalPaid { get; set; }
    public double DueAmount => TotalAmount - TotalPaid; // Deuda pendiente
}

public class ProviderReportDto
{
    public long ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalPurchases { get; set; }
    public double TotalAmount { get; set; }
    public double TotalPaid { get; set; }
    public double DueAmount => TotalAmount - TotalPaid; // Deuda pendiente
}
