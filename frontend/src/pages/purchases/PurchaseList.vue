<template>
  <q-page padding>
    <div class="row q-col-gutter-sm">
      <div class="col-12">
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
        <div class="row q-gutter-sm items-center full-width-xs">
          <q-input
            dense debounce="300"
            v-model="filter"
            placeholder="Buscar"
            class="col-grow"
          >
            <template v-slot:append>
              <q-icon name="search" />
            </template>
          </q-input>
          <q-btn
            color="primary"
            icon="add"
            label="Nueva"
            to="/purchases/create"
            class="full-width-xs mobile-only-mt"
          />
        </div>
      </template>

      <template v-slot:item="props">
        <div class="q-pa-xs col-xs-12 col-sm-6 col-md-4">
          <q-card flat bordered>
            <q-card-section>
              <div class="row items-center justify-between">
                <div class="text-subtitle2 text-weight-bold">{{ props.row.ref }}</div>
                <div class="text-caption text-grey">{{ new Date(props.row.date).toLocaleDateString() }}</div>
              </div>
              <div class="q-mt-sm text-caption text-grey">Proveedor: {{ props.row.providerName }}</div>
              <div class="row q-mt-sm items-center justify-between">
                <div>
                  <q-chip :color="getStatusColor(props.row.status)" text-color="white" dense size="sm">
                    {{ props.row.status }}
                  </q-chip>
                  <q-chip :color="getPaymentStatusColor(props.row.paymentStatus)" text-color="white" dense size="sm">
                    {{ props.row.paymentStatus }}
                  </q-chip>
                </div>
                <div class="text-subtitle1 text-weight-bolder text-primary">
                  {{ formatCurrency(props.row.grandTotal) }}
                </div>
              </div>
            </q-card-section>
            <q-separator />
            <q-card-actions align="right">
              <q-btn flat round color="primary" icon="visibility" size="sm" @click="viewPurchase(props.row.id)" />
              <q-btn flat round color="accent" icon="receipt" size="sm" @click="printPurchaseVoucher(props.row.id)" />
              <q-btn flat round color="negative" icon="delete" size="sm" @click="confirmDeleteAction(props.row)" />
            </q-card-actions>
          </q-card>
        </div>
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
          <q-btn flat round color="accent" icon="receipt" @click="printPurchaseVoucher(props.row.id)" />
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
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { purchaseService } from '@/services/purchase.service'
import type { PurchaseReadDto, PagingParams } from '@/types'
import { useConfirm } from '@/composables/useConfirm'
import { useRouter } from 'vue-router'
import { useCurrency } from '@/composables/useCurrency'

const $q = useQuasar()
const router = useRouter()
const { formatCurrency } = useCurrency()
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
