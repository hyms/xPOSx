export interface Voucher {
  id?: number
  saleId?: number
  purchaseId?: number
  saleReturnId?: number
  purchaseReturnId?: number
  voucherType: string
  voucherNumber: string
  issuedAt: string
  cae?: string
  caeExpiration?: string
}