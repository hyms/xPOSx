import api from '@/api'
import type { Warehouse } from '@/types'

export const warehouseService = {
  getAll: () => api.get<Warehouse[]>('/warehouses'),
  getById: (id: number) => api.get<Warehouse>(`/warehouses/${id}`),
  create: (warehouse: Warehouse) => api.post('/warehouses', warehouse),
  update: (id: number, warehouse: Warehouse) => api.put(`/warehouses/${id}`, warehouse),
  delete: (id: number) => api.delete(`/warehouses/${id}`)
}
