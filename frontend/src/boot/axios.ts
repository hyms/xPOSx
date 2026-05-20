import { boot } from 'quasar/wrappers'
import axios, { AxiosInstance } from 'axios'
import { useSettingsStore } from '@/stores/settings'

declare module '@vue/runtime-core' {
  interface ComponentCustomProperties {
    $axios: AxiosInstance;
    $api: AxiosInstance;
  }
}

// Be careful when using SSR for client-side substitutions.
// This is a template; feel free to customize.
const api = axios.create({ baseURL: process.env.VITE_API_URL || '/api' })

api.interceptors.response.use(
  (response) => {
    const version = response.headers['x-settings-version'];
    if (version) {
      const settingsStore = useSettingsStore();
      settingsStore.updateSettingsVersion(parseInt(version, 10));
    }
    return response;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export default boot(({ app }) => {
  // for use inside Vue files (Options API) through this.$axios and this.$api

  app.config.globalProperties.$axios = axios
  // ^ ^ ^ this will allow you to use this.$axios (for Vue Options API form)
  //       so you won't necessarily have to import axios in each vue file

  app.config.globalProperties.$api = api
  // ^ ^ ^ this will allow you to use this.$api (for Vue Options API form)
  //       so you can easily perform requests against your app's API
})

export { api }
