import api from '@/api'
import type { Category } from '@/types'

export const categoryService = {
  getAll: () => api.get<Category[]>('/categories'),
  getById: (id: number) => api.get<Category>(`/categories/${id}`),
  create: (category: Category) => api.post('/categories', category),
  update: (id: number, category: Category) => api.put(`/categories/${id}`, category),
  delete: (id: number) => api.delete(`/categories/${id}`)
}
