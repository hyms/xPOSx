import type { RouteLocationNormalized } from 'vue-router';
import { useAuthStore } from '@/stores/auth';
import { useWarehouseStore } from '@/stores/warehouse';

// Obtener la ruta secreta de administración de las variables de entorno o usar 'admin' como fallback robusto
const secretLoginPath = import.meta.env.VITE_ADMIN_SECRET_PATH || 'admin';
const secretLoginFullPath = secretLoginPath.startsWith('/') ? secretLoginPath : `/${secretLoginPath}`;

/**
 * Determina si una ruta dada es pública y no requiere autenticación ni almacén activo.
 */
export function isPublicRoute(to: RouteLocationNormalized): boolean {
  const path = to.path.toLowerCase();
  const normalizedSecretPath = secretLoginFullPath.toLowerCase();

  // 1. Exclusiones exactas: raíz, catálogo, carrito, y la página de login secreta
  if (
    path === '/' ||
    path === '/productos' ||
    path === '/carrito' ||
    path === normalizedSecretPath
  ) {
    return true;
  }

  // 2. Ruta dinámica de CMS: /p/:slug
  if (path.startsWith('/p/')) {
    return true;
  }

  // 3. Verificación de metadata explícita
  if (to.meta.requiresAuth === false) {
    return true;
  }

  return false;
}

/**
 * Guardia de navegación de grado militar para Vue Router 4.
 * Retorna true para permitir navegación, o un string de ruta para redirigir.
 * 100% libre de callbacks 'next' obsoletos y asíncronamente seguro contra bucles.
 */
export async function setupNavigationGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  piniaStore: any
): Promise<boolean | string> {
  const authStore = useAuthStore(piniaStore);
  const warehouseStore = useWarehouseStore(piniaStore);

  const targetPath = to.path;

  // Caso 1: Si ya estamos navegando a la raíz '/', permitimos la navegación sin más lógica
  // Esto rompe cualquier posible recursión en la raíz de forma irrevocable.
  if (targetPath === '/') {
    return true;
  }

  // Caso 2: Bloquear de forma silenciosa e impenetrable los endpoints tradicionales de login/admin
  // para evitar la exposición innecesaria de accesos.
  if (secretLoginFullPath !== '/admin' && (targetPath === '/login' || targetPath === '/admin')) {
    return '/';
  } else if (secretLoginFullPath === '/admin' && targetPath === '/login') {
    return '/';
  }

  // Caso 3: Permitir libre acceso a rutas públicas exceptuadas
  if (isPublicRoute(to)) {
    // Si un usuario ya autenticado intenta ir al login secreto, lo movemos al dashboard privado
    if (targetPath.toLowerCase() === secretLoginFullPath.toLowerCase() && authStore.isAuthenticated) {
      return '/dashboard';
    }
    return true;
  }

  // Caso 4: Control de autenticación de grado militar para todas las rutas privadas
  if (!authStore.isAuthenticated) {
    console.warn(`[Router Guard] Acceso no autorizado a '${targetPath}'. Redirigiendo a la raíz.`);
    return '/';
  }

  // Caso 5: Validación robusta de Almacén Activo (warehouse_id) para vistas privadas
  const activeWarehouseId = warehouseStore.activeWarehouseId || localStorage.getItem('active_warehouse_id');
  if (!activeWarehouseId) {
    console.warn(`[Router Guard] Ruta privada '${targetPath}' accedida sin almacén activo. Intentando inicializar...`);
    try {
      await warehouseStore.fetchWarehouses();
    } catch (e) {
      console.error('[Router Guard] Error crítico al obtener almacenes:', e);
    }
    
    // Validar por segunda vez tras intentar inicializar
    const finalWarehouseId = warehouseStore.activeWarehouseId || localStorage.getItem('active_warehouse_id');
    if (!finalWarehouseId) {
      console.warn(`[Router Guard] Imposible asignar almacén. Redirigiendo de forma segura para evitar bucles.`);
      return '/';
    }
  }

  // Caso 6: Validación estricta de permisos basada en metadata de rutas
  if (to.meta.permission && !authStore.hasPermission(to.meta.permission as string)) {
    console.warn(`[Router Guard] Usuario carece de permisos para '${targetPath}'. Redirigiendo a la raíz.`);
    return '/';
  }

  // Todo correcto, permitir navegación limpia
  return true;
}
