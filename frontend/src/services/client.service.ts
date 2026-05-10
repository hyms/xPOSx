import api from '@/api'
import type { Client } from '@/types'

export const clientService = {
  getAll: () => api.get<Client[]>('/clients'),
  getById: (id: number) => api.get<Client>(`/clients/${id}`),
  create: (client: Client) => api.post('/clients', client),
  update: (id: number, client: Client) => api.put(`/clients/${id}`, client),
  delete: (id: number) => api.delete(`/clients/${id}`)
}
