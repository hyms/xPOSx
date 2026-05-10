import { defineStore } from 'pinia'
import api from '@/api/index'

interface AuthState {
  token: string | null
  username: string | null
  permissions: string[]
  loading: boolean
}

export const useAuthStore = defineStore('auth', {
  state: (): AuthState => ({
    token: localStorage.getItem('token'),
    username: localStorage.getItem('username'),
    permissions: JSON.parse(localStorage.getItem('permissions') || '[]'),
    loading: false
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    hasPermission: (state) => (permission: string) => state.permissions.includes(permission)
  },

  actions: {
    async login(credentials: any) {
      this.loading = true
      try {
        const response = await api.post('/auth/login', credentials)
        const { token, username, permissions } = response.data
        
        this.token = token
        this.username = username
        this.permissions = permissions
        
        localStorage.setItem('token', token)
        localStorage.setItem('username', username)
        localStorage.setItem('permissions', JSON.stringify(permissions))
        
        return true
      } catch (error) {
        console.error('Login error:', error)
        throw error
      } finally {
        this.loading = false
      }
    },

    logout() {
      this.token = null
      this.username = null
      this.permissions = []
      localStorage.removeItem('token')
      localStorage.removeItem('username')
      localStorage.removeItem('permissions')
    }
  }
})
