import { boot } from 'quasar/wrappers'
import axios, { AxiosInstance } from 'axios'
import api from '@/api/index'

/**
 * Módulo de Boot para Axios en Quasar Framework.
 * 
 * Este archivo registra las instancias globales de Axios en la aplicación de Vue
 * para soportar el uso a través de Options API (this.$axios y this.$api).
 * 
 * Además, centraliza y re-exporta la instancia singleton 'api' configurada con
 * interceptores de seguridad y control defensivo de versiones (en 'src/api/index.ts')
 * para mitigar bucles infinitos en el cliente provocados por la caché agresiva de PWA.
 */

declare module '@vue/runtime-core' {
  interface ComponentCustomProperties {
    $axios: AxiosInstance;
    $api: AxiosInstance;
  }
}

export default boot(({ app }) => {
  // Registro global de instancias para mantener compatibilidad en componentes de Vue
  app.config.globalProperties.$axios = axios
  app.config.globalProperties.$api = api
})

export { api }
