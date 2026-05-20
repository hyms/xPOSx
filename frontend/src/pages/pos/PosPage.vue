<template>
    <q-page class="pos-page" :style="{ backgroundColor: 'var(--color-background)' }">
        <!-- Desktop Layout -->
        <div class="row full-height no-wrap gt-sm">
            <!-- Products Section -->
            <div class="col-grow column q-pa-sm">
                <PosSearch
                    v-model:search="search"
                    v-model:selectedCategory="selectedCategory"
                    :categories="categories"
                    @scan="handleBarcodeScan"
                    @open-scanner="showScanner = true"
                />

                <PosProductGrid
                    :products="filteredProducts"
                    :get-stock="getStock"
                    @add-to-cart="addToCart"
                />
            </div>

            <!-- Cart Section (Desktop) -->
            <div
                class="pos-cart column shadow-2 q-pa-sm"
                :style="{
                    backgroundColor: 'var(--color-background-elevated)',
                    borderLeft: '1px solid var(--color-border)',
                }"
                style="width: 400px"
            >
                <PosCart
                    :cart="cart"
                    :clients="clients"
                    :warehouses="warehouses"
                    :get-product-name="getProductName"
                    :subtotal="subtotal"
                    :total="total"
                    :form-data="formData"
                    :is-searching-client="isSearchingClient"
                    @clear-cart="clearCart"
                    @fetch-stocks="fetchStocks"
                    @increment="incrementQty"
                    @decrement="decrementQty"
                    @remove="removeFromCart"
                    @calculate-totals="calculateTotals"
                    @checkout="openCheckout"
                    @search-client="(nit, cb) => searchClientByNit(nit).then(() => cb(true)).catch(() => cb(false))"
                    @register-client="(data, cb) => quickRegisterClient(data).then(() => cb())"
                    v-model:clientId="formData.clientId"
                    v-model:warehouseId="formData.warehouseId"
                    v-model:taxRate="formData.taxRate"
                    v-model:discount="formData.discount"
                    v-model:shipping="formData.shipping"
                />
            </div>
        </div>

        <!-- Mobile Layout -->
        <div class="lt-md column full-height">
            <!-- Search and Categories Mobile -->
            <div class="q-pa-sm bg-background-elevated shadow-1">
                <PosSearch
                    v-model:search="search"
                    v-model:selectedCategory="selectedCategory"
                    :categories="categories"
                    @scan="handleBarcodeScan"
                    @open-scanner="showScanner = true"
                />
            </div>

            <!-- Products Grid Mobile -->
            <div class="col column q-pa-sm">
                <PosProductGrid
                    :products="filteredProducts"
                    :get-stock="getStock"
                    :ratio="1.2"
                    @add-to-cart="addToCart"
                />
            </div>

            <!-- Cart FAB Mobile -->
            <q-page-sticky position="bottom-right" :offset="[18, 18]">
                <div class="column q-gutter-y-sm items-end">
                    <q-btn fab icon="qr_code_scanner" color="accent" @click="showScanner = true">
                        <q-tooltip>Escanear productos</q-tooltip>
                    </q-btn>
                    <q-btn fab icon="shopping_cart" color="primary" @click="showCartMobile = true">
                        <q-badge color="negative" floating v-if="cart.length > 0">{{ cart.length }}</q-badge>
                    </q-btn>
                </div>
            </q-page-sticky>

            <!-- Cart Dialog Mobile -->
            <q-dialog v-model="showCartMobile" position="bottom" full-width maximized>
                <q-card class="column no-wrap" style="height: 80vh">
                    <div class="col column q-pa-md">
                        <PosCart
                            :cart="cart"
                            :clients="clients"
                            :warehouses="warehouses"
                            :get-product-name="getProductName"
                            :subtotal="subtotal"
                            :total="total"
                            :form-data="formData"
                            :is-searching-client="isSearchingClient"
                            checkout-label="PAGAR"
                            @clear-cart="clearCart"
                            @fetch-stocks="fetchStocks"
                            @increment="incrementQty"
                            @decrement="decrementQty"
                            @remove="removeFromCart"
                            @calculate-totals="calculateTotals"
                            @checkout="openCheckout"
                            @search-client="(nit, cb) => searchClientByNit(nit).then(() => cb(true)).catch(() => cb(false))"
                            @register-client="(data, cb) => quickRegisterClient(data).then(() => cb())"
                            v-model:clientId="formData.clientId"
                            v-model:warehouseId="formData.warehouseId"
                            v-model:taxRate="formData.taxRate"
                            v-model:discount="formData.discount"
                            v-model:shipping="formData.shipping"
                        >
                            <template #header-actions>
                                <q-btn icon="qr_code_scanner" flat round dense color="primary" @click="showScanner = true" class="q-mr-sm" />
                                <q-btn icon="close" flat round dense v-close-popup class="q-mr-sm" />
                            </template>
                        </PosCart>
                    </div>
                </q-card>
            </q-dialog>
        </div>

        <!-- Dialogs -->
        <PosCheckoutDialog
            v-model="showCheckoutDialog"
            :total="total"
            v-model:paidAmount="paidAmount"
            v-model:notes="formData.notes"
            :change="change"
            :quick-amounts="quickAmounts"
            :saving="saving"
            :is-invoice-blocked="isInvoiceBlocked"
            :sin-limit="SIN_LIMIT"
            @calculate-change="calculateChange"
            @set-quick-amount="setQuickAmount"
            @submit="submitSale"
        />

        <PosScanResultDialog
            v-model="showScanResultDialog"
            :product="lastScannedProduct"
            @go-to-checkout="() => {
                showScanResultDialog = false;
                showScanner = false;
                if ($q.screen.lt.md) {
                    showCartMobile = true;
                } else {
                    showCheckoutDialog = true;
                }
            }"
        />

        <BarcodeScannerDialog
            v-model="showScanner"
            @detect="(code) => { search = code; handleBarcodeScan(); }"
            show-continuous-option
            title="Escanear Productos"
        />
    </q-page>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from "vue";
import { useQuasar } from "quasar";
import BarcodeScannerDialog from "@/components/BarcodeScannerDialog.vue";
import { useBarcodeScanner } from "@/composables/useBarcodeScanner";

// Modularized Logic and Components
import { usePosLogic } from "./composables/usePosLogic";
import PosSearch from "./components/PosSearch.vue";
import PosProductGrid from "./components/PosProductGrid.vue";
import PosCart from "./components/PosCart.vue";
import PosCheckoutDialog from "./components/PosCheckoutDialog.vue";
import PosScanResultDialog from "./components/PosScanResultDialog.vue";

const $q = useQuasar();
const {
    search, selectedCategory, categories, warehouses, clients,
    cart, showCheckoutDialog, showCartMobile, showScanner, showScanResultDialog,
    lastScannedProduct, paidAmount, change, saving, formData, quickAmounts,
    subtotal, total, filteredProducts, isSearchingClient, isInvoiceBlocked, SIN_LIMIT,
    calculateTotals, setQuickAmount, handleBarcodeScan, getProductName, getStock,
    fetchData, fetchStocks, addToCart, incrementQty, decrementQty, removeFromCart,
    clearCart, calculateChange, submitSale, openCheckout, searchClientByNit, quickRegisterClient
} = usePosLogic();

// Physical Barcode Scanner Logic
useBarcodeScanner({
    onScan: (code) => {
        search.value = code;
        handleBarcodeScan();
    },
    enabled: () => !showCheckoutDialog.value && !showScanner.value
});

// Keyboard shortcuts
const handleKeydown = (e: KeyboardEvent) => {
    if (e.key === "F2") {
        e.preventDefault();
        const searchInput = document.querySelector(".pos-search-input input") as HTMLInputElement;
        if (searchInput) searchInput.focus();
    } else if (e.key === "F4") {
        e.preventDefault();
        if (cart.value.length > 0 && formData.clientId && formData.warehouseId) {
            openCheckout();
        }
    } else if (e.key === "Escape") {
        if (showCheckoutDialog.value) {
            showCheckoutDialog.value = false;
        } else if (search.value) {
            search.value = "";
        }
    }
};

onMounted(() => {
    fetchData();
    window.addEventListener("keydown", handleKeydown);
});

onUnmounted(() => {
    window.removeEventListener("keydown", handleKeydown);
});
</script>

<style lang="scss" scoped>
.pos-page {
    height: calc(100vh - 50px);
}
.pos-cart {
    border-left: 1px solid var(--color-border);
}
</style>
