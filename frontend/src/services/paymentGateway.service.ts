import api from '@/api'
import type { PaymentGatewaySetting } from '@/types'

export const paymentGatewayService = {
  get: () => api.get<PaymentGatewaySetting>('/settings/payment-gateway'),
  update: (data: PaymentGatewaySetting) => api.post('/settings/payment-gateway', data)
}
