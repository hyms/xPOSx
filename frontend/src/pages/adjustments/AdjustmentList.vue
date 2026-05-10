<template>
  <q-page padding>
    <div class="row q-col-gutter-md">
      <div class="col-12">
        <q-table
          title="Ajustes de Inventario"
          :rows="adjustments"
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
            <q-btn color="primary" label="Nuevo Ajuste" icon="add" to="/adjustments/create" class="q-ml-md" />
          </template>

          <template v-slot:body-cell-actions="props">
            <q-td :props="props">
              <q-btn flat round color="primary" icon="visibility" @click="viewAdjustment(props.row.id)" />
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
import { adjustmentService } from '@/services/adjustment.service';
import type { AdjustmentReadDto } from '@/types'
import { useConfirm } from '@/composables/useConfirm'

const $q = useQuasar()
const { confirmDelete } = useConfirm()
const adjustments = ref<AdjustmentReadDto[]>([])
const loading = ref(true)
const filter = ref('')

const columns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => new Date(val).toLocaleDateString(), sortable: true, align: 'left' as const },
  { name: 'ref', label: 'Referencia', field: 'ref', sortable: true, align: 'left' as const },
  { name: 'warehouse', label: 'Almacén', field: 'warehouseName', align: 'left' as const },
  { name: 'items', label: 'Items', field: 'items', align: 'center' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const fetchAdjustments = async (filter?: string) => {
  loading.value = true
  try {
    const response = await adjustmentService.getAll(filter)
    adjustments.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar ajustes' })
  } finally {
    loading.value = false
  }
}

watch(filter, (newFilter) => {
  fetchAdjustments(newFilter)
})

const viewAdjustment = (_id: number) => {
  $q.notify({ message: 'Detalle no implementado aún', color: 'info' })
}

const confirmDeleteAction = (adjustment: AdjustmentReadDto) => {
  confirmDelete(adjustment.ref, async () => {
    await adjustmentService.delete(adjustment.id)
    fetchAdjustments()
  })
}

onMounted(fetchAdjustments)
</script>
