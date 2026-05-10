export interface NotificationSetting {
  key: string;
  label: string;
  category: string;
  value: boolean;
}

export interface CurrencySetting {
  id: number;
  code: string;
  symbol: string;
}

export interface MailSetting {
  id: number;
  host: string;
  port: number;
  username: string;
  password: string;
  encryption: string;
  fromAddress: string;
  fromName: string;
}

export interface SmsSetting {
  id: number;
  sid: string;
  token: string;
  fromNumber: string;
}

export interface PaymentGatewaySetting {
  id: number;
  stripeKey: string;
  stripeSecret: string;
  paypalClientId: string;
  paypalClientSecret: string;
}

export interface Notification {
  id: number;
  message: string;
  createdAt: string;
  read: boolean;
}

export interface Setting {
  id?: number;
  key: string;
  value: string;
}

export interface Purchase {
  id?: number;
  ref?: string;
  date?: string;
  providerId?: number;
  warehouseId?: number;
  grandTotal: number;
  paidAmount: number;
  status: string;
  paymentStatus: string;
  details: PurchaseDetail[];
  voucher?: Partial<Voucher>;
  discount?: number;
  shipping?: number;
  voucherId?: number;
}

export interface PurchaseDetail {
  productId: number;
  cost: number;
  quantity: number;
  total: number;
  productName?: string;
}

export interface SaleReturn {
  id?: number;
  ref: string;
  date: string;
  clientId?: number;
  warehouseId?: number;
  grandTotal: number;
  paidAmount: number;
  status: string;
  paymentStatus: string;
  details: SaleReturnDetail[];
  voucher?: Partial<Voucher>;
}

export interface SaleReturnDetail {
  productId: number;
  price: number;
  quantity: number;
  total: number;
  productName?: string;
}

export interface Sale {
    id?: number;
    ref: string;
    date: string;
    clientId?: number;
    warehouseId?: number;
    grandTotal: number;
    paidAmount: number;
    status: string;
    paymentStatus: string;
    details: SaleDetail[];
    voucher?: Partial<Voucher>;
    isPos?: boolean;
    discount?: number;
    shipping?: number;
    taxRate?: number;
    notes?: string;
}

export interface SaleDetail {
    productId: number;
    quantity: number;
    price: number;
    total: number;
    productName?: string;
}

export interface Adjustment {
    id?: number;
    date: string;
    warehouseId: number;
    items: number;
    notes: string;
    details: AdjustmentDetail[];
}

export interface AdjustmentDetail {
    productId: number;
    quantity: number;
    type: 'add' | 'sub';
}

export interface AdjustmentReadDto {
    id: number;
    date: string;
    ref: string;
    warehouseName: string;
    items: number;
}

export interface Warehouse {
    id?: number;
    name: string;
    city: string;
    mobile: string;
    email: string;
    country: string;
}

export interface Product {
    id?: number;
    name: string;
    code: string;
    cost: number;
    price: number;
    categoryId?: number;
    unitId?: number;
    stockAlert: number;
    note: string;
    isActive: boolean;
    notSelling: boolean;
    image?: string;
}

export interface Category {
    id?: number;
    code: string;
    name: string;
}

export interface Client {
    id?: number;
    name: string;
    code: number;
    nitCi: string;
    phone: string;
    email: string;
    companyName: string;
    city: string;
    address: string;
}

export interface Expense {
    id?: number;
    date: string;
    expenseCategoryId: number;
    warehouseId: number;
    details: string;
    amount: number;
}

export interface ExpenseCategory {
    id?: number;
    name: string;
    description: string;
}

export interface ExpenseReadDto {
    id: number;
    date: string;
    ref: string;
    categoryName: string;
    warehouseName: string;
    amount: number;
}

export interface DashboardSummaryDto {
    sales: number;
    purchases: number;
    expenses: number;
    revenue: number;
    todaySales: number;
    todaySalesCount: number;
    monthlySales: number;
    monthlyPurchases: number;
    recentSales: SaleReadDto[];
    topProducts: TopProductDto[];
}

export interface User {
    id?: number;
    username: string;
    email: string;
    password?: string;
    newPassword?: string;
    firstName: string;
    lastName: string;
    role: number;
    isActive?: boolean;
    roleDetails?: Role;
}

export interface Provider {
    id?: number;
    name: string;
    code: number;
    email: string;
    phone: string;
    city: string;
    country: string;
    address: string;
}

export interface PurchaseReadDto {
    id: number;
    date: string;
    ref: string;
    providerName: string;
    warehouseName: string;
    grandTotal: number;
    status: string;
    paymentStatus: string;
    voucherId?: number;
}

export interface PagingParams {
    page: number;
    pageSize: number;
    search?: string;
    sortBy?: string;
    sortDescending?: boolean;
    filter?: any;
}

export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    totalItems?: number | undefined;
}

export interface Quotation {
    id?: number;
    ref?: string;
    date: string;
    clientId: number;
    warehouseId: number;
    grandTotal: number;
    status: string;
    details: QuotationDetail[];
}

export interface QuotationDetail {
    productId: number;
    name: string;
    price: number;
    quantity: number;
    total: number;
}

export interface QuotationReadDto {
    id: number;
    date: string;
    ref: string;
    clientName: string;
    warehouseName: string;
    grandTotal: number;
    status: string;
}

export interface SalesReportDto {
    date: string;
    ref: string;
    clientName: string;
    warehouseName: string;
    grandTotal: number;
}

export interface PurchaseReportDto {
    date: string;
    ref: string;
    providerName: string;
    warehouseName: string;
    grandTotal: number;
}

export interface StockReportDto {
    productCode: string;
    productName: string;
    categoryName: string;
    warehouseName: string;
    quantity: number;
    stockAlert: number;
}

export interface ProfitLossReportDto {
    totalSales: number;
    totalPurchases: number;
    totalExpenses: number;
    totalReturns: number;
    netProfit: number;
}

export interface ClientReportDto {
    clientId: number;
    clientName: string;
    phone: string;
    email: string;
    totalSales: number;
    totalAmount: number;
    totalPaid: number;
    dueAmount: number;
}

export interface ProviderReportDto {
    providerId: number;
    providerName: string;
    phone: string;
    email: string;
    totalPurchases: number;
    totalAmount: number;
    totalPaid: number;
    dueAmount: number;
}

export interface TopProductDto {
    name: string;
    quantity: number;
    total: number;
}

export interface StockAlertReportDto {
    code: string;
    name: string;
    quantity: number;
    stockAlert: number;
}

export interface ActivityReportDto {
    date: string;
    ref: string;
    type: string;
    warehouseName: string;
    userName: string;
    total: number;
}

export interface PurchaseReturn {
  id?: number;
  ref: string;
  date: string;
  providerId?: number;
  warehouseId?: number;
  grandTotal: number;
  paidAmount: number;
  status: string;
  paymentStatus: string;
  details: PurchaseReturnDetail[];
  voucher?: Partial<Voucher>;
}

export interface PurchaseReturnDetail {
  productId: number;
  cost: number;
  quantity: number;
  total: number;
  productName?: string;
}

export interface Voucher {
  id?: number;
  voucherType: string;
  voucherNumber: string;
  cae: string;
  caeExpiration: string;
}

export interface SaleReturnReadDto {
    id: number;
    date: string;
    ref: string;
    clientName: string;
    warehouseName: string;
    grandTotal: number;
    status: string;
}

export interface PurchaseReturnReadDto {
    id: number;
    date: string;
    ref: string;
    providerName: string;
    warehouseName: string;
    grandTotal: number;
    status: string;
}

export interface Role {
    id?: number;
    name: string;
    description: string;
    permissions?: number[];
}

export interface Permission {
    id: number;
    name: string;
    description: string;
}

export interface SaleReadDto {
    id: number;
    date: string;
    ref: string;
    clientName: string;
    warehouseName: string;
    grandTotal: number;
    status: string;
    paymentStatus: string;
}

export interface Transfer {
    id?: number;
    date: string;
    fromWarehouseId: number;
    toWarehouseId: number;
    items: number;
    grandTotal: number;
    status: string;
    notes: string;
    details: TransferDetail[];
}

export interface TransferDetail {
    productId: number;
    cost: number;
    quantity: number;
    total: number;
}

export interface TransferReadDto {
    id: number;
    date: string;
    ref: string;
    fromWarehouseName: string;
    toWarehouseName: string;
    items: number;
    grandTotal: number;
    status: string;
}

export interface Unit {
    id?: number;
    name: string;
    shortName: string;
    baseUnit?: number;
    operator: string;
    operatorValue: number;
}
