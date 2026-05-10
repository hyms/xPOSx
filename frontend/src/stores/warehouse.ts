import { defineStore } from 'pinia'
import type { Warehouse } from '@/types'
import { warehouseService } from '@/services/warehouse.service'

interface WarehouseState {
  warehouses: Warehouse[]
  activeWarehouseId: number | null
  loading: boolean
}

export const useWarehouseStore = defineStore('warehouse', {
  state: (): WarehouseState => ({
    warehouses: [],
    activeWarehouseId: localStorage.getItem('activeWarehouseId') 
      ? parseInt(localStorage.getItem('activeWarehouseId')!) 
      : null,
    loading: false
  }),

  getters: {
    activeWarehouse: (state) => state.warehouses.find(w => w.id === state.activeWarehouseId)
  },

  actions: {
    async fetchWarehouses() {
      this.loading = true
      try {
        const response = await warehouseService.getAll()
        this.warehouses = response.data
        if (!this.activeWarehouseId && this.warehouses.length > 0) {
          this.setActiveWarehouse(this.warehouses[0].id!)
        }
      } finally {
        this.loading = false
      }
    },

    setActiveWarehouse(id: number) {
      this.activeWarehouseId = id
      localStorage.setItem('activeWarehouseId', id.toString())
    }
  }
})
