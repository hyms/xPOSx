import api from '@/api'
import type { Setting } from '@/types'

export const settingService = {
  get: () => api.get<Setting>('/settings'),
  update: (setting: Setting) => api.put('/settings', setting),
}
