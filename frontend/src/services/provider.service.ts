import api from '@/api'
import type { Provider } from '@/types'

export const providerService = {
  getAll: () => api.get<Provider[]>('/providers'),
  getById: (id: number) => api.get<Provider>(`/providers/${id}`),
  create: (provider: Provider) => api.post('/providers', provider),
  update: (id: number, provider: Provider) => api.put(`/providers/${id}`, provider),
  delete: (id: number) => api.delete(`/providers/${id}`)
}
