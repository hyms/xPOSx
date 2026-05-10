<template>
  <q-page padding>
    <div class="row q-col-gutter-md">
      <div class="col-12">
        <q-table
          title="Usuarios"
          :rows="users"
          :columns="columns"
          row-key="id"
          :loading="loading"
        >
          <template v-slot:top-right>
            <q-btn color="primary" label="Nuevo Usuario" icon="add" @click="openDialog()" />
          </template>

          <template v-slot:body-cell-status="props">
            <q-td :props="props">
              <q-toggle
                :model-value="props.value"
                checked-icon="check"
                unchecked-icon="clear"
                color="positive"
                @update:model-value="handleToggleStatus(props.row)"
              />
            </q-td>
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

    <!-- User Dialog -->
    <q-dialog v-model="showDialog" persistent>
      <q-card style="min-width: 350px">
        <q-card-section>
          <div class="text-h6">{{ isEdit ? 'Editar Usuario' : 'Nuevo Usuario' }}</div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-form @submit="saveUser" class="q-gutter-md">
            <q-input
              v-model="formData.username"
              label="Nombre de Usuario"
              lazy-rules
              :rules="[ val => val && val.length > 0 || 'Campo requerido']"
            />
            <q-input
              v-model="formData.firstName"
              label="Nombre"
              lazy-rules
              :rules="[ val => val && val.length > 0 || 'Campo requerido']"
            />
            <q-input
              v-model="formData.lastName"
              label="Apellido"
            />
            <q-input
              v-model="formData.email"
              label="Email"
              type="email"
              lazy-rules
              :rules="[ val => val && val.length > 0 || 'Campo requerido']"
            />
            <q-input
              v-if="!isEdit"
              v-model="formData.password"
              label="Contraseña"
              type="password"
              lazy-rules
              :rules="[ val => val && val.length > 0 || 'Campo requerido']"
            />
            <q-select
              v-model="formData.role"
              :options="roleOptions"
              label="Rol"
              emit-value
              map-options
              lazy-rules
              :rules="[ val => !!val || 'Campo requerido']"
            />

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
import { userService } from '@/services/user.service';
import type { User } from '@/types'
import { roleService } from '@/services/role.service'
import { useConfirm } from '@/composables/useConfirm'

const $q = useQuasar()
const { confirmDelete } = useConfirm()
const users = ref<User[]>([])
const loading = ref(true)
const saving = ref(false)
const showDialog = ref(false)
const isEdit = ref(false)
const roleOptions = ref<any[]>([])

const formData = reactive<User>({
  username: '',
  email: '',
  password: '',
  firstName: '',
  lastName: '',
  role: 0,
  isActive: true
})

const columns = [
  { name: 'fullName', label: 'Nombre Completo', field: (row: User) => `${row.firstName} ${row.lastName}`, sortable: true, align: 'left' as const },
  { name: 'username', label: 'Usuario', field: 'username', sortable: true, align: 'left' as const },
  { name: 'email', label: 'Email', field: 'email', sortable: true, align: 'left' as const },
  { name: 'roleName', label: 'Rol', field: (row: User) => row.roleDetails?.name, sortable: true, align: 'left' as const },
  { name: 'status', label: 'Estado', field: 'isActive', sortable: true, align: 'center' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const fetchUsers = async () => {
  loading.value = true
  try {
    const response = await userService.getAll()
    users.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar usuarios' })
  } finally {
    loading.value = false
  }
}

const fetchRoles = async () => {
  try {
    const response = await roleService.getAll()
    roleOptions.value = response.data.map(r => ({ label: r.name, value: r.id }))
  } catch (error) {
    console.error('Error fetching roles:', error)
  }
}

const openDialog = (user?: User) => {
  if (user) {
    isEdit.value = true
    Object.assign(formData, { ...user, password: '' })
  } else {
    isEdit.value = false
    Object.assign(formData, { username: '', email: '', password: '', firstName: '', lastName: '', role: 0, isActive: true })
  }
  showDialog.value = true
}

const saveUser = async () => {
  saving.value = true
  try {
    if (isEdit.value) {
      await userService.update(formData.id!, formData)
      $q.notify({ color: 'positive', message: 'Usuario actualizado correctamente' })
    } else {
      await userService.create(formData)
      $q.notify({ color: 'positive', message: 'Usuario creado correctamente' })
    }
    showDialog.value = false
    fetchUsers()
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar usuario' })
  } finally {
    saving.value = false
  }
}

const confirmDeleteAction = (user: User) => {
  confirmDelete(user.username, async () => {
    await userService.delete(user.id!)
    fetchUsers()
  })
}

const handleToggleStatus = (user: User) => {
  const action = user.isActive ? 'desactivar' : 'activar'
  const color = user.isActive ? 'negative' : 'positive'

  $q.dialog({
    title: 'Confirmar',
    message: `¿Estás seguro de que quieres ${action} al usuario ${user.username}?`,
    cancel: true,
    persistent: true,
    ok: {
      color: color,
      label: action.charAt(0).toUpperCase() + action.slice(1)
    }
  }).onOk(async () => {
    try {
      await userService.toggleStatus(user.id!)
      $q.notify({
        color: 'positive',
        message: `Usuario ${action}do correctamente.`
      })
      fetchUsers()
    } catch (error) {
      $q.notify({
        color: 'negative',
        message: `Error al ${action} el usuario.`
      })
      // Revert toggle on error if needed, though fetchUsers() will handle it
    }
  })
}


onMounted(() => {
  fetchUsers()
  fetchRoles()
})
</script>
