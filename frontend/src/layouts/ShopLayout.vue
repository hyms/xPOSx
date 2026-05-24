<template>
  <q-layout view="lHh Lpr lFf" :class="$q.dark.isActive ? 'bg-dark text-white' : 'bg-grey-1 text-grey-9'">
    <!-- Modern Elegant Glassmorphism Header -->
    <q-header elevated class="glass-header q-py-xs" :class="$q.dark.isActive ? 'bg-grey-10-glass' : 'bg-white-glass'">
      <q-toolbar class="container max-width-lg q-mx-auto">
        <q-btn flat no-caps class="q-mr-sm" to="/">
          <q-avatar size="36px" class="q-mr-sm">
            <q-img :src="settingsStore.getLogoUrl" />
          </q-avatar>
          <span class="text-weight-bold text-h6 text-primary tracking-wide">
            {{ settingsStore.settings?.companyName || 'xPOSx Shop' }}
          </span>
        </q-btn>

        <q-space />

        <!-- Navigation Links -->
        <div class="gt-xs row q-gutter-md q-mr-lg">
          <q-btn flat no-caps label="Inicio" to="/" class="nav-link" active-class="text-primary text-weight-bold" />
          <q-btn flat no-caps label="Catálogo" to="/catalog" class="nav-link" active-class="text-primary text-weight-bold" />
          <q-btn flat no-caps label="Términos" to="/page/terminos-y-condiciones" class="nav-link" active-class="text-primary text-weight-bold" />
        </div>

        <!-- Action Icons -->
        <div class="row items-center q-gutter-sm">
          <!-- Dark Mode Toggle -->
          <q-btn flat round dense :icon="$q.dark.isActive ? 'light_mode' : 'dark_mode'" @click="toggleDarkMode" class="text-primary" />

          <!-- Shopping Cart Badge -->
          <q-btn flat round dense icon="shopping_cart" to="/checkout" class="text-primary q-mr-sm">
            <q-badge v-if="cartStore.totalItems > 0" color="red" floating rounded>
              {{ cartStore.totalItems }}
            </q-badge>
          </q-btn>

          <!-- Backoffice Portal Button -->
          <q-btn outline no-caps color="primary" label="POS Admin" to="/admin" class="backoffice-btn xs-hide" icon="dashboard" />
        </div>
      </q-toolbar>
    </q-header>

    <!-- Mobile Navigation Drawer / Bottom Navigation -->
    <q-footer class="lt-sm bg-white text-grey-9 shadow-2 border-top" style="z-index: 2000;">
      <q-tabs active-color="primary" class="text-grey-7" dense mobile-arrows>
        <q-route-tab to="/" icon="home" label="Inicio" />
        <q-route-tab to="/catalog" icon="grid_view" label="Catálogo" />
        <q-route-tab to="/checkout" icon="shopping_cart" label="Carrito" />
        <q-route-tab to="/page/terminos-y-condiciones" icon="gavel" label="Términos" />
      </q-tabs>
    </q-footer>

    <q-page-container>
      <router-view />
    </q-page-container>

    <!-- Beautiful Footer -->
    <footer class="q-py-xl text-center border-top bg-glass-footer" :class="$q.dark.isActive ? 'bg-grey-10 text-grey-4' : 'bg-grey-2 text-grey-7'">
      <div class="container q-px-md q-mx-auto">
        <div class="row q-col-gutter-lg justify-center text-left">
          <div class="col-12 col-sm-4 text-center text-sm-left">
            <span class="text-weight-bold text-h6 text-primary">{{ settingsStore.settings?.companyName || 'xPOSx Shop' }}</span>
            <p class="q-mt-sm">Tu portal de compras rápido, seguro y con pre-venta QR integrada.</p>
          </div>
          <div class="col-6 col-sm-3 offset-sm-1">
            <span class="text-weight-bold text-subtitle1 text-primary">Enlaces</span>
            <ul class="footer-links q-pl-none q-mt-sm" style="list-style: none;">
              <li><router-link to="/" class="footer-link">Inicio</router-link></li>
              <li><router-link to="/catalog" class="footer-link">Catálogo</router-link></li>
              <li><router-link to="/checkout" class="footer-link">Carrito de Compras</router-link></li>
            </ul>
          </div>
          <div class="col-6 col-sm-3">
            <span class="text-weight-bold text-subtitle1 text-primary">Legal</span>
            <ul class="footer-links q-pl-none q-mt-sm" style="list-style: none;">
              <li><router-link to="/page/terminos-y-condiciones" class="footer-link">Términos y Condiciones</router-link></li>
            </ul>
          </div>
        </div>
        <div class="q-mt-lg text-caption">
          © {{ new Date().getFullYear() }} {{ settingsStore.settings?.companyName || 'xPOSx' }}. Todos los derechos reservados.
        </div>
      </div>
    </footer>
  </q-layout>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useQuasar } from 'quasar';
import { useSettingsStore } from '@/stores/settings';
import { useCartStore } from '@/stores/cart';

const $q = useQuasar();
const settingsStore = useSettingsStore();
const cartStore = useCartStore();

onMounted(async () => {
  if (!settingsStore.settings) {
    await settingsStore.fetchSettings();
  }
});

const toggleDarkMode = () => {
  $q.dark.toggle();
};
</script>

<style scoped>
.glass-header {
  backdrop-filter: blur(15px);
  -webkit-backdrop-filter: blur(15px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  color: inherit !important;
}

.bg-white-glass {
  background-color: rgba(255, 255, 255, 0.8) !important;
}

.bg-grey-10-glass {
  background-color: rgba(29, 29, 29, 0.8) !important;
}

.nav-link {
  transition: all 0.2s ease;
  font-size: 1rem;
}

.backoffice-btn {
  border-radius: 8px;
  font-weight: 500;
}

.border-top {
  border-top: 1px solid rgba(0, 0, 0, 0.08);
}

.body--dark .border-top {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.footer-link {
  color: inherit;
  text-decoration: none;
  display: inline-block;
  padding: 4px 0;
  transition: color 0.2s ease;
}

.footer-link:hover {
  color: var(--q-primary);
}

.tracking-wide {
  letter-spacing: 0.05em;
}
</style>
