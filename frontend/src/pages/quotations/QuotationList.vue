<template>
  <q-page padding>
    <q-table
      title="Cotizaciones"
      :rows="quotations"
      :columns="columns"
      row-key="id"
      :loading="loading"
    >
      <template v-slot:top-right>
        <q-btn color="primary" label="Nueva Cotización" icon="add" to="/quotations/create" />
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
          <q-btn flat round color="primary" icon="visibility" @click="viewQuotation(props.row.id)" />
          <q-btn flat round color="negative" icon="delete" @click="confirmDeleteQuotation(props.row)" />
        </q-td>
      </template>
    </q-table>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { quotationService } from '@/services/quotation.service'
import { useConfirm } from '@/composables/useConfirm'
import type { QuotationReadDto } from '@/types'

import { useCurrency } from '@/composables/useCurrency';

const $q = useQuasar()
const { confirmDelete } = useConfirm()
const loading = ref(false)
const quotations = ref<QuotationReadDto[]>([])
const { formatCurrency } = useCurrency();


const columns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => val.split('T')[0], sortable: true, align: 'left' as const },
  { name: 'ref', label: 'Referencia', field: 'ref', sortable: true, align: 'left' as const },
  { name: 'clientName', label: 'Cliente', field: 'clientName', sortable: true, align: 'left' as const },
  { name: 'warehouseName', label: 'Almacén', field: 'warehouseName', sortable: true, align: 'left' as const },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', sortable: true, align: 'right' as const },
  { name: 'status', label: 'Estado', field: 'status', align: 'center' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const fetchQuotations = async () => {
  loading.value = true
  try {
    const response = await quotationService.getAll()
    quotations.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar cotizaciones' })
  } finally {
    loading.value = false
  }
}

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'sent': return 'positive'
    case 'pending': return 'warning'
    default: return 'grey'
  }
}

const viewQuotation = (id: number) => {
  $q.notify({ message: 'Detalle de cotización ' + id })
}

const confirmDeleteQuotation = (item: QuotationReadDto) => {
  confirmDelete(item.ref, async () => {
    await quotationService.delete(item.id)
    fetchQuotations()
  })
}

onMounted(fetchQuotations)
</script>
