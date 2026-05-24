import axios, { type AxiosInstance } from "axios";
import { Notify } from "quasar";
import { useSettingsStore } from "@/stores/settings";

class ApiClient {
  private static instance: ApiClient;
  private axiosInstance: AxiosInstance;

  private constructor() {
    this.axiosInstance = axios.create({
      baseURL: process.env.VITE_API_URL || '/api',
    });

    this.setupInterceptors();
  }

  public static getInstance(): ApiClient {
    if (!ApiClient.instance) {
      ApiClient.instance = new ApiClient();
    }
    return ApiClient.instance;
  }

  public get api(): AxiosInstance {
    return this.axiosInstance;
  }

  private setupInterceptors() {
    this.axiosInstance.interceptors.request.use((config) => {
      const token = localStorage.getItem("token");
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    this.axiosInstance.interceptors.response.use(
      (response) => {
        const version = response.headers['x-settings-version'];
        if (version) {
          try {
            const settingsStore = useSettingsStore();
            settingsStore.updateSettingsVersion(parseInt(version, 10));
          } catch (e) {
            console.error("Error updating settings version in interceptor:", e);
          }
        }
        return response;
      },
      (error) => {
        if (error.response) {
          const status = error.response.status;
          const message = error.response.data?.message || error.response.data?.Message || "Ocurrió un error inesperado.";

          if (status === 401) {
            const isLoginRequest = error.config?.url?.includes("/auth/login");
            if (!isLoginRequest) {
              localStorage.removeItem("token");
              localStorage.removeItem("username");

              window.location.href = "/";
            }
          } else if (status === 403) {
            Notify.create({
              color: "negative",
              message: message || "No tienes permisos para realizar esta acción.",
              icon: "lock",
              position: "top-right",
              timeout: 4000
            });
          } else if (status >= 400 && status < 500) {
            Notify.create({
              color: "warning",
              message: message,
              icon: "warning",
              position: "top-right",
              timeout: 4000
            });
          } else if (status >= 500) {
            Notify.create({
              color: "negative",
              message: "Error interno del servidor. Por favor, intente de nuevo más tarde.",
              icon: "report",
              position: "top-right",
              timeout: 5000
            });
          }
        } else if (error.request) {
          Notify.create({
            color: "negative",
            message: "No se pudo conectar con el servidor. Verifique su conexión de red.",
            icon: "cloud_off",
            position: "top-right",
            timeout: 5000
          });
        }
        return Promise.reject(error);
      },
    );
  }
}

// Exportamos la instancia única (Singleton)
export default ApiClient.getInstance().api;
