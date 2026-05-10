import api from '@/api'
import type { Sale, SaleReadDto, PagedResult } from '@/types'

export interface GetAllSalesParams {
  page?: number
  rowsPerPage?: number
  sortBy?: string
  descending?: boolean
  filter?: string
}

export const saleService = {
  getAll: (params: GetAllSalesParams) => {
    return api.get<PagedResult<SaleReadDto>>('/sales', { params })
  },
  getById: (id: number) => api.get<Sale>(`/sales/${id}`),
  create: (sale: Sale) => api.post('/sales', sale),
  delete: (id: number) => api.delete(`/sales/${id}`)
}
