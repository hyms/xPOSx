<template>
  <q-page padding>
    <div class="row q-col-gutter-md">
      <div class="col-12">
        <q-table
          v-if="loading && sales.length === 0"
          class="q-mt-md"
          :columns="columns"
          row-key="id"
          :rows="Array(pagination.rowsPerPage).fill({})"
        >
          <template v-slot:header="props">
            <q-tr :props="props">
              <q-th v-for="col in props.cols" :key="col.name" :props="props">
                <q-skeleton type="text" width="50px" />
              </q-th>
            </q-tr>
          </template>
          <template v-slot:body="props">
            <q-tr :props="props">
              <q-td v-for="col in props.cols" :key="col.name" :props="props">
                <q-skeleton type="text" />
              </q-td>
            </q-tr>
          </template>
        </q-table>

        <q-table
          v-else
          title="Ventas"
          :rows="sales"
          :columns="columns"
          row-key="id"
          :loading="loading"
          :filter="filter"
          v-model:pagination="pagination"
          @request="onRequest"
        >
          <template v-slot:loading>
            <q-inner-loading showing color="primary" />
          </template>

          <template v-if="!loading && sales.length === 0 && filter === ''" v-slot:no-data="{ icon, message }">
            <div class="full-width row flex-center text-primary q-gutter-sm">
              <q-icon size="2em" :name="icon" />
              <span>
                {{ message }}
              </span>
              <q-btn color="primary" label="Nueva Venta" icon="add" to="/sales/create" />
            </div>
          </template>

          <template v-slot:top-right>
            <q-input dense debounce="300" color="primary" v-model="filter" placeholder="Buscar">
              <template v-slot:append>
                <q-icon name="search" />
              </template>
            </q-input>
            <q-btn color="primary" label="Ir al POS" icon="point_of_sale" to="/pos" class="q-mr-sm q-ml-md" />
            <q-btn color="secondary" label="Nueva Venta" icon="add" to="/sales/create" />
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
              <q-btn flat round color="primary" icon="visibility" @click="viewSale(props.row.id)" />
              <q-btn flat round color="accent" icon="receipt" @click="printVoucher(props.row.id)" />
              <q-btn flat round color="negative" icon="delete" @click="confirmDeleteAction(props.row)" />
            </q-td>
          </template>
        </q-table>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { saleService } from '@/services/sale.service'
import type { SaleReadDto, PagedResult, PagingParams } from '@/types'
import { useConfirm } from '@/composables/useConfirm'

const $q = useQuasar()
const router = useRouter()
const { confirmDelete } = useConfirm()
const sales = ref<SaleReadDto[]>([])
const loading = ref(false)
const filter = ref('')
const pagination = ref({
  sortBy: 'date',
  descending: true,
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 10
})

const columns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => new Date(val).toLocaleDateString(), sortable: true, align: 'left' as const },
  { name: 'ref', label: 'Referencia', field: 'ref', sortable: true, align: 'left' as const },
  { name: 'client', label: 'Cliente', field: 'clientName', align: 'left' as const, sortable: true },
  { name: 'warehouse', label: 'Almacén', field: 'warehouseName', align: 'left' as const, sortable: true },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', format: (val: number) => `$${val.toFixed(2)}`, align: 'right' as const, sortable: true },
  { name: 'paidAmount', label: 'Pagado', field: 'paidAmount', format: (val: number) => `$${val.toFixed(2)}`, align: 'right' as const, sortable: true },
  { name: 'status', label: 'Estado', field: 'status', align: 'center' as const, sortable: true },
  { name: 'paymentStatus', label: 'Pago', field: 'paymentStatus', align: 'center' as const, sortable: true },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const, sortable: false }
]

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'completed': return 'positive'
    case 'pending': return 'warning'
    case 'ordered': return 'info'
    default: return 'primary'
  }
}

const getPaymentStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
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
    const response: { data: PagedResult<SaleReadDto> } = await saleService.getAll(params)
    sales.value = response.data.items
    pagination.value.rowsNumber = response.data.totalItems || 0
    pagination.value.page = page
    pagination.value.rowsPerPage = rowsPerPage
    pagination.value.sortBy = sortBy
    pagination.value.descending = descending
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar ventas' })
  } finally {
    loading.value = false
  }
}

const viewSale = (_id: number) => {
  $q.notify({ message: 'Detalle no implementado aún', color: 'info' })
}

const printVoucher = (id: number) => {
  router.push(`/sales/print/${id}`)
}

const confirmDeleteAction = (sale: SaleReadDto) => {
  confirmDelete(sale.ref, async () => {
    await saleService.delete(sale.id)
    onRequest({ pagination: pagination.value, filter: filter.value })
  })
}

onMounted(() => {
  onRequest({ pagination: pagination.value, filter: filter.value })
})
</script>
