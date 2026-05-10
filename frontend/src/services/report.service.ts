import api from '@/api'
import type { 
  SalesReportDto, PurchaseReportDto, 
  StockReportDto, ProfitLossReportDto, 
  DashboardSummaryDto, ClientReportDto, 
  ProviderReportDto, TopProductDto,
  StockAlertReportDto, ActivityReportDto
} from '@/types'

export const reportService = {
  getSalesReport: (params?: any) => api.get<SalesReportDto[]>('/reports/sales', { params }),
  getPurchaseReport: (params?: any) => api.get<PurchaseReportDto[]>('/reports/purchases', { params }),
  getStockReport: (params?: any) => api.get<StockReportDto[]>('/reports/stock', { params }),
  getProfitLossReport: (params?: any) => api.get<ProfitLossReportDto>('/reports/profit-loss', { params }),
  getDashboardSummary: () => api.get<DashboardSummaryDto>('/reports/dashboard-summary'),
  getClientReport: () => api.get<ClientReportDto[]>('/reports/clients'),
  getProviderReport: () => api.get<ProviderReportDto[]>('/reports/providers'),
  getTopProductsReport: (params?: any) => api.get<TopProductDto[]>('/reports/top-products', { params }),
  getStockAlertsReport: () => api.get<StockAlertReportDto[]>('/reports/stock-alerts'),
  getActivityReport: (params?: any) => api.get<ActivityReportDto[]>('/reports/activity', { params })
}
