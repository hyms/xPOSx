export interface Setting {
  id?: number
  email: string
  currency: string
  companyName: string
  companyPhone: string
  companyAddress: string
  logo?: string
  footer?: string
  version: string
  days: number
}

export interface Notification {
  id: number
  userId?: number
  type: string
  title: string
  message: string
  isRead: boolean
  createdAt: string
}

export interface NotificationSetting {
  id: number
  key: string
  label: string
  value: boolean
  category: string
}

export interface CurrencySetting {
  id?: number
  code: string
  symbol: string
}

export interface MailSetting {
  id?: number
  host: string
  port: number
  username: string
  password?: string
  encryption: string
  fromAddress: string
  fromName: string
}

export interface SmsSetting {
  id?: number
  sid: string
  token?: string
  fromNumber: string
}

export interface PaymentGatewaySetting {
  id?: number
  stripeKey?: string
  stripeSecret?: string
  paypalClientId?: string
  paypalClientSecret?: string
}