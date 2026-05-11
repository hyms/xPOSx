<template>
  <q-page class="flex flex-center bg-grey-2">
    <q-card style="width: 400px; max-width: 90vw;">
      <q-card-section class="text-white text-center q-pa-lg" :style="{ backgroundColor: 'var(--color-primary)' }">
        <div class="text-h4" :style="{ fontFamily: 'var(--font-family-display)' }">xPOSx</div>
        <div class="text-subtitle2" :style="{ fontFamily: 'var(--font-family-body)' }">Panel de Administración</div>
      </q-card-section>

      <q-card-section class="q-pa-lg">
        <q-form @submit="onSubmit" class="q-gutter-md">
          <q-input
            v-model="loginData.username"
            label="Usuario"
            filled
            lazy-rules
            autocomplete="username"
            :rules="[ val => val && val.length > 0 || 'Por favor ingresa tu usuario']"
          >
            <template v-slot:prepend>
              <q-icon name="person" />
            </template>
          </q-input>

          <q-input
            v-model="loginData.password"
            type="password"
            label="Contraseña"
            filled
            lazy-rules
            autocomplete="current-password"
            :rules="[ val => val && val.length > 0 || 'Por favor ingresa tu contraseña']"
          >
            <template v-slot:prepend>
              <q-icon name="lock" />
            </template>
          </q-input>

          <div class="q-mt-lg">
            <q-btn
              label="Iniciar Sesión"
              type="submit"
              color="primary"
              class="full-width"
              size="lg"
              :loading="authStore.loading"
            />
          </div>
        </q-form>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const $q = useQuasar()
const authStore = useAuthStore()

const loginData = reactive({
  username: '',
  password: ''
})

const onSubmit = async () => {
  try {
    const success = await authStore.login(loginData)
    if (success) {
      $q.notify({
        color: 'positive',
        message: 'Bienvenido de nuevo',
        icon: 'check_circle'
      })
      router.push('/')
    }
  } catch (error: any) {
    let message = 'Error al iniciar sesión'
    if (error.response?.data) {
      message = typeof error.response.data === 'object' 
        ? error.response.data.message || JSON.stringify(error.response.data)
        : error.response.data
    }
    $q.notify({
      color: 'negative',
      message: message,
      icon: 'error',
      position: 'top',
      timeout: 5000
    })
  }
}
</script>
