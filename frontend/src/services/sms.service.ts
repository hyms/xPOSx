import api from '@/api'
import type { SmsSetting } from '@/types'

export const smsService = {
  get: () => api.get<SmsSetting>('/settings/sms'),
  update: (data: SmsSetting) => api.post('/settings/sms', data)
}
