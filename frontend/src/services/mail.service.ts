import api from '@/api'
import type { MailSetting } from '@/types'

export const mailService = {
  get: () => api.get<MailSetting>('/settings/mail'),
  update: (data: MailSetting) => api.post('/settings/mail', data)
}
