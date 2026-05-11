export interface SalesReportDto {
  date: string
  ref: string
  clientName: string
  warehouseName: string
  grandTotal: number
  paidAmount: number
  paymentStatus: string
}

export interface PurchaseReportDto {
  date: string
  ref: string
  providerName: string
  warehouseName: string
  grandTotal: number
  paidAmount: number
  status: string
}

export interface StockReportDto {
  productCode: string
  productName: string
  categoryName: string
  warehouseName: string
  quantity: number
  stockAlert: number
}

export interface ProfitLossReportDto {
  totalSales: number
  totalPurchases: number
  totalExpenses: number
  totalReturns: number
  netProfit: number
}

export interface DashboardSummaryDto {
  sales: number
  purchases: number
  expenses: number
  revenue: number
  todaySales: number
  todaySalesCount: number
  monthlySales: number
  monthlyPurchases: number
  recentSales: RecentSaleDto[]
  topProducts: TopProductDto[]
}

export interface TopProductDto {
  name: string
  quantity: number
  total: number
}

export interface RecentSaleDto {
  ref: string
  clientName: string
  grandTotal: number
  status: string
}

export interface ClientReportDto {
  clientId: number
  clientName: string
  phone: string
  email: string
  totalSales: number
  totalAmount: number
  totalPaid: number
  dueAmount: number
}

export interface ProviderReportDto {
  providerId: number
  providerName: string
  phone: string
  email: string
  totalPurchases: number
  totalAmount: number
  totalPaid: number
  dueAmount: number
}

export interface StockAlertReportDto {
  code: string
  name: string
  quantity: number
  stockAlert: number
}

export interface ActivityReportDto {
  date: string
  ref: string
  type: string
  total: number
  warehouseName: string
  userName: string
}