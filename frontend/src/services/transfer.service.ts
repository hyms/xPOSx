import api from '@/api'
import type { Transfer, TransferReadDto } from '@/types'

export const transferService = {
  getAll: (filter?: string) => {
    return api.get<TransferReadDto[]>('/transfers', { params: { filter } })
  },
  getById: (id: number) => api.get<Transfer>(`/transfers/${id}`),
  create: (transfer: Transfer) => api.post('/transfers', transfer),
  delete: (id: number) => api.delete(`/transfers/${id}`)
}
