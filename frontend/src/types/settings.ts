export interface Setting {
  id: number;
  key: string;
  value: string;
}

export interface NotificationSetting {
  key: string;
  label: string;
  category: string;
  value: boolean;
}

export interface CurrencySetting {
  id: number;
  code: string;
  symbol: string;
}

export interface MailSetting {
  id: number;
  host: string;
  port: number;
  username: string;
  password: string;
  encryption: string;
  fromAddress: string;
  fromName: string;
}

export interface SmsSetting {
  id: number;
  sid: string;
  token: string;
  fromNumber: string;
}

export interface PaymentGatewaySetting {
  id: number;
  stripeKey: string;
  stripeSecret: string;
  paypalClientId: string;
  paypalClientSecret: string;
}

export interface Notification {
  id: number;
  message: string;
  createdAt: string;
  read: boolean;
}
