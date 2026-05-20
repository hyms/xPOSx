import { defineStore } from 'pinia'
import type { Warehouse } from '@/types'
import { warehouseService } from '@/services/warehouse.service'
import api from '@/api/index'

interface WarehouseState {
  warehouses: Warehouse[]
  activeWarehouseId: number | null
  loading: boolean
}

export const useWarehouseStore = defineStore('warehouse', {
  state: (): WarehouseState => ({
    warehouses: [],
    activeWarehouseId: localStorage.getItem('active_warehouse_id')
      ? Number(localStorage.getItem('active_warehouse_id'))
      : null,
    loading: false
  }),

  getters: {
    activeWarehouse: (state) => state.warehouses.find((w: Warehouse) => w.id === state.activeWarehouseId)
  },

  actions: {
    async fetchWarehouses() {
      this.loading = true
      try {
        const response = await warehouseService.getAll()
        this.warehouses = response.data
        
        if (!this.activeWarehouseId && this.warehouses.length > 0) {
          const savedId = localStorage.getItem('active_warehouse_id')
          if (savedId) {
            this.activeWarehouseId = Number(savedId)
          } else {
            this.activeWarehouseId = this.warehouses[0].id
            localStorage.setItem('active_warehouse_id', String(this.activeWarehouseId))
          }
        }
      } finally {
        this.loading = false
      }
    },

    async setActiveWarehouse(id: number) {
      this.activeWarehouseId = id;
      localStorage.setItem('active_warehouse_id', String(id));
      try {
        const response = await api.post(`/auth/switch-warehouse/${id}`)
        const { token } = response.data
        if (token) {
          localStorage.setItem('token', token)
        }
      } catch (error) {
        console.error('Error switching warehouse:', error)
      } finally {
        // Force a page reload or router push to clear all local composable states (like POS cart)
        window.location.reload(); 
      }
    }
  }
})
