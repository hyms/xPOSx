<template>
  <q-page padding>
    <q-card>
      <q-tabs
        v-model="tab"
        dense
        class="text-grey"
        active-color="primary"
        indicator-color="primary"
        align="justify"
        narrow-indicator
      >
        <q-tab name="sale_returns" label="Devoluciones Ventas" icon="assignment_return" />
        <q-tab name="purchase_returns" label="Devoluciones Compras" icon="keyboard_return" />
      </q-tabs>

      <q-separator />

      <q-tab-panels v-model="tab" animated>
        <!-- Sale Returns Tab -->
        <q-tab-panel name="sale_returns">
          <q-table
            title="Devoluciones de Ventas"
            :rows="saleReturns"
            :columns="saleColumns"
            row-key="id"
            :loading="loadingSales"
          >
            <template v-slot:top-right>
              <q-btn color="primary" label="Nueva Devolución" icon="add" to="/returns/sales/create" />
            </template>

            <template v-slot:body-cell-grandTotal="props">
              <q-td :props="props">
                {{ formatCurrency(props.row.grandTotal) }}
              </q-td>
            </template>

            <template v-slot:body-cell-status="props">
              <q-td :props="props">
                <q-chip :color="getStatusColor(props.row.status)" text-color="white" dense>
                  {{ props.row.status }}
                </q-chip>
              </q-td>
            </template>

            <template v-slot:body-cell-actions="props">
              <q-td :props="props">
                <q-btn flat round color="primary" icon="visibility" @click="viewSaleReturn(props.row.id)" />
                <q-btn flat round color="accent" icon="receipt" @click="printSaleReturnVoucher(props.row.id)" />
                <q-btn flat round color="negative" icon="delete" @click="confirmDeleteSaleReturn(props.row)" />
              </q-td>
            </template>
          </q-table>
        </q-tab-panel>

        <!-- Purchase Returns Tab -->
        <q-tab-panel name="purchase_returns">
          <q-table
            title="Devoluciones de Compras"
            :rows="purchaseReturns"
            :columns="purchaseColumns"
            row-key="id"
            :loading="loadingPurchases"
          >
            <template v-slot:top-right>
              <q-btn color="primary" label="Nueva Devolución" icon="add" to="/returns/purchases/create" />
            </template>

            <template v-slot:body-cell-grandTotal="props">
              <q-td :props="props">
                {{ formatCurrency(props.row.grandTotal) }}
              </q-td>
            </template>

            <template v-slot:body-cell-status="props">
              <q-td :props="props">
                <q-chip :color="getStatusColor(props.row.status)" text-color="white" dense>
                  {{ props.row.status }}
                </q-chip>
              </q-td>
            </template>

            <template v-slot:body-cell-actions="props">
              <q-td :props="props">
                <q-btn flat round color="primary" icon="visibility" @click="viewPurchaseReturn(props.row.id)" />
                <q-btn flat round color="accent" icon="receipt" @click="printPurchaseReturnVoucher(props.row.id)" />
                <q-btn flat round color="negative" icon="delete" @click="confirmDeletePurchaseReturn(props.row)" />
              </q-td>
            </template>
          </q-table>
        </q-tab-panel>
      </q-tab-panels>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { returnService } from '@/services/return.service'
import { useConfirm } from '@/composables/useConfirm'
import type { SaleReturnReadDto, PurchaseReturnReadDto } from '@/types'
import { useRouter } from 'vue-router'

import { useCurrency } from '@/composables/useCurrency';

const $q = useQuasar()
const { confirmDelete } = useConfirm()
const router = useRouter()
const tab = ref('sale_returns')
const { formatCurrency } = useCurrency();

const loadingSales = ref(false)
const loadingPurchases = ref(false)

const saleReturns = ref<SaleReturnReadDto[]>([])
const purchaseReturns = ref<PurchaseReturnReadDto[]>([])

const saleColumns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => val.split('T')[0], sortable: true, align: 'left' as const },
  { name: 'ref', label: 'Referencia', field: 'ref', sortable: true, align: 'left' as const },
  { name: 'clientName', label: 'Cliente', field: 'clientName', sortable: true, align: 'left' as const },
  { name: 'warehouseName', label: 'Almacén', field: 'warehouseName', sortable: true, align: 'left' as const },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', sortable: true, align: 'right' as const },
  { name: 'voucherId', label: 'Voucher ID', field: 'voucherId', sortable: true, align: 'left' as const, format: (val: any) => val || 'N/A'  },
  { name: 'status', label: 'Estado', field: 'status', align: 'center' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const purchaseColumns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => val.split('T')[0], sortable: true, align: 'left' as const },
  { name: 'ref', label: 'Referencia', field: 'ref', sortable: true, align: 'left' as const },
  { name: 'providerName', label: 'Proveedor', field: 'providerName', sortable: true, align: 'left' as const },
  { name: 'warehouseName', label: 'Almacén', field: 'warehouseName', sortable: true, align: 'left' as const },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', sortable: true, align: 'right' as const },
  { name: 'voucherId', label: 'Voucher ID', field: 'voucherId', sortable: true, align: 'left' as const, format: (val: any) => val || 'N/A'  },
  { name: 'status', label: 'Estado', field: 'status', align: 'center' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const fetchSaleReturns = async () => {
  loadingSales.value = true
  try {
    const response = await returnService.getAllSaleReturns()
    saleReturns.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar devoluciones de ventas' })
  } finally {
    loadingSales.value = false
  }
}

const fetchPurchaseReturns = async () => {
  loadingPurchases.value = true
  try {
    const response = await returnService.getAllPurchaseReturns()
    purchaseReturns.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar devoluciones de compras' })
  } finally {
    loadingPurchases.value = false
  }
}

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'completed': return 'positive'
    case 'pending': return 'warning'
    default: return 'grey'
  }
}

const viewSaleReturn = (id: number) => {
  router.push(`/returns/sales/${id}`)
}

const printSaleReturnVoucher = (id: number) => {
  router.push(`/returns/sales/print/${id}`)
}

const viewPurchaseReturn = (id: number) => {
  router.push(`/returns/purchases/${id}`)
}

const printPurchaseReturnVoucher = (id: number) => {
  router.push(`/returns/purchases/print/${id}`)
}

const confirmDeleteSaleReturn = (item: SaleReturnReadDto) => {
  confirmDelete(item.ref, async () => {
    await returnService.deleteSaleReturn(item.id)
    fetchSaleReturns()
  })
}

const confirmDeletePurchaseReturn = (item: PurchaseReturnReadDto) => {
  confirmDelete(item.ref, async () => {
    await returnService.deletePurchaseReturn(item.id)
    fetchPurchaseReturns()
  })
}

onMounted(() => {
  fetchSaleReturns()
  fetchPurchaseReturns()
})
</script>
