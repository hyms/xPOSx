export interface PagedResult<T> {
  items: T[]
  totalItems: number
  page: number
  pageSize: number
}

export interface PagingParams {
  page: number
  pageSize: number
  sortBy?: string
  sortDescending?: boolean
  filter?: string
  [key: string]: any
}