import api from '@/api'
import type { User } from '@/types'

export const userService = {
  getAll: () => api.get<User[]>('/users'),
  getById: (id: number) => api.get<User>(`/users/${id}`),
  create: (user: User) => api.post('/users', user),
  update: (id: number, user: User) => api.put(`/users/${id}`, user),
  delete: (id: number) => api.delete(`/users/${id}`),
  toggleStatus: (id: number) => api.put(`/users/${id}/toggle-status`),
  getProfile: () => api.get<User>('/users/profile'),
  updateProfile: (user: User) => api.put('/users/profile', user),
  updatePassword: (newPassword: string) => api.put('/users/profile/password', { newPassword })
}
