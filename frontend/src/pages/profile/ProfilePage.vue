<template>
  <q-page padding>
    <div class="row q-col-gutter-md justify-center">
      <div class="col-12 col-md-8">
        <q-card>
          <q-card-section class="bg-primary text-white">
            <div class="text-h6">Mi Perfil</div>
          </q-card-section>

          <q-card-section class="q-pa-md">
            <q-form @submit="onUpdate" class="q-gutter-md">
              <div class="row q-col-gutter-md">
                <div class="col-12 col-md-6">
                  <q-input
                    v-model="userData.username"
                    label="Usuario"
                    outlined
                    dense
                    readonly
                  />
                </div>
                <div class="col-12 col-md-6">
                  <q-input
                    v-model="userData.email"
                    label="Email"
                    outlined
                    dense
                    lazy-rules
                    :rules="[val => !!val || 'Requerido', val => /.+@.+\..+/.test(val) || 'Email inválido']"
                  />
                </div>
                <div class="col-12 col-md-6">
                  <q-input
                    v-model="userData.firstName"
                    label="Nombre"
                    outlined
                    dense
                    lazy-rules
                    :rules="[val => !!val || 'Requerido']"
                  />
                </div>
                <div class="col-12 col-md-6">
                  <q-input
                    v-model="userData.lastName"
                    label="Apellido"
                    outlined
                    dense
                  />
                </div>
                <div class="col-12 col-md-6">
                  <q-input
                    v-model="userData.newPassword"
                    label="Nueva Contraseña"
                    type="password"
                    outlined
                    dense
                    lazy-rules
                    :rules="[val => !val || val.length >= 6 || 'Mínimo 6 caracteres']"
                  />
                </div>
              </div>

              <div v-if="errorMessage" class="q-pa-md bg-negative text-white rounded-borders">
                {{ errorMessage }}
              </div>

              <div class="row justify-end q-mt-md">
                <q-btn
                  label="Actualizar Perfil"
                  type="submit"
                  color="primary"
                  :loading="loading"
                />
              </div>
            </q-form>
          </q-card-section>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { useQuasar } from 'quasar'
import { userService } from '@/services/user.service'
import type { User } from '@/types'
import { useAuthStore } from '@/stores/auth'

const $q = useQuasar()
const authStore = useAuthStore()
const loading = ref(false)
const errorMessage = ref('')

const userData = reactive<User & { newPassword?: string }>({
  username: '',
  email: '',
  firstName: '',
  lastName: '',
  role: 0
})

const fetchProfile = async () => {
  try {
    const response = await userService.getProfile()
    Object.assign(userData, response.data)
  } catch (error: any) {
    errorMessage.value = 'Error al cargar el perfil'
  }
}

const onUpdate = async () => {
  loading.value = true
  errorMessage.value = ''
  try {
    const data: any = {
      email: userData.email,
      firstName: userData.firstName,
      lastName: userData.lastName
    }
    if (userData.newPassword) {
      data.newPassword = userData.newPassword
    }
    await userService.updateProfile(data)
    $q.notify({
      color: 'positive',
      message: 'Perfil actualizado con éxito',
      icon: 'check_circle'
    })
    userData.newPassword = ''
    if (userData.username) authStore.username = userData.username
  } catch (error: any) {
    errorMessage.value = error.response?.data?.message || error.response?.data || 'Error al actualizar el perfil'
  } finally {
    loading.value = false
  }
}

onMounted(fetchProfile)
</script>