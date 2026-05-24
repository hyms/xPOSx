import { route } from 'quasar/wrappers'
import {
  createMemoryHistory,
  createRouter,
  createWebHashHistory,
  createWebHistory,
} from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import routes from './routes'

/*
 * If not building with SSR mode, you can
 * directly export the Router instantiation;
 *
 * The function below can be async too; either use
 * async/await or return a Promise which resolves
 * with the Router instance.
 */

export default route(function ({ store /*, ssrContext */ }) {
  const createHistory = process.env.SERVER
    ? createMemoryHistory
    : (process.env.VUE_ROUTER_MODE === 'history' ? createWebHistory : createWebHashHistory)

  const Router = createRouter({
    scrollBehavior: () => ({ left: 0, top: 0 }),
    routes,

    // Leave this as is and make changes in quasar.config.js instead!
    // vueRouterMode [hash|history]
    // vueRouterBase [optional]
    history: createHistory(process.env.VUE_ROUTER_BASE)
  })

  Router.beforeEach((to) => {
    const authStore = useAuthStore(store)

    // Block traditional routes completely & silently
    if (to.path === '/login' || to.path === '/admin') {
      return '/'
    }

    if (to.meta.requiresAuth && !authStore.isAuthenticated) {
      // Silent redirect to home instead of exposing a login page
      return '/'
    } else if (to.meta.guest && authStore.isAuthenticated) {
      // If already logged in and visiting secret entry point, send to dashboard
      return '/dashboard'
    }

    // Permission check
    if (to.meta.permission && !authStore.hasPermission(to.meta.permission as string)) {
      return '/'
    }
  })

  return Router
})
