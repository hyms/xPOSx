export interface User {
  id?: number
  username: string
  email: string
  password?: string
  firstName?: string
  lastName?: string
  role: number
  roleDetails?: Role
  isActive?: boolean
  defaultWarehouseId?: number | null
  warehouseIds?: number[]
  allWarehousesAccess?: boolean
}

export interface Role {
  id?: number
  name: string
  description?: string
  permissions?: number[]
}

export interface Permission {
  id: number
  name: string
  description: string
}