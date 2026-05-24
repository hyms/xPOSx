<template>
  <q-layout view="lHh Lpr lFf" :class="$q.dark.isActive ? 'bg-dark text-white' : 'bg-grey-1 text-grey-9'">
    <!-- Modern Elegant Glassmorphism Header -->
    <q-header elevated class="glass-header q-py-xs" :class="$q.dark.isActive ? 'bg-grey-10-glass' : 'bg-white-glass'">
      <q-toolbar class="container max-width-lg q-mx-auto header-toolbar justify-between">
        
        <!-- Brand / Logo Area -->
        <div class="row items-center">
          <!-- Hamburger menu button on mobile/tablet (lt-md) -->
          <q-btn
            flat
            dense
            round
            icon="menu"
            aria-label="Menu"
            @click="toggleLeftDrawer"
            class="lt-md q-mr-sm text-primary touch-btn-48"
          />

          <router-link to="/" class="row items-center cursor-pointer no-decoration">
            <q-avatar size="36px" class="q-mr-sm">
              <q-img :src="settingsStore.getLogoUrl" alt="logo" />
            </q-avatar>
            <span class="text-weight-bold text-h6 text-primary tracking-wide xs-hide">
              {{ settingsStore.settings?.companyName || 'xPOSx Shop' }}
            </span>
          </router-link>
        </div>

        <!-- Desktop Navigation Links (gt-sm) -->
        <div class="gt-sm row q-gutter-md">
          <q-btn 
            flat 
            no-caps 
            label="Inicio" 
            to="/" 
            class="nav-link touch-btn-48" 
            active-class="text-primary text-weight-bold" 
          />
          <q-btn 
            flat 
            no-caps 
            label="Catálogo" 
            to="/productos" 
            class="nav-link touch-btn-48" 
            active-class="text-primary text-weight-bold" 
          />
          <q-btn 
            flat 
            no-caps 
            label="Términos" 
            to="/p/terminos-y-condiciones" 
            class="nav-link touch-btn-48" 
            active-class="text-primary text-weight-bold" 
          />
        </div>

        <!-- Action Icons (Dark Mode & Shopping Cart) -->
        <div class="row items-center q-gutter-md">
          <!-- Dark Mode Toggle -->
          <q-btn 
            flat 
            round 
            dense 
            :icon="$q.dark.isActive ? 'light_mode' : 'dark_mode'" 
            @click="toggleDarkMode" 
            class="text-primary touch-btn-48" 
          />

          <!-- Shopping Cart Badge Button -->
          <q-btn 
            flat 
            round 
            dense 
            icon="shopping_cart" 
            to="/carrito" 
            class="text-primary touch-btn-48 relative-position"
          >
            <q-badge v-if="cartStore.totalItems > 0" color="red" floating rounded class="cart-badge">
              {{ cartStore.totalItems }}
            </q-badge>
          </q-btn>
        </div>
      </q-toolbar>
    </q-header>

    <!-- Drawer: Mobile Navigation Menu -->
    <q-drawer
      v-model="leftDrawerOpen"
      side="left"
      overlay
      bordered
      :class="$q.dark.isActive ? 'bg-grey-10 text-white' : 'bg-white text-grey-9'"
    >
      <div class="q-pa-md text-center border-bottom q-mb-md">
        <q-avatar size="60px" class="q-mb-sm">
          <q-img :src="settingsStore.getLogoUrl" alt="logo" />
        </q-avatar>
        <div class="text-weight-bold text-subtitle1 text-primary">
          {{ settingsStore.settings?.companyName || 'xPOSx Shop' }}
        </div>
      </div>

      <q-list class="q-px-sm">
        <q-item 
          clickable 
          to="/" 
          exact
          v-close-popup
          active-class="active-drawer-item text-primary"
          class="drawer-nav-item q-my-xs"
        >
          <q-item-section avatar>
            <q-icon name="home" />
          </q-item-section>
          <q-item-section class="text-weight-bold">Inicio</q-item-section>
        </q-item>

        <q-item 
          clickable 
          to="/productos" 
          exact
          v-close-popup
          active-class="active-drawer-item text-primary"
          class="drawer-nav-item q-my-xs"
        >
          <q-item-section avatar>
            <q-icon name="grid_view" />
          </q-item-section>
          <q-item-section class="text-weight-bold">Catálogo</q-item-section>
        </q-item>

        <q-item 
          clickable 
          to="/carrito" 
          exact
          v-close-popup
          active-class="active-drawer-item text-primary"
          class="drawer-nav-item q-my-xs"
        >
          <q-item-section avatar>
            <q-icon name="shopping_cart" />
          </q-item-section>
          <q-item-section class="text-weight-bold">Carrito</q-item-section>
        </q-item>

        <q-item 
          clickable 
          to="/p/terminos-y-condiciones" 
          exact
          v-close-popup
          active-class="active-drawer-item text-primary"
          class="drawer-nav-item q-my-xs"
        >
          <q-item-section avatar>
            <q-icon name="gavel" />
          </q-item-section>
          <q-item-section class="text-weight-bold">Términos y Condiciones</q-item-section>
        </q-item>
      </q-list>
    </q-drawer>

    <!-- Main Page Container -->
    <q-page-container>
      <router-view />
    </q-page-container>

    <!-- Footer for Mobile bottom tab navigation (lt-md) -->
    <q-footer class="lt-md bg-white text-grey-9 shadow-2 border-top" style="z-index: 2000;">
      <q-tabs active-color="primary" class="text-grey-7" dense mobile-arrows>
        <q-route-tab to="/" icon="home" label="Inicio" class="touch-btn-48" />
        <q-route-tab to="/productos" icon="grid_view" label="Catálogo" class="touch-btn-48" />
        <q-route-tab to="/carrito" icon="shopping_cart" label="Carrito" class="touch-btn-48" />
        <q-route-tab to="/p/terminos-y-condiciones" icon="gavel" label="Términos" class="touch-btn-48" />
      </q-tabs>
    </q-footer>

    <!-- Main Footer -->
    <footer class="q-py-xl text-center border-top bg-glass-footer" :class="$q.dark.isActive ? 'bg-grey-10 text-grey-4' : 'bg-grey-2 text-grey-7'">
      <div class="container q-px-md q-mx-auto max-width-lg">
        <div class="row q-col-gutter-lg justify-center text-left">
          <div class="col-12 col-md-4 text-center text-md-left">
            <span class="text-weight-bold text-h6 text-primary">{{ settingsStore.settings?.companyName || 'xPOSx Shop' }}</span>
            <p class="q-mt-sm text-body2">Tu portal de compras rápido, seguro y con pre-venta QR integrada.</p>
          </div>
          <div class="col-6 col-md-3 offset-md-1">
            <span class="text-weight-bold text-subtitle1 text-primary">Enlaces</span>
            <ul class="footer-links q-pl-none q-mt-sm">
              <li><router-link to="/" class="footer-link">Inicio</router-link></li>
              <li><router-link to="/productos" class="footer-link">Catálogo</router-link></li>
              <li><router-link to="/carrito" class="footer-link">Carrito de Compras</router-link></li>
            </ul>
          </div>
          <div class="col-6 col-md-3">
            <span class="text-weight-bold text-subtitle1 text-primary">Legal</span>
            <ul class="footer-links q-pl-none q-mt-sm">
              <li><router-link to="/p/terminos-y-condiciones" class="footer-link">Términos y Condiciones</router-link></li>
            </ul>
          </div>
        </div>
        <div class="q-mt-lg text-caption text-grey-6 border-top q-pt-md">
          © {{ new Date().getFullYear() }} {{ settingsStore.settings?.companyName || 'xPOSx' }}. Todos los derechos reservados.
        </div>
      </div>
    </footer>
  </q-layout>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useQuasar } from 'quasar';
import { useSettingsStore } from '@/stores/settings';
import { useCartStore } from '@/stores/cart';

const $q = useQuasar();
const settingsStore = useSettingsStore();
const cartStore = useCartStore();

const leftDrawerOpen = ref(false);

onMounted(async () => {
  if (!settingsStore.settings) {
    await settingsStore.fetchSettings();
  }
});

const toggleLeftDrawer = () => {
  leftDrawerOpen.value = !leftDrawerOpen.value;
};

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

.header-toolbar {
  height: 64px;
}

.no-decoration {
  text-decoration: none;
}

.nav-link {
  transition: all 0.2s ease;
  font-size: 1rem;
  font-weight: 500;
}

.touch-btn-48 {
  min-height: 48px !important;
  display: flex;
  align-items: center;
}

.cart-badge {
  top: 4px;
  right: 4px;
}

.border-bottom {
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

.body--dark .border-bottom {
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.border-top {
  border-top: 1px solid rgba(0, 0, 0, 0.08);
}

.body--dark .border-top {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.drawer-nav-item {
  border-radius: 8px;
  min-height: 48px;
}

.active-drawer-item {
  background: rgba(var(--q-primary), 0.08);
}

.bg-glass-footer {
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
}

.footer-links {
  list-style: none;
}

.footer-link {
  color: inherit;
  text-decoration: none;
  display: inline-block;
  padding: 6px 0;
  transition: color 0.2s ease;
  min-height: 36px;
  line-height: 24px;
}

.footer-link:hover {
  color: var(--q-primary);
}

.tracking-wide {
  letter-spacing: 0.05em;
}

.max-width-lg {
  max-width: 1200px;
}
</style>
