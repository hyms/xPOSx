export interface Warehouse {
  id?: number
  name: string
  city?: string
  mobile?: string
  email?: string
  country?: string
}

export interface Category {
  id?: number
  code: string
  name: string
  description?: string
}

export interface Unit {
  id?: number
  name: string
  shortName: string
  baseUnit?: number
  operator?: string
  operatorValue?: number
}

export interface Product {
  id?: number
  code: string
  name: string
  cost: number
  price: number
  categoryId?: number
  unitId?: number
  unitSaleId?: number
  unitPurchaseId?: number
  taxNet?: number
  taxMethod?: string
  note?: string
  stockAlert?: number
  isVariant?: boolean
  notSelling?: boolean
  isActive?: boolean
  image?: string
}