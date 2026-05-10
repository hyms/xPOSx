import api from '@/api'
import type { 
  SaleReturn, SaleReturnReadDto, 
  PurchaseReturn, PurchaseReturnReadDto 
} from '@/types'

export const returnService = {
  // Sale Returns
  getAllSaleReturns: () => api.get<SaleReturnReadDto[]>('/salereturns'),
  getSaleReturnById: (id: number) => api.get<SaleReturn>(`/salereturns/${id}`),
  createSaleReturn: (data: SaleReturn) => api.post('/salereturns', data),
  updateSaleReturn: (id: number, data: SaleReturn) => api.put(`/salereturns/${id}`, data),
  deleteSaleReturn: (id: number) => api.delete(`/salereturns/${id}`),

  // Purchase Returns
  getAllPurchaseReturns: () => api.get<PurchaseReturnReadDto[]>('/purchasereturns'),
  getPurchaseReturnById: (id: number) => api.get<PurchaseReturn>(`/purchasereturns/${id}`),
  createPurchaseReturn: (data: PurchaseReturn) => api.post('/purchasereturns', data),
  updatePurchaseReturn: (id: number, data: PurchaseReturn) => api.put(`/purchasereturns/${id}`, data),
  deletePurchaseReturn: (id: number) => api.delete(`/purchasereturns/${id}`)
}
