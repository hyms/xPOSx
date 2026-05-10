import api from '@/api'
import type { Quotation, QuotationReadDto } from '@/types'

export const quotationService = {
  getAll: () => api.get<QuotationReadDto[]>('/quotations'),
  getById: (id: number) => api.get<Quotation>(`/quotations/${id}`),
  create: (data: Quotation) => api.post('/quotations', data),
  update: (id: number, data: Quotation) => api.put(`/quotations/${id}`, data),
  delete: (id: number) => api.delete(`/quotations/${id}`)
}
