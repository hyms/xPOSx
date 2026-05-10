<template>
  <q-page padding>
    <div class="row q-col-gutter-md">
      <div class="col-12">
        <q-table
          title="Clientes"
          :rows="clients"
          :columns="columns"
          row-key="id"
          :loading="loading"
        >
          <template v-slot:top-right>
            <q-btn color="primary" label="Nuevo Cliente" icon="add" @click="openDialog()" />
          </template>

          <template v-slot:body-cell-actions="props">
            <q-td :props="props">
              <q-btn flat round color="primary" icon="edit" @click="openDialog(props.row)" />
              <q-btn flat round color="negative" icon="delete" @click="confirmDelete(props.row)" />
            </q-td>
          </template>
        </q-table>
      </div>
    </div>

    <!-- Client Dialog -->
    <q-dialog v-model="showDialog" persistent>
      <q-card style="min-width: 450px">
        <q-card-section>
          <div class="text-h6">{{ isEdit ? 'Editar Cliente' : 'Nuevo Cliente' }}</div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-form @submit="saveClient" class="q-gutter-md">
            <div class="row q-col-gutter-sm">
              <div class="col-12 col-md-6">
                <q-input v-model="formData.name" label="Nombre" lazy-rules :rules="[ val => !!val || 'Requerido']" />
              </div>
              <div class="col-12 col-md-6">
                <q-input v-model="formData.nitCi" label="NIT/CI" lazy-rules :rules="[ val => !!val || 'Requerido']" />
              </div>
              <div class="col-12 col-md-6">
                <q-input v-model="formData.phone" label="Teléfono" lazy-rules :rules="[ val => !!val || 'Requerido']" />
              </div>
              <div class="col-12 col-md-6">
                <q-input v-model="formData.email" label="Email" type="email" />
              </div>
              <div class="col-12">
                <q-input v-model="formData.companyName" label="Empresa" />
              </div>
              <div class="col-12 col-md-6">
                <q-input v-model="formData.city" label="Ciudad" />
              </div>
              <div class="col-12">
                <q-input v-model="formData.address" label="Dirección" type="textarea" autogrow />
              </div>
            </div>

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
import { clientService } from '@/services/client.service';
import type { Client } from '@/types'

const $q = useQuasar()
const clients = ref<Client[]>([])
const loading = ref(true)
const saving = ref(false)
const showDialog = ref(false)
const isEdit = ref(false)

const formData = reactive<Client>({
  name: '',
  code: null,
  nitCi: '',
  phone: '',
  email: '',
  companyName: '',
  city: '',
  address: ''
})

const columns = [
  { name: 'id', label: 'ID', field: 'id', sortable: true, align: 'left' as const },
  { name: 'name', label: 'Nombre', field: 'name', sortable: true, align: 'left' as const },
  { name: 'nitCi', label: 'NIT/CI', field: 'nitCi', format: (val: string) => val || '', align: 'left' as const },
  { name: 'phone', label: 'Teléfono', field: 'phone', align: 'left' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const fetchClients = async () => {
  loading.value = true
  try {
    const response = await clientService.getAll()
    clients.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar clientes' })
  } finally {
    loading.value = false
  }
}

const openDialog = (client?: Client) => {
  if (client) {
    isEdit.value = true
    Object.assign(formData, { ...client })
  } else {
    isEdit.value = false
    Object.assign(formData, { name: '', code: null, nitCi: '', phone: '', email: '', companyName: '', city: '', address: '' })
  }
  showDialog.value = true
}

const saveClient = async () => {
  saving.value = true
  try {
    if (isEdit.value) {
      await clientService.update(formData.id!, formData)
      $q.notify({ color: 'positive', message: 'Cliente actualizado' })
    } else {
      await clientService.create(formData)
      $q.notify({ color: 'positive', message: 'Cliente creado' })
    }
    showDialog.value = false
    fetchClients()
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar cliente' })
  } finally {
    saving.value = false
  }
}

const confirmDelete = (client: Client) => {
  $q.dialog({
    title: 'Confirmar eliminación',
    message: `¿Eliminar a ${client.name}?`,
    cancel: true,
    persistent: true
  }).onOk(async () => {
    try {
      await clientService.delete(client.id!)
      $q.notify({ color: 'positive', message: 'Cliente eliminado' })
      fetchClients()
    } catch (error) {
      $q.notify({ color: 'negative', message: 'Error al eliminar cliente' })
    }
  })
}

onMounted(fetchClients)
</script>
