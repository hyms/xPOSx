import { defineStore } from 'pinia'
import { cashShiftService, type CashShift, type CashRegister } from '@/services/cashShift.service'
import { useWarehouseStore } from '@/stores/warehouse'
import type { ActiveShiftDto } from '@/services/cashShift.service'

interface CashShiftState {
  activeShift: ActiveShiftDto | null
  registers: CashRegister[]
  loading: boolean
}

export const useCashShiftStore = defineStore('cashShift', {
  state: (): CashShiftState => ({
    activeShift: null,
    registers: [],
    loading: false
  }),

  actions: {
    async fetchActiveShift() {
      this.loading = true
      try {
        const response = await cashShiftService.getActiveShift()
        this.activeShift = response.data
      } catch (error) {
        console.error('Error fetching active shift:', error)
      } finally {
        this.loading = false
      }
    },

    async fetchRegisters() {
      const warehouseStore = useWarehouseStore()
      if (!warehouseStore.activeWarehouseId) return

      this.loading = true
      try {
        const response = await cashShiftService.getRegistersByWarehouse(warehouseStore.activeWarehouseId)
        this.registers = response.data
      } catch (error) {
        console.error('Error fetching registers:', error)
      } finally {
        this.loading = false
      }
    },

    async openShift(registerId: number, startingCash: number) {
      this.loading = true
      try {
        await cashShiftService.openShift({ registerId, startingCash })
        await this.fetchActiveShift()
        return true
      } catch (error) {
        console.error('Error opening shift:', error)
        throw error
      } finally {
        this.loading = false
      }
    },

    async closeShift(shiftId: number, actualCash: number, notes?: string) {
      this.loading = true
      try {
        await cashShiftService.closeShift({ shiftId, actualCash, notes })
        this.activeShift = null
        return true
      } catch (error) {
        console.error('Error closing shift:', error)
        throw error
      } finally {
        this.loading = false
      }
    }
  }
})
