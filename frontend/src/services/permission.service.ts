import api from '@/api'
import type { Permission } from '@/types'

export const permissionService = {
  getAll: () => api.get<Permission[]>('/permissions'),
}
