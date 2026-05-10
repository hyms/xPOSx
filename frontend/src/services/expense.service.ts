import api from '@/api'
import type { Expense, ExpenseCategory, ExpenseReadDto } from '@/types'

export const expenseService = {
  // Expenses
  getAll: () => api.get<ExpenseReadDto[]>('/expenses'),
  getById: (id: number) => api.get<Expense>(`/expenses/${id}`),
  create: (expense: Expense) => api.post('/expenses', expense),
  update: (id: number, expense: Expense) => api.put(`/expenses/${id}`, expense),
  delete: (id: number) => api.delete(`/expenses/${id}`),

  // Categories
  getCategories: () => api.get<ExpenseCategory[]>('/expense-categories'),
  getCategoryById: (id: number) => api.get<ExpenseCategory>(`/expense-categories/${id}`),
  createCategory: (category: ExpenseCategory) => api.post('/expense-categories', category),
  updateCategory: (id: number, category: ExpenseCategory) => api.put(`/expense-categories/${id}`, category),
  deleteCategory: (id: number) => api.delete(`/expense-categories/${id}`)
}
