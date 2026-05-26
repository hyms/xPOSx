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
        const versionHeader = response.headers['x-settings-version'] || response.headers['X-Settings-Version'];
        if (versionHeader) {
          try {
            const version = parseInt(versionHeader as string, 10);
            
            // Sincronizar el estado en el store de settings
            const settingsStore = useSettingsStore();
            settingsStore.updateSettingsVersion(version);

            // Obtener versión registrada localmente
            const savedVersionRaw = localStorage.getItem('settings_version');
            const savedVersion = savedVersionRaw ? parseInt(savedVersionRaw, 10) : null;

            if (savedVersion !== null && savedVersion < version) {
              // Algoritmo defensivo para prevenir bucles infinitos de recarga (máximo 2 recargas en 10 segundos)
              const reloadHistoryKey = 'settings_reload_history';
              const now = Date.now();
              let history: number[] = [];
              try {
                const raw = localStorage.getItem(reloadHistoryKey);
                if (raw) history = JSON.parse(raw);
              } catch (e) {
                history = [];
              }

              // Filtrar reintentos fuera de la ventana de 10 segundos
              history = history.filter(ts => now - ts < 10000);

              if (history.length < 2) {
                history.push(now);
                localStorage.setItem(reloadHistoryKey, JSON.stringify(history));
                localStorage.setItem('settings_version', version.toString());
                
                console.warn(`[Axios Interceptor] Disparidad de versión de configuraciones detectada (Local: ${savedVersion}, Servidor: ${version}). Forzando recarga de seguridad.`);
                
                // Forzar la recarga de forma asíncrona y segura
                setTimeout(() => {
                  window.location.reload();
                }, 100);
              } else {
                console.error(`[Axios Interceptor] Bucle de recarga infinito prevenido. Disparidad persistente de versión de settings (Local: ${savedVersion}, Servidor: ${version}) pero se alcanzó el límite defensivo.`);
              }
            } else {
              // Mantener sincronizado el almacenamiento local
              localStorage.setItem('settings_version', version.toString());
            }
          } catch (e) {
            console.error("Error al validar versión de configuraciones en el interceptor:", e);
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
              const hadToken = !!localStorage.getItem("token");
              localStorage.removeItem("token");
              localStorage.removeItem("username");
              localStorage.removeItem("permissions");
              localStorage.removeItem("active_warehouse_id");

              // EVITAR BUCLE INFINITO DE RECARGAS EN LA RAÍZ:
              // Solo redirigir a '/' si el usuario no se encuentra ya en un endpoint público/raíz,
              // y si realmente tenía un token guardado (lo que denota una expiración real de sesión activa).
              const isPublicPath = ["/", "/productos", "/carrito"].includes(window.location.pathname) || window.location.pathname.startsWith("/p/");
              if (!isPublicPath && hadToken) {
                console.warn("[Axios Interceptor] Sesión privada expirada (401). Redirigiendo a raíz.");
                window.location.href = "/";
              } else {
                console.info("[Axios Interceptor] Error 401 en ruta pública o sin token activo. No se fuerza recarga.");
              }
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
