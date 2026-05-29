import api from '@/api'
import type { Product } from '@/types'

export const productService = {
  getAll: () => api.get<Product[]>('/products'),
  getById: (id: number) => api.get<Product>(`/products/${id}`),
  create: (product: Product) => api.post('/products', product),
  update: (id: number, product: Product) => api.put(`/products/${id}`, product),
  delete: (id: number) => api.delete(`/products/${id}`),
  getPublic: () => api.get<Product[]>('/products/public'),
  getPublicTop: (limit = 5) => api.get<Product[]>(`/products/public/top?limit=${limit}`)
}
