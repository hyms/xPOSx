import api from '@/api'
import type { Adjustment, AdjustmentReadDto } from '@/types'

export const adjustmentService = {
  getAll: (filter?: string) => {
    return api.get<AdjustmentReadDto[]>('/adjustments', { params: { filter } })
  },
  getById: (id: number) => api.get<Adjustment>(`/adjustments/${id}`),
  create: (adjustment: Adjustment) => api.post('/adjustments', adjustment),
  delete: (id: number) => api.delete(`/adjustments/${id}`)
}
