import { route } from 'quasar/wrappers'
import {
  createMemoryHistory,
  createRouter,
  createWebHashHistory,
  createWebHistory,
} from 'vue-router'
import { useAuthStore } from '@/stores/auth'

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
    routes: [
      {
        path: '/login',
        component: () => import('../layouts/GuestLayout.vue'),
        meta: { guest: true },
        children: [
          { path: '', component: () => import('../pages/auth/LoginPage.vue') }
        ]
      },
      {
        path: '/',
        component: () => import('../layouts/MainLayout.vue'),
        meta: { requiresAuth: true },
        children: [
          { path: '', component: () => import('../pages/home/IndexPage.vue') },
          { path: 'users', component: () => import('../pages/users/UserList.vue'), meta: { permission: 'users_view' } },
          { path: 'roles', component: () => import('../pages/roles/RoleList.vue'), meta: { permission: 'roles_view' } },
          { path: 'permissions', component: () => import('../pages/permissions/PermissionList.vue'), meta: { permission: 'permissions_view' } },
          { path: 'warehouses', component: () => import('../pages/warehouses/WarehouseList.vue'), meta: { permission: 'warehouses_view' } },
          { path: 'pos', component: () => import('../pages/pos/PosPage.vue'), meta: { permission: 'pos_view' } },
          { path: 'sales', component: () => import('../pages/sales/SaleList.vue'), meta: { permission: 'sales_view' } },
          { path: 'sales/create', component: () => import('../pages/sales/SaleForm.vue'), meta: { permission: 'sales_create' } },
          { path: 'quotations', component: () => import('../pages/quotations/QuotationList.vue'), meta: { permission: 'quotations_view' } },
          { path: 'quotations/create', component: () => import('../pages/quotations/QuotationForm.vue'), meta: { permission: 'quotations_create' } },
          { path: 'categories', component: () => import('../pages/categories/CategoryList.vue'), meta: { permission: 'categories_view' } },
          { path: 'units', component: () => import('../pages/units/UnitList.vue'), meta: { permission: 'units_view' } },
          { path: 'products', component: () => import('../pages/products/ProductList.vue'), meta: { permission: 'products_view' } },
          { path: 'purchases', component: () => import('../pages/purchases/PurchaseList.vue'), meta: { permission: 'purchases_view' } },
          { path: 'purchases/create', component: () => import('../pages/purchases/PurchaseForm.vue'), meta: { permission: 'purchases_create' } },
          { path: 'purchases/:id', component: () => import('../pages/purchases/PurchaseForm.vue'), meta: { permission: 'purchases_view' } },
          { path: 'expenses', component: () => import('../pages/expenses/ExpenseList.vue'), meta: { permission: 'expenses_view' } },
          { path: 'returns', component: () => import('../pages/returns/ReturnList.vue'), meta: { permission: 'returns_view' } },
          { path: 'returns/sales/create', component: () => import('../pages/returns/SaleReturnForm.vue'), meta: { permission: 'returns_view' } },
          { path: 'returns/sales/:id', component: () => import('../pages/returns/SaleReturnForm.vue'), meta: { permission: 'returns_view' } },
          { path: 'returns/purchases/create', component: () => import('../pages/returns/PurchaseReturnForm.vue'), meta: { permission: 'returns_view' } },
          { path: 'adjustments', component: () => import('../pages/adjustments/AdjustmentList.vue'), meta: { permission: 'adjustments_view' } },
          { path: 'adjustments/create', component: () => import('../pages/adjustments/AdjustmentForm.vue'), meta: { permission: 'adjustments_create' } },
          { path: 'transfers', component: () => import('../pages/transfers/TransferList.vue'), meta: { permission: 'transfers_view' } },
          { path: 'transfers/create', component: () => import('../pages/transfers/TransferForm.vue'), meta: { permission: 'transfers_create' } },
          { path: 'clients', component: () => import('../pages/clients/ClientList.vue'), meta: { permission: 'clients_view' } },
          { path: 'providers', component: () => import('../pages/providers/ProviderList.vue'), meta: { permission: 'providers_view' } },
          { path: 'profile', component: () => import('../pages/profile/ProfilePage.vue') },
          { path: 'reports', component: () => import('../pages/reports/ReportsPage.vue'), meta: { permission: 'reports_view' } },
          { path: 'settings', component: () => import('../pages/settings/SettingsPage.vue'), meta: { permission: 'settings_view' } },
          {
            path: '/:catchAll(.*)*',
            component: () => import('../pages/ErrorNotFound.vue')
          }
        ],
      },
      {
        path: '/sales/print/:id',
        component: () => import('../layouts/PrintLayout.vue'),
        meta: { requiresAuth: true, permission: 'sales_view' },
        children: [
          { path: '', component: () => import('../components/VoucherPrint.vue'), props: true }
        ]
      },
      {
        path: '/purchases/print/:id',
        component: () => import('../layouts/PrintLayout.vue'),
        meta: { requiresAuth: true, permission: 'purchases_view' },
        children: [
          { path: '', component: () => import('../components/PurchasePrint.vue'), props: true }
        ]
      },
      {
        path: '/quotations/print/:id',
        component: () => import('../layouts/PrintLayout.vue'),
        meta: { requiresAuth: true, permission: 'quotations_view' },
        children: [
          { path: '', component: () => import('../components/QuotationPrint.vue'), props: true }
        ]
      },
      {
        path: '/returns/sales/print/:id',
        component: () => import('../layouts/PrintLayout.vue'),
        meta: { requiresAuth: true, permission: 'returns_view' },
        children: [
          { path: '', component: () => import('../components/SaleReturnPrint.vue'), props: true }
        ]
      },
      {
        path: '/returns/purchases/print/:id',
        component: () => import('../layouts/PrintLayout.vue'),
        meta: { requiresAuth: true, permission: 'returns_view' },
        children: [
          { path: '', component: () => import('../components/PurchaseReturnPrint.vue'), props: true }
        ]
      }
    ],

    // Leave this as is and make changes in quasar.config.js instead!
    // vueRouterMode [hash|history]
    // vueRouterBase [optional]
    history: createHistory(process.env.VUE_ROUTER_BASE)
  })

  Router.beforeEach((to) => {
    const authStore = useAuthStore(store)
    
    if (to.meta.requiresAuth && !authStore.isAuthenticated) {
      return '/login'
    } else if (to.meta.guest && authStore.isAuthenticated) {
      return '/'
    }

    // Permission check
    if (to.meta.permission && !authStore.hasPermission(to.meta.permission as string)) {
      return '/' // Or a forbidden page
    }
  })

  return Router
})

