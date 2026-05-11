export interface QuotationDetail {
  productId: number
  productVariantId?: number
  saleUnitId?: number
  name?: string
  price: number
  taxNet?: number
  taxMethod?: string
  discount?: number
  discountMethod?: string
  quantity: number
  total: number
}

export interface Quotation {
  id?: number
  ref?: string
  date: string
  clientId: number
  warehouseId: number
  taxRate?: number
  taxNet?: number
  discount?: number
  shipping?: number
  grandTotal: number
  status: string
  notes?: string
  details: QuotationDetail[]
}

export interface QuotationReadDto {
  id: number
  ref: string
  date: string
  clientName: string
  warehouseName: string
  grandTotal: number
  status: string
}