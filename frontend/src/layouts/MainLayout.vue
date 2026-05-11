<template>
    <q-layout view="lHh Lpr lFf">
        <q-header class="glass-header text-primary">
            <q-toolbar>
                <q-btn
                    flat
                    dense
                    round
                    icon="menu"
                    aria-label="Menu"
                    @click="toggleLeftDrawer"
                />

                <q-toolbar-title class="text-weight-bold" :style="{ fontFamily: 'var(--font-family-display)' }">
                    <q-icon name="point_of_sale" size="md" class="q-mr-sm" />
                    <span class="gt-xs">xPOSx Admin</span>
                </q-toolbar-title>

                <q-space />

                <div class="row items-center no-wrap q-gutter-sm">
                    <!-- Warehouse Selector -->
                    <q-select
                        v-if="warehouseStore.warehouses.length > 0"
                        v-model="activeWarehouseId"
                        :options="warehouseOptions"
                        dense
                        outlined
                        dark
                        emit-value
                        map-options
                        options-dense
                        class="gt-sm"
                        style="min-width: 150px"
                        bg-color="primary"
                        label-color="white"
                    >
                        <template v-slot:prepend>
                            <q-icon name="store" color="white" size="xs" />
                        </template>
                    </q-select>

                    <!-- Fullscreen -->
                    <q-btn
                        flat
                        round
                        dense
                        :icon="
                            $q.fullscreen && $q.fullscreen.isActive
                                ? 'fullscreen_exit'
                                : 'fullscreen'
                        "
                        @click="toggleFullscreen"
                        class="gt-xs"
                    >
                        <q-tooltip>Pantalla Completa</q-tooltip>
                    </q-btn>

                    <!-- Dark Mode -->
                    <q-btn
                        flat
                        round
                        dense
                        :icon="$q.dark.isActive ? 'light_mode' : 'dark_mode'"
                        @click="$q.dark.toggle()"
                    >
                        <q-tooltip>Modo Oscuro/Claro</q-tooltip>
                    </q-btn>

                    <q-separator vertical inset dark class="q-mx-sm gt-xs" />

                    <q-btn flat no-caps>
                        <q-avatar
                            size="32px"
                            class="q-mr-sm bg-primary text-white"
                        >
                            <q-icon name="person" size="20px" />
                        </q-avatar>
                        <div class="text-subtitle2 gt-xs">
                            {{ authStore.username }}
                        </div>
                        <q-icon name="arrow_drop_down" />

                        <q-menu
                            transition-show="jump-down"
                            transition-hide="jump-up"
                        >
                            <q-list style="min-width: 180px">
                                <q-item-label header>Usuario</q-item-label>
                                <q-item clickable v-close-popup to="/profile">
                                    <q-item-section avatar
                                        ><q-icon name="person" color="primary"
                                    /></q-item-section>
                                    <q-item-section>Mi Perfil</q-item-section>
                                </q-item>
                                <q-item
                                    clickable
                                    v-close-popup
                                    to="/settings"
                                    v-if="
                                        authStore.hasPermission('settings_view')
                                    "
                                >
                                    <q-item-section avatar
                                        ><q-icon
                                            name="settings"
                                            color="primary"
                                    /></q-item-section>
                                    <q-item-section
                                        >Configuración</q-item-section
                                    >
                                </q-item>
                                <q-separator />
                                <q-item
                                    clickable
                                    v-close-popup
                                    @click="logout"
                                    class="text-negative"
                                >
                                    <q-item-section avatar
                                        ><q-icon name="logout" color="negative"
                                    /></q-item-section>
                                    <q-item-section
                                        >Cerrar Sesión</q-item-section
                                    >
                                </q-item>
                            </q-list>
                        </q-menu>
                    </q-btn>
                </div>
            </q-toolbar>
        </q-header>

        <q-drawer
            v-model="leftDrawerOpen"
            show-if-above
            bordered
            :mini="miniState"
            @mouseover="miniState = false"
            @mouseout="miniState = true"
            mini-to-overlay
            :width="280"
            :breakpoint="500"
            :class="$q.dark.isActive ? 'bg-dark' : 'bg-grey-1'"
            style="transition: width 0.3s cubic-bezier(0.2, 0.8, 0.2, 1); background-color: var(--color-background-elevated);" 
        >
            <q-scroll-area class="fit">
                <q-list padding>
                    <q-item clickable to="/" exact active-class="active-item">
                        <q-item-section avatar>
                            <q-icon name="dashboard" />
                        </q-item-section>
                        <q-item-section>Dashboard</q-item-section>
                    </q-item>

                    <q-item
                        v-if="authStore.hasPermission('pos_view')"
                        clickable
                        to="/pos"
                        exact
                        active-class="active-item"
                        class="pos-cta-btn"
                    >
                        <q-item-section avatar>
                            <q-icon name="point_of_sale" />
                        </q-item-section>
                        <q-item-section class="text-weight-bold"
                            >VENTA POS</q-item-section
                        >
                    </q-item>

                    <q-separator q-my-sm v-if="!miniState" />

                    <q-expansion-item
                        v-if="hasOperationsPermission"
                        icon="receipt_long"
                        label="Operaciones"
                        :content-inset-level="0.5"
                        default-opened
                        header-class="text-weight-medium"
                    >
                        <q-item
                            v-if="authStore.hasPermission('sales_view')"
                            clickable
                            to="/sales"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="list_alt"
                            /></q-item-section>
                            <q-item-section>Ventas</q-item-section>
                        </q-item>
                        <q-item
                            v-if="authStore.hasPermission('quotations_view')"
                            clickable
                            to="/quotations"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="request_quote"
                            /></q-item-section>
                            <q-item-section>Cotizaciones</q-item-section>
                        </q-item>
                        <q-item
                            v-if="authStore.hasPermission('expenses_view')"
                            clickable
                            to="/expenses"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="payments"
                            /></q-item-section>
                            <q-item-section>Gastos</q-item-section>
                        </q-item>
                        <q-item
                            v-if="authStore.hasPermission('returns_view')"
                            clickable
                            to="/returns"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="assignment_return"
                            /></q-item-section>
                            <q-item-section>Devoluciones</q-item-section>
                        </q-item>
                    </q-expansion-item>

                    <q-expansion-item
                        v-if="hasInventoryPermission"
                        icon="inventory_2"
                        label="Inventario"
                        :content-inset-level="0.5"
                        header-class="text-weight-medium"
                    >
                        <q-item
                            v-if="authStore.hasPermission('products_view')"
                            clickable
                            to="/products"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="inventory"
                            /></q-item-section>
                            <q-item-section>Productos</q-item-section>
                        </q-item>
                        <q-item
                            v-if="authStore.hasPermission('purchases_view')"
                            clickable
                            to="/purchases"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="shopping_cart"
                            /></q-item-section>
                            <q-item-section>Compras</q-item-section>
                        </q-item>
                        <q-item
                            v-if="authStore.hasPermission('transfers_view')"
                            clickable
                            to="/transfers"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="swap_horiz"
                            /></q-item-section>
                            <q-item-section>Transferencias</q-item-section>
                        </q-item>
                        <q-item
                            v-if="authStore.hasPermission('adjustments_view')"
                            clickable
                            to="/adjustments"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="tune"
                            /></q-item-section>
                            <q-item-section>Ajustes de Stock</q-item-section>
                        </q-item>
                        <q-item
                            v-if="authStore.hasPermission('categories_view')"
                            clickable
                            to="/categories"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="category"
                            /></q-item-section>
                            <q-item-section>Categorías</q-item-section>
                        </q-item>
                        <q-item
                            v-if="authStore.hasPermission('units_view')"
                            clickable
                            to="/units"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="straighten"
                            /></q-item-section>
                            <q-item-section>Unidades</q-item-section>
                        </q-item>
                    </q-expansion-item>

                    <q-expansion-item
                        v-if="hasContactsPermission"
                        icon="contacts"
                        label="Contactos"
                        :content-inset-level="0.5"
                        header-class="text-weight-medium"
                    >
                        <q-item
                            v-if="authStore.hasPermission('clients_view')"
                            clickable
                            to="/clients"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="person_add"
                            /></q-item-section>
                            <q-item-section>Clientes</q-item-section>
                        </q-item>
                        <q-item
                            v-if="authStore.hasPermission('providers_view')"
                            clickable
                            to="/providers"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="local_shipping"
                            /></q-item-section>
                            <q-item-section>Proveedores</q-item-section>
                        </q-item>
                    </q-expansion-item>

                    <q-separator q-my-sm />

                    <q-expansion-item
                        v-if="hasAdminPermission"
                        icon="admin_panel_settings"
                        label="Administración"
                        :content-inset-level="0.5"
                        header-class="text-weight-medium"
                    >
                        <q-item
                            v-if="authStore.hasPermission('users_view')"
                            clickable
                            to="/users"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="people"
                            /></q-item-section>
                            <q-item-section>Usuarios</q-item-section>
                        </q-item>
                        <q-item
                            v-if="authStore.hasPermission('roles_view')"
                            clickable
                            to="/roles"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="security"
                            /></q-item-section>
                            <q-item-section>Roles</q-item-section>
                        </q-item>
                        <!-- <q-item v-if="authStore.hasPermission('permissions_view')" clickable to="/permissions" active-class="active-item">
              <q-item-section avatar><q-icon name="key" /></q-item-section>
              <q-item-section>Permisos</q-item-section>
            </q-item> -->
                        <q-item
                            v-if="authStore.hasPermission('warehouses_view')"
                            clickable
                            to="/warehouses"
                            active-class="active-item"
                        >
                            <q-item-section avatar
                                ><q-icon name="store"
                            /></q-item-section>
                            <q-item-section>Almacenes</q-item-section>
                        </q-item>
                    </q-expansion-item>

                    <q-item
                        v-if="authStore.hasPermission('reports_view')"
                        clickable
                        to="/reports"
                        active-class="active-item"
                    >
                        <q-item-section avatar
                            ><q-icon name="bar_chart"
                        /></q-item-section>
                        <q-item-section>Reportes</q-item-section>
                    </q-item>

                    <q-item
                        v-if="authStore.hasPermission('settings_view')"
                        clickable
                        to="/settings"
                        active-class="active-item"
                    >
                        <q-item-section avatar
                            ><q-icon name="settings"
                        /></q-item-section>
                        <q-item-section>Configuración</q-item-section>
                    </q-item>
                </q-list>
            </q-scroll-area>

            <div class="absolute-bottom q-ma-sm flex justify-center gt-xs">
                <q-btn
                    flat
                    dense
                    round
                    :icon="miniState ? 'chevron_right' : 'chevron_left'"
                    @click="miniState = !miniState"
                />
            </div>
        </q-drawer>

        <q-page-container
            :class="$q.dark.isActive ? 'bg-grey-10' : 'bg-grey-2'"
        >
            <router-view />
        </q-page-container>
    </q-layout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useQuasar, AppFullscreen } from "quasar";
import { useAuthStore } from "@/stores/auth";
import { useWarehouseStore } from "@/stores/warehouse";
import type { Warehouse } from "@/types";

import { useSettings } from "@/services/settings.service";

const $q = useQuasar();
const router = useRouter();
const authStore = useAuthStore();
const warehouseStore = useWarehouseStore();
const settingsStore = useSettings();

const leftDrawerOpen = ref(false);
const miniState = ref(false);

const activeWarehouseId = computed({
    get: () => warehouseStore.activeWarehouseId,
    set: (val) => val && warehouseStore.setActiveWarehouse(val),
});

const warehouseOptions = computed(() => {
    return warehouseStore.warehouses.map((w: Warehouse) => ({
        label: w.name,
        value: w.id,
    }));
});

const hasOperationsPermission = computed(() => {
    return [
        "sales_view",
        "quotations_view",
        "expenses_view",
        "returns_view",
    ].some((p) => authStore.hasPermission(p));
});

const hasInventoryPermission = computed(() => {
    return [
        "products_view",
        "purchases_view",
        "transfers_view",
        "adjustments_view",
        "categories_view",
        "units_view",
    ].some((p) => authStore.hasPermission(p));
});

const hasContactsPermission = computed(() => {
    return ["clients_view", "providers_view"].some((p) =>
        authStore.hasPermission(p),
    );
});

const hasAdminPermission = computed(() => {
    return [
        "users_view",
        "roles_view",
        "permissions_view",
        "warehouses_view",
    ].some((p) => authStore.hasPermission(p));
});

function toggleLeftDrawer() {
    leftDrawerOpen.value = !leftDrawerOpen.value;
}

function toggleFullscreen() {
    if (!AppFullscreen.isCapable) {
        $q.notify({
            color: "negative",
            message: "Tu navegador no soporta pantalla completa",
            icon: "warning",
        });
        return;
    }

    AppFullscreen.toggle().catch((err) => {
        console.error("Error toggling fullscreen:", err);
    });
}

function logout() {
    authStore.logout();
    router.push("/login");
}

onMounted(() => {
    warehouseStore.fetchWarehouses();
    settingsStore.fetchSettings();
});
</script>

<style lang="scss">
.pos-cta-btn {
    background: var(--color-primary);
    color: var(--color-text-dark) !important;
    box-shadow: 0 6px 16px 0 rgba(var(--color-primary-rgb), 0.45);
    border-radius: 50px !important;
    margin: 12px !important;
    transition: all 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);

    .q-icon,
    .q-item__section--main {
        color: var(--color-text-dark) !important;
    }

    &:hover {
        box-shadow: 0 8px 24px rgba(var(--color-primary-rgb), 0.35);
        transform: translateY(-2px) scale(1.01);
    }
}

.active-item {
    color: var(--color-primary) !important;
    background: rgba(var(--color-primary-rgb), 0.1);
    font-weight: 700;
    transition: all 0.2s ease-in-out;

    .q-item__section--main {
        color: var(--color-primary);
    }

    .q-icon {
        color: var(--color-primary);
    }

    .body--dark & {
        color: var(--color-text-dark) !important;
        background: rgba(255, 255, 255, 0.1);

        .q-item__section--main {
            color: var(--color-text-dark);
        }

        .q-icon {
            color: var(--color-text-dark);
        }
    }
}

.q-drawer {
    .q-item {
        border-radius: 8px; /* Slightly less rounded pills */
        margin: 6px 10px; /* Adjusted margin */
        transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
        color: var(--color-text-light); /* Light mode text color */
        font-weight: 500;

        &:hover {
            background: rgba(var(--color-primary-rgb), 0.08);
            transform: translateX(3px); /* Subtle micro-interaction */
            color: var(--color-primary); /* Primary color on hover */
        }
    }

    /* Exclude expansion items from the global pill styling so children align nicely */
    .q-expansion-item__container > .q-item {
        margin: 4px 12px;
    }

    .body--dark & {
        .q-item {
            color: var(--color-text-dark); /* Dark mode text color */
        }
        .q-item:hover {
            background: rgba(var(--color-primary-rgb), 0.15);
            color: var(--color-primary);
        }
        .q-expansion-item {
            color: var(--color-text-dark);
        }
    }
}

.rounded-borders {
    border-radius: 8px;
}

// Global dark mode adjustments
.body--dark {
    background: var(--color-background) !important;
    color: var(--color-text-dark) !important;

    .q-layout,
    .q-header,
    .q-footer {
        background: var(--color-background) !important;
    }
}
</style>
