import api from '@/api'
import type { Role } from '@/types'

export const roleService = {
  getAll: () => api.get<Role[]>('/roles'),
  getById: (id: number) => api.get<Role>(`/roles/${id}`),
  create: (role: Role) => api.post('/roles', role),
  update: (id: number, role: Role) => api.put(`/roles/${id}`, role),
  delete: (id: number) => api.delete(`/roles/${id}`),
  assignPermissions: (id: number, permissions: number[]) => api.post(`/roles/${id}/permissions`, permissions)
}
