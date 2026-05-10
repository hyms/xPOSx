import api from '@/api'
import type { CurrencySetting } from '@/types'

export const currencyService = {
  get: () => api.get<CurrencySetting>('/settings/currency'),
  update: (data: CurrencySetting) => api.post('/settings/currency', data)
}
