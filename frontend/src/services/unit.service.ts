import api from '@/api'
import type { Unit } from '@/types'

export const unitService = {
  getAll: () => api.get<Unit[]>('/units'),
  getById: (id: number) => api.get<Unit>(`/units/${id}`),
  create: (unit: Unit) => api.post('/units', unit),
  update: (id: number, unit: Unit) => api.put(`/units/${id}`, unit),
  delete: (id: number) => api.delete(`/units/${id}`)
}
