import { register } from 'register-service-worker';
import { Notify } from 'quasar';

// El ciclo de vida del ServiceWorkerRegistration maneja eventos críticos de caché.
// Documentación: https://developer.mozilla.org/en-US/docs/Web/API/ServiceWorkerRegistration

register(process.env.SERVICE_WORKER_FILE, {
  // Las opciones de registro se pasarán como segundo argumento a ServiceWorkerContainer.register()
  // registrationOptions: { scope: './' },

  ready(/* registration */) {
    console.log('[PWA] El Service Worker está activo y sirviendo la aplicación desde la caché.');
  },

  registered(/* registration */) {
    console.log('[PWA] El Service Worker ha sido registrado exitosamente.');
  },

  cached(/* registration */) {
    console.log('[PWA] El contenido de la aplicación ha sido cacheado para uso offline.');
  },

  updatefound(/* registration */) {
    console.log('[PWA] Se ha detectado nuevo contenido. Descargando assets de la nueva versión...');
  },

  updated(registration: ServiceWorkerRegistration) {
    console.log('[PWA] ¡Nueva versión disponible! Purga de caché y actualización activa en curso.');

    // Notificar al usuario elegantemente mediante un banner de Quasar Notify
    Notify.create({
      type: 'info',
      color: 'primary',
      textColor: 'white',
      message: '¡Nueva versión disponible!',
      caption: 'Se aplicarán los cambios de forma segura.',
      icon: 'system_update',
      position: 'bottom-right',
      timeout: 5000,
      actions: [
        {
          label: 'Actualizar',
          color: 'white',
          handler: () => {
            applyUpdate(registration);
          }
        }
      ]
    });

    // Como fallback de fondo, forzamos la actualización pasados 4 segundos para evitar estados inconsistentes
    setTimeout(() => {
      applyUpdate(registration);
    }, 4000);
  },

  offline() {
    console.log('[PWA] Sin conexión a Internet detectada. La aplicación se está ejecutando en modo offline.');
  },

  error(err) {
    console.error('[PWA] Error durante el registro del Service Worker:', err);
  },
});

/**
 * Purgar las cachés de Workbox/Navegador y aplicar la nueva versión de forma limpia.
 */
function applyUpdate(registration: ServiceWorkerRegistration) {
  if (registration && registration.waiting) {
    // Indicar al Service Worker en espera que ejecute skipWaiting() y tome el control
    registration.waiting.postMessage({ type: 'SKIP_WAITING' });
  }

  // Purgar las cachés de almacenamiento para asegurar que el index.html de la versión vieja desaparezca
  if ('caches' in window) {
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames.map((cacheName) => {
          console.log(`[PWA] Eliminando caché obsoleta: ${cacheName}`);
          return caches.delete(cacheName);
        })
      );
    }).then(() => {
      console.log('[PWA] Cachés purgadas exitosamente. Recargando aplicación...');
      window.location.reload();
    }).catch((err) => {
      console.error('[PWA] Error al purgar cachés:', err);
      window.location.reload();
    });
  } else {
    window.location.reload();
  }
}
