import type { Voucher } from './fiscal'

export interface TransferDetail {
  productId: number
  productVariantId?: number
  purchaseUnitId?: number
  cost: number
  quantity: number
  total: number
}

export interface Transfer {
  id?: number
  ref?: string
  date: string
  fromWarehouseId: number
  toWarehouseId: number
  items: number
  grandTotal: number
  status: string
  notes?: string
  details: TransferDetail[]
}

export interface TransferReadDto {
  id: number
  ref: string
  date: string
  fromWarehouseName: string
  toWarehouseName: string
  items: number
  grandTotal: number
  status: string
}

export interface PurchaseDetail {
  productId: number
  productVariantId?: number
  purchaseUnitId?: number
  cost: number
  taxNet?: number
  taxMethod?: string
  discount?: number
  discountMethod?: string
  quantity: number
  total: number
  productName?: string
}

export interface Purchase {
  id?: number
  ref?: string
  date: string
  providerId: number
  warehouseId: number
  taxRate?: number
  taxNet?: number
  discount?: number
  shipping?: number
  grandTotal: number
  paidAmount: number
  status: string
  paymentStatus: string
  notes?: string
  details: PurchaseDetail[]
  voucher?: Voucher
}

export interface PurchaseReadDto {
  id: number
  ref: string
  date: string
  providerName: string
  warehouseName: string
  grandTotal: number
  paidAmount: number
  dueAmount: number
  status: string
  paymentStatus: string
  voucherId?: number
}

export interface AdjustmentDetail {
  productId: number
  productVariantId?: number
  quantity: number
  type: string // 'add' or 'sub'
}

export interface Adjustment {
  id?: number
  ref?: string
  date: string
  warehouseId: number
  items: number
  notes?: string
  details: AdjustmentDetail[]
}

export interface AdjustmentReadDto {
  id: number;
  ref: string;
  date: string;
  warehouseName: string;
  items: number;
}

export interface SaleDetail {
  productId: number
  productVariantId?: number
  saleUnitId?: number
  price: number
  taxNet?: number
  taxMethod?: string
  discount?: number
  discountMethod?: string
  quantity: number
  total: number
  productName?: string
}

export interface Sale {
  id?: number
  ref?: string
  date: string
  isPos: boolean
  clientId: number
  warehouseId: number
  taxRate?: number
  taxNet?: number
  discount?: number
  shipping?: number
  grandTotal: number
  paidAmount: number
  status: string
  paymentStatus: string
  notes?: string
  details: SaleDetail[]
  voucher?: Voucher
}

export interface SaleReadDto {
  id: number
  ref: string
  date: string
  clientName: string
  warehouseName: string
  grandTotal: number
  paidAmount: number
  status: string
  paymentStatus: string
  voucherId?: number
}

export interface ExpenseCategory {
  id?: number
  userId?: number
  name: string
  description?: string
}

export interface Expense {
  id?: number
  date: string
  ref?: string
  userId?: number
  expenseCategoryId: number
  warehouseId: number
  details: string
  amount: number
}

export interface ExpenseReadDto {
  id: number
  date: string
  ref: string
  categoryName: string
  warehouseName: string
  details: string
  amount: number
}