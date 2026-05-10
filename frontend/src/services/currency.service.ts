import api from '@/api'
import type { CurrencySetting } from '@/types'

export const currencyService = {
  get: () => api.get<CurrencySetting>('/currencies'),
  update: (data: CurrencySetting) => api.post('/currencies', data)
}
