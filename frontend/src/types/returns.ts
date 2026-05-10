import type { Voucher } from './fiscal'

export interface SaleReturnDetail {
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

export interface SaleReturn {
  id?: number
  ref?: string
  date: string
  saleId?: number
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
  details: SaleReturnDetail[]
  voucher?: Voucher
}

export interface SaleReturnReadDto {
  id: number
  ref: string
  date: string
  clientName: string
  warehouseName: string
  grandTotal: number
  status: string
  paymentStatus: string
  voucherId?: number
}

export interface PurchaseReturnDetail {
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

export interface PurchaseReturn {
  id?: number
  ref?: string
  date: string
  purchaseId?: number
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
  details: PurchaseReturnDetail[]
  voucher?: Voucher
}

export interface PurchaseReturnReadDto {
  id: number
  ref: string
  date: string
  providerName: string
  warehouseName: string
  grandTotal: number
  status: string
  paymentStatus: string
  voucherId?: number
}