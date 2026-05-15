import axios, { type AxiosInstance } from "axios";

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
      (response) => response,
      (error) => {
        if (error.response && error.response.status === 401) {
          const isLoginRequest = error.config?.url?.includes("/auth/login");
          if (!isLoginRequest) {
            localStorage.removeItem("token");
            localStorage.removeItem("username");

            window.location.href = "/login";
          }
        }
        return Promise.reject(error);
      },
    );
  }
}

// Exportamos la instancia única (Singleton)
export default ApiClient.getInstance().api;
