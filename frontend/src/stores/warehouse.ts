import { defineStore } from 'pinia'
import type { Warehouse } from '@/types'
import { warehouseService } from '@/services/warehouse.service'
import api from '@/api/index'

interface WarehouseState {
  warehouses: Warehouse[]
  activeWarehouseId: number | null
  loading: boolean
}

function getActiveWarehouseIdFromToken(): number | null {
  const token = localStorage.getItem('token');
  if (!token) return null;
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    const claims = JSON.parse(jsonPayload);
    return claims.active_warehouse_id ? Number(claims.active_warehouse_id) : null;
  } catch (e) {
    return null;
  }
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
        
        let savedId = localStorage.getItem('active_warehouse_id')
          ? Number(localStorage.getItem('active_warehouse_id'))
          : null;

        // If no saved ID or it's not in the list of available warehouses, fallback to first warehouse
        if (!savedId || !this.warehouses.some((w: Warehouse) => w.id === savedId)) {
          if (this.warehouses.length > 0) {
            savedId = this.warehouses[0].id;
          }
        }

        const tokenWarehouseId = getActiveWarehouseIdFromToken();

        if (savedId !== null) {
          this.activeWarehouseId = savedId;
          localStorage.setItem('active_warehouse_id', String(savedId));

          // If the token claim is different from the active warehouse ID, switch it to sync the backend!
          if (savedId !== tokenWarehouseId) {
            await this.setActiveWarehouse(savedId);
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
