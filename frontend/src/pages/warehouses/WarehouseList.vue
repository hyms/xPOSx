<template>
  <q-page padding>
    <div class="row q-col-gutter-md">
      <div class="col-12">
        <q-table
          title="Almacenes"
          :rows="warehouses"
          :columns="columns"
          row-key="id"
          :loading="loading"
        >
          <template v-slot:top-right>
            <q-btn color="primary" label="Nuevo Almacén" icon="add" @click="openDialog()" />
          </template>

          <template v-slot:body-cell-actions="props">
            <q-td :props="props">
              <q-btn flat round color="primary" icon="edit" @click="openDialog(props.row)" />
              <q-btn flat round color="negative" icon="delete" @click="confirmDeleteAction(props.row)" />
            </q-td>
          </template>
        </q-table>
      </div>
    </div>

    <!-- Warehouse Dialog -->
    <q-dialog v-model="showDialog" persistent>
      <q-card style="min-width: 350px">
        <q-card-section>
          <div class="text-h6">{{ isEdit ? 'Editar Almacén' : 'Nuevo Almacén' }}</div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-form @submit="saveWarehouse" class="q-gutter-md">
            <q-input
              v-model="formData.name"
              label="Nombre del Almacén"
              lazy-rules
              :rules="[ val => val && val.length > 0 || 'Campo requerido']"
            />
            <q-input v-model="formData.city" label="Ciudad" />
            <q-input v-model="formData.mobile" label="Teléfono/Móvil" />
            <q-input v-model="formData.email" label="Email" type="email" />
            <q-input v-model="formData.country" label="País" />

            <div class="row justify-end q-mt-md">
              <q-btn label="Cancelar" color="primary" flat v-close-popup />
              <q-btn :label="isEdit ? 'Actualizar' : 'Guardar'" color="primary" type="submit" :loading="saving" />
            </div>
          </q-form>
        </q-card-section>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { useQuasar } from 'quasar'
import { warehouseService } from '@/services/warehouse.service';
import type { Warehouse } from '@/types'
import { useConfirm } from '@/composables/useConfirm'

const $q = useQuasar()
const { confirmDelete } = useConfirm()
const warehouses = ref<Warehouse[]>([])
const loading = ref(true)
const saving = ref(false)
const showDialog = ref(false)
const isEdit = ref(false)

const formData = reactive<Warehouse>({
  name: '',
  city: '',
  mobile: '',
  email: '',
  country: ''
})

const columns = [
  { name: 'id', label: 'ID', field: 'id', sortable: true, align: 'left' as const },
  { name: 'name', label: 'Nombre', field: 'name', sortable: true, align: 'left' as const },
  { name: 'city', label: 'Ciudad', field: 'city', sortable: true, align: 'left' as const },
  { name: 'mobile', label: 'Teléfono', field: 'mobile', align: 'left' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const fetchWarehouses = async () => {
  loading.value = true
  try {
    const response = await warehouseService.getAll()
    warehouses.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar almacenes' })
  } finally {
    loading.value = false
  }
}

const openDialog = (warehouse?: Warehouse) => {
  if (warehouse) {
    isEdit.value = true
    Object.assign(formData, { ...warehouse })
  } else {
    isEdit.value = false
    Object.assign(formData, { name: '', city: '', mobile: '', email: '', country: '' })
  }
  showDialog.value = true
}

const saveWarehouse = async () => {
  saving.value = true
  try {
    if (isEdit.value) {
      await warehouseService.update(formData.id!, formData)
      $q.notify({ color: 'positive', message: 'Almacén actualizado correctamente' })
    } else {
      await warehouseService.create(formData)
      $q.notify({ color: 'positive', message: 'Almacén creado correctamente' })
    }
    showDialog.value = false
    fetchWarehouses()
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar almacén' })
  } finally {
    saving.value = false
  }
}

const confirmDeleteAction = (warehouse: Warehouse) => {
  confirmDelete(warehouse.name, async () => {
    await warehouseService.delete(warehouse.id!)
    fetchWarehouses()
  })
}

onMounted(fetchWarehouses)
</script>
