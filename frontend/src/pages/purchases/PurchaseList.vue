<template>
  <q-page padding>
    <q-table
      title="Compras"
      :rows="purchases"
      :columns="columns"
      row-key="id"
      v-model:pagination="pagination"
      :loading="loading"
      :filter="filter"
      @request="onRequest"
      binary-state-sort
      :rows-per-page-options="[10, 20, 50, 100]"
    >
      <template v-slot:top-right>
        <q-input
          borderless dense debounce="300"
          v-model="filter"
          placeholder="Buscar"
          class="q-mr-md"
        >
          <template v-slot:append>
            <q-icon name="search" />
          </template>
        </q-input>
        <q-btn
          color="primary"
          icon="add"
          label="Nueva Compra"
          to="/purchases/create"
        />
      </template>

      <template v-slot:body-cell-status="props">
        <q-td :props="props">
          <q-badge :color="getStatusColor(props.value)">
            {{ props.value }}
          </q-badge>
        </q-td>
      </template>

      <template v-slot:body-cell-paymentStatus="props">
        <q-td :props="props">
          <q-badge :color="getPaymentStatusColor(props.value)">
            {{ props.value }}
          </q-badge>
        </q-td>
      </template>

      <template v-slot:body-cell-actions="props">
        <q-td :props="props">
          <q-btn flat round color="primary" icon="visibility" @click="viewPurchase(props.row.id)" />
          <q-btn v-if="props.row.voucherId" flat round color="accent" icon="receipt" @click="printPurchaseVoucher(props.row.id)" />
          <q-btn flat round color="negative" icon="delete" @click="confirmDeleteAction(props.row)" />
        </q-td>
      </template>
      
      <template v-slot:loading>
        <q-inner-loading showing color="primary" />
      </template>
      
      <template v-if="!loading && purchases.length === 0" v-slot:no-data="{ icon, message, filter }">
        <div class="full-width row flex-center text-accent q-gutter-sm">
          <q-icon size="2em" :name="filter ? 'filter_alt_off' : icon" />
          <span>{{ filter ? 'No se encontraron resultados' : message }}</span>
        </div>
      </template>

    </q-table>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { purchaseService } from '@/services/purchase.service'
import type { PurchaseReadDto, PagingParams } from '@/types'
import { useConfirm } from '@/composables/useConfirm'
import { useRouter } from 'vue-router'

const $q = useQuasar()
const router = useRouter()
const { confirmDelete } = useConfirm()

const purchases = ref<PurchaseReadDto[]>([])
const loading = ref(false)
const filter = ref('')
const pagination = ref({
  sortBy: 'date',
  descending: true,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0
})

const columns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => new Date(val).toLocaleDateString(), sortable: true, align: 'left' as const },
  { name: 'ref', label: 'Referencia', field: 'ref', sortable: true, align: 'left' as const },
  { name: 'providerName', label: 'Proveedor', field: 'providerName', sortable: true, align: 'left' as const },
  { name: 'warehouseName', label: 'Almacén', field: 'warehouseName', sortable: true, align: 'left' as const },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', format: (val: number) => `$${val.toFixed(2)}`, sortable: true, align: 'right' as const },
  { name: 'paidAmount', label: 'Pagado', field: 'paidAmount', format: (val: number) => `$${val.toFixed(2)}`, sortable: true, align: 'right' as const },
  { name: 'status', label: 'Estado', field: 'status', align: 'center' as const, sortable: true },
  { name: 'paymentStatus', label: 'Pago', field: 'paymentStatus', align: 'center' as const, sortable: true },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const getStatusColor = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'received': return 'positive'
    case 'pending': return 'warning'
    case 'ordered': return 'info'
    default: return 'primary'
  }
}

const getPaymentStatusColor = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'paid': return 'positive'
    case 'partial': return 'warning'
    case 'unpaid': return 'negative'
    default: return 'grey'
  }
}

const onRequest = async (props: any) => {
  const { page, rowsPerPage, sortBy, descending } = props.pagination
  const filterValue = props.filter

  loading.value = true

  const params: PagingParams = {
    page,
    pageSize: rowsPerPage,
    sortBy,
    sortDescending: descending,
    filter: filterValue
  }

  try {
    const response = await purchaseService.getAll(params)
    purchases.value = response.data.items
    pagination.value.rowsNumber = response.data.totalItems || 0
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar compras' })
  } finally {
    loading.value = false
  }

  pagination.value.page = page
  pagination.value.rowsPerPage = rowsPerPage
  pagination.value.sortBy = sortBy
  pagination.value.descending = descending
}

const viewPurchase = (id: number) => {
  router.push(`/purchases/${id}`)
}

const printPurchaseVoucher = (id: number) => {
  router.push(`/purchases/print/${id}`)
}

const confirmDeleteAction = (purchase: PurchaseReadDto) => {
  confirmDelete(purchase.ref, async () => {
    try {
      await purchaseService.delete(purchase.id)
      $q.notify({ color: 'positive', message: 'Compra eliminada correctamente' })
      onRequest({ pagination: pagination.value, filter: filter.value })
    } catch (error) {
      $q.notify({ color: 'negative', message: 'Error al eliminar la compra' })
    }
  })
}

onMounted(() => {
  onRequest({ pagination: pagination.value, filter: filter.value })
})
</script>
