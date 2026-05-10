import api from '@/api'
import type { Purchase, PurchaseReadDto, PagingParams, PagedResult } from '@/types'

export const purchaseService = {
  getAll: (params: PagingParams) => {
    return api.get<PagedResult<PurchaseReadDto>>('/purchases', { params })
  },
  getById: (id: number) => api.get<Purchase>(`/purchases/${id}`),
  create: (purchase: Purchase) => api.post('/purchases', purchase),
  update: (id: number, purchase: Purchase) => api.put(`/purchases/${id}`, purchase),
  delete: (id: number) => api.delete(`/purchases/${id}`)
}
