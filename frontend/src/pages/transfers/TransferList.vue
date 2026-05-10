<template>
  <q-page padding>
    <div class="row q-col-gutter-md">
      <div class="col-12">
        <q-table
          title="Transferencias"
          :rows="transfers"
          :columns="columns"
          row-key="id"
          :loading="loading"
          :filter="filter"
        >
          <template v-slot:top-right>
            <q-input dense debounce="300" color="primary" v-model="filter" placeholder="Buscar">
              <template v-slot:append>
                <q-icon name="search" />
              </template>
            </q-input>
            <q-btn color="primary" label="Nueva Transferencia" icon="add" to="/transfers/create" class="q-ml-md" />
          </template>

          <template v-slot:body-cell-status="props">
            <q-td :props="props">
              <q-badge :color="getStatusColor(props.value)">
                {{ props.value }}
              </q-badge>
            </q-td>
          </template>

          <template v-slot:body-cell-actions="props">
            <q-td :props="props">
              <q-btn flat round color="primary" icon="visibility" @click="viewTransfer(props.row.id)" />
              <q-btn flat round color="negative" icon="delete" @click="confirmDeleteAction(props.row)" />
            </q-td>
          </template>
        </q-table>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useQuasar } from 'quasar'
import { transferService } from '@/services/transfer.service';
import type { TransferReadDto } from '@/types'
import { useConfirm } from '@/composables/useConfirm'

const $q = useQuasar()
const { confirmDelete } = useConfirm()
const transfers = ref<TransferReadDto[]>([])
const loading = ref(true)
const filter = ref('')

const columns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => new Date(val).toLocaleDateString(), sortable: true, align: 'left' as const },
  { name: 'ref', label: 'Referencia', field: 'ref', sortable: true, align: 'left' as const },
  { name: 'fromWarehouse', label: 'Desde', field: 'fromWarehouseName', align: 'left' as const },
  { name: 'toWarehouse', label: 'Hacia', field: 'toWarehouseName', align: 'left' as const },
  { name: 'items', label: 'Items', field: 'items', align: 'center' as const },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', format: (val: number) => `$${val.toFixed(2)}`, align: 'right' as const },
  { name: 'status', label: 'Estado', field: 'status', align: 'center' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'completed': return 'positive'
    case 'sent': return 'warning'
    case 'pending': return 'grey'
    default: return 'primary'
  }
}

const fetchTransfers = async (filter?: string) => {
  loading.value = true
  try {
    const response = await transferService.getAll(filter)
    transfers.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar transferencias' })
  } finally {
    loading.value = false
  }
}

watch(filter, (newFilter) => {
  fetchTransfers(newFilter)
})

const viewTransfer = (_id: number) => {
  // Logic to view details, maybe a dialog
  $q.notify({ message: 'Detalle no implementado aún', color: 'info' })
}

const confirmDeleteAction = (transfer: TransferReadDto) => {
  confirmDelete(transfer.ref, async () => {
    await transferService.delete(transfer.id)
    fetchTransfers()
  })
}

onMounted(fetchTransfers)
</script>
