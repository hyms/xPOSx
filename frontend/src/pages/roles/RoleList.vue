<template>
  <q-page padding>
    <div class="row q-col-gutter-md">
      <div class="col-12">
        <q-table
          title="Roles"
          :rows="roles"
          :columns="columns"
          row-key="id"
          :loading="loading"
          :filter="filter"
        >
          <template v-slot:top-right>
            <q-input v-model="filter" debounce="300" placeholder="Buscar rol..." dense borderless>
              <template v-slot:prepend>
                <q-icon name="search" />
              </template>
            </q-input>
            <q-btn color="primary" label="Nuevo Rol" icon="add" @click="openDialog()" class="q-ml-md" />
          </template>

          <template v-slot:body-cell-actions="props">
            <q-td :props="props">
              <q-btn flat round color="primary" icon="edit" @click="openDialog(props.row)" />
              <q-btn flat round color="secondary" icon="key" @click="openPermissionsDialog(props.row)" />
              <q-btn flat round color="negative" icon="delete" @click="confirmDelete(props.row)" />
            </q-td>
          </template>
        </q-table>
      </div>
    </div>

    <!-- Role Dialog -->
    <FormDialog
      v-model="showDialog"
      :title="isEdit ? 'Editar Rol' : 'Nuevo Rol'"
      @submit="saveRole"
      :saving="saving"
    >
      <q-input
        v-model="formData.name"
        label="Nombre del Rol"
        lazy-rules
        :rules="[ val => !!val || 'Requerido']"
        outlined
        dense
      />
      <q-input
        v-model="formData.description"
        label="Descripción"
        type="textarea"
        autogrow
        outlined
        dense
      />
    </FormDialog>

    <!-- Permissions Dialog -->
    <q-dialog v-model="showPermissionsDialog" persistent min-width="500px">
      <q-card>
        <q-card-section class="row items-center q-pb-none">
          <div class="text-h6">Asignar Permisos: {{ selectedRole?.name }}</div>
          <q-space />
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <q-card-section class="q-pt-none scroll" style="max-height: 50vh">
          <q-input v-model="permissionFilter" debounce="300" placeholder="Buscar permiso..." dense class="q-mb-md" outlined>
            <template v-slot:prepend>
              <q-icon name="search" />
            </template>
          </q-input>
          <q-list bordered separator>
            <q-item v-for="permission in filteredPermissions" :key="permission.id" tag="label" v-ripple>
              <q-item-section side top>
                <q-checkbox v-model="assignedPermissionIds" :val="permission.id" />
              </q-item-section>

              <q-item-section>
                <q-item-label>{{ permission.name }}</q-item-label>
                <q-item-label caption>{{ permission.description }}</q-item-label>
              </q-item-section>
            </q-item>
          </q-list>
        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" color="primary" v-close-popup />
          <q-btn label="Guardar Permisos" color="primary" @click="savePermissions" :loading="savingPermissions" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, computed } from 'vue'
import { useQuasar } from 'quasar'
import { roleService } from '@/services/role.service';
import type { Role } from '@/types'
import { permissionService } from '@/services/permission.service'
import FormDialog from '@/components/FormDialog.vue'
import type { Permission } from '@/types'

const $q = useQuasar()
const roles = ref<Role[]>([])
const allPermissions = ref<Permission[]>([])
const assignedPermissionIds = ref<number[]>([])
const loading = ref(true)
const saving = ref(false)
const savingPermissions = ref(false)
const showDialog = ref(false)
const showPermissionsDialog = ref(false)
const isEdit = ref(false)
const selectedRole = ref<Role | null>(null)
const filter = ref('')
const permissionFilter = ref('')

const formData = reactive<Role>({
  name: '',
  description: ''
})

const columns = [
  { name: 'id', label: 'ID', field: 'id', sortable: true, align: 'left' as const },
  { name: 'name', label: 'Nombre', field: 'name', sortable: true, align: 'left' as const },
  { name: 'description', label: 'Descripción', field: 'description', sortable: true, align: 'left' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const fetchRoles = async () => {
  loading.value = true
  try {
    const response = await roleService.getAll()
    roles.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar roles' })
  } finally {
    loading.value = false
  }
}

const fetchAllPermissions = async () => {
  try {
    const response = await permissionService.getAll()
    allPermissions.value = response.data
  } catch (error) {
    console.error('Error fetching permissions:', error)
  }
}

const openDialog = (role?: Role) => {
  if (role) {
    isEdit.value = true
    Object.assign(formData, { ...role })
  } else {
    isEdit.value = false
    Object.assign(formData, { name: '', description: '' })
  }
  showDialog.value = true
}

const openPermissionsDialog = async (role: Role) => {
  selectedRole.value = role
  assignedPermissionIds.value = []
  showPermissionsDialog.value = true
  
  try {
    // In a real scenario, the backend role object might already have permission IDs
    // or we might need a specific endpoint to get them.
    // For now, let's assume we fetch the role details which include permissions.
    const response = await roleService.getById(role.id!)
    // Assuming backend returns permissions as objects with ID
    if (response.data.permissions) {
      // @ts-ignore (mapping based on assumed API response)
      assignedPermissionIds.value = response.data.permissions.map((p: Permission) => p.id || p)
    }
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar permisos del rol' })
  }
}

const saveRole = async () => {
  saving.value = true
  try {
    if (isEdit.value) {
      await roleService.update(formData.id!, formData)
      $q.notify({ color: 'positive', message: 'Rol actualizado correctamente' })
    } else {
      await roleService.create(formData)
      $q.notify({ color: 'positive', message: 'Rol creado correctamente' })
    }
    showDialog.value = false
    fetchRoles()
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar rol' })
  } finally {
    saving.value = false
  }
}

const savePermissions = async () => {
  if (!selectedRole.value) return
  savingPermissions.value = true
  try {
    await roleService.assignPermissions(selectedRole.value.id!, assignedPermissionIds.value)
    $q.notify({ color: 'positive', message: 'Permisos asignados correctamente' })
    showPermissionsDialog.value = false
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al asignar permisos' })
  } finally {
    savingPermissions.value = false
  }
}

const confirmDelete = (role: Role) => {
  $q.dialog({
    title: 'Confirmar eliminación',
    message: `¿Estás seguro de que quieres eliminar el rol ${role.name}?`,
    cancel: true,
    persistent: true
  }).onOk(async () => {
    try {
      await roleService.delete(role.id!)
      $q.notify({ color: 'positive', message: 'Rol eliminado' })
      fetchRoles()
    } catch (error) {
      $q.notify({ color: 'negative', message: 'Error al eliminar rol' })
    }
  })
}

onMounted(() => {
  fetchRoles()
  fetchAllPermissions()
})

const filteredPermissions = computed(() => {
  if (!permissionFilter.value) return allPermissions.value
  const search = permissionFilter.value.toLowerCase()
  return allPermissions.value.filter(p =>
    (p.name ?? '').toLowerCase().includes(search) ||
    (p.description ?? '').toLowerCase().includes(search)
  )
})
</script>
