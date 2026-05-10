export interface Client {
  id?: number
  name: string
  companyName?: string
  code: number
  email: string
  phone?: string
  country?: string
  city?: string
  address?: string
  nitCi: string
}

export interface Provider {
  id?: number
  name: string
  code: number
  email: string
  phone?: string
  country?: string
  city?: string
  address?: string
}