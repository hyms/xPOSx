<template>
    <div class="column full-height">
        <div
            v-if="showHeader"
            class="text-h6 q-mb-md flex items-center"
            :style="{ fontFamily: 'var(--font-family-display)' }"
        >
            <q-icon name="shopping_cart" color="primary" class="q-mr-sm" />
            Carrito
            <q-space />
            <slot name="header-actions"></slot>
            <q-btn
                flat
                round
                icon="delete_sweep"
                color="negative"
                @click="$emit('clear-cart')"
                aria-label="Vaciar carrito"
                class="q-btn--flat"
            >
                <q-tooltip>Vaciar carrito</q-tooltip>
            </q-btn>
        </div>

        <div class="q-gutter-y-sm q-mb-md">
            <div class="row q-col-gutter-xs">
                <div class="col-5">
                    <q-input
                        v-model="formData.clientNit"
                        label="NIT / CI"
                        :dense="!$q.screen.lt.md"
                        outlined
                        stack-label
                        :loading="isSearchingClient"
                        @keyup.enter="onNitSearch"
                        @blur="onNitSearch"
                        class="touch-input-48"
                    />
                </div>
                <div class="col-7">
                    <q-input
                        v-model="formData.clientName"
                        label="Razón Social"
                        :dense="!$q.screen.lt.md"
                        outlined
                        stack-label
                        readonly
                        class="touch-input-48"
                        input-class="text-weight-bold"
                    />
                </div>
            </div>

            <q-select
                v-model="warehouseId"
                :options="warehouses"
                option-label="name"
                option-value="id"
                label="Almacén"
                :dense="!$q.screen.lt.md"
                outlined
                emit-value
                map-options
                stack-label
                @update:model-value="$emit('fetch-stocks')"
                class="touch-input-48"
            />
        </div>

        <q-scroll-area class="col q-mb-md">
            <div
                v-if="cart.length === 0"
                class="full-height flex flex-center text-grey-6 q-pa-md text-center"
            >
                <div :style="{ fontFamily: 'var(--font-family-body)' }">
                    <q-icon
                        name="add_shopping_cart"
                        size="64px"
                        class="q-mb-sm opacity-50"
                        :style="{
                            color: 'var(--color-text-primary)',
                            opacity: 0.4,
                        }"
                    />
                    <div
                        class="text-subtitle1"
                        :style="{
                            color: 'var(--color-text-primary)',
                            opacity: 0.6,
                        }"
                    >
                        El carrito está vacío
                    </div>
                </div>
            </div>
            <q-list v-else separator>
                <q-item
                    v-for="(item, index) in cart"
                    :key="item.productId"
                    class="q-px-sm"
                >
                    <q-item-section>
                        <q-item-label
                            class="text-weight-medium"
                            :style="{ fontFamily: 'var(--font-family-body)' }"
                        >
                            {{ getProductName(item.productId) }}
                        </q-item-label>
                        <q-item-label
                            caption
                            class="text-tabular-nums"
                            :style="{ fontFamily: 'var(--font-family-body)' }"
                        >
                            {{ formatCurrency(item.price) }} x
                            {{ item.quantity }}
                        </q-item-label>
                    </q-item-section>
                    <q-item-section side>
                        <div class="row items-center no-wrap q-gutter-x-xs">
                            <q-btn
                                size="13px"
                                flat
                                round
                                icon="remove"
                                color="primary"
                                @click="$emit('decrement', index)"
                                padding="8px"
                            />
                            <div
                                class="q-mx-sm text-weight-bold text-subtitle1 text-tabular-nums"
                                style="
                                    min-width: 24px;
                                    text-align: center;
                                    fontfamily: &quot;var(--font-family-body)&quot;;
                                "
                            >
                                {{ item.quantity }}
                            </div>
                            <q-btn
                                size="13px"
                                flat
                                round
                                icon="add"
                                color="primary"
                                @click="$emit('increment', index)"
                                padding="8px"
                            />
                            <q-btn
                                size="sm"
                                flat
                                round
                                color="negative"
                                icon="close"
                                @click="$emit('remove', index)"
                            />
                        </div>
                    </q-item-section>
                </q-item>
            </q-list>
        </q-scroll-area>

        <q-separator :style="{ backgroundColor: 'var(--color-border)' }" />

        <div class="q-py-md">
            <div
                class="row justify-between text-subtitle1 q-mb-sm"
                :style="{ fontFamily: 'var(--font-family-body)' }"
            >
                <span>Subtotal:</span>
                <span class="text-tabular-nums">{{
                    formatCurrency(subtotal)
                }}</span>
            </div>

            <div class="row q-col-gutter-sm q-mb-sm">
                <div class="col-4">
                    <q-input
                        v-model.number="taxRate"
                        label="Impuesto %"
                        type="number"
                        :dense="!$q.screen.lt.md"
                        outlined
                        stack-label
                        input-class="text-right"
                        @update:model-value="$emit('calculate-totals')"
                        class="touch-input-48"
                    />
                </div>
                <div class="col-4">
                    <q-input
                        v-model.number="discount"
                        label="Descuento $"
                        type="number"
                        :dense="!$q.screen.lt.md"
                        outlined
                        stack-label
                        input-class="text-right"
                        @update:model-value="$emit('calculate-totals')"
                        class="touch-input-48"
                    />
                </div>
                <div class="col-4">
                    <q-input
                        v-model.number="shipping"
                        label="Envío $"
                        type="number"
                        :dense="!$q.screen.lt.md"
                        outlined
                        stack-label
                        input-class="text-right"
                        @update:model-value="$emit('calculate-totals')"
                        class="touch-input-48"
                    />
                </div>
            </div>

            <q-separator
                class="q-my-sm"
                :style="{ backgroundColor: 'var(--color-border)' }"
            />
            <div
                class="row justify-between text-h5 text-weight-bolder"
                :style="{
                    fontFamily: 'var(--font-family-display)',
                    color: 'var(--color-primary)',
                }"
            >
                <span>Total:</span>
                <span class="text-tabular-nums">{{
                    formatCurrency(total)
                }}</span>
            </div>
        </div>

        <q-btn
            :label="checkoutLabel"
            color="primary"
            class="full-width text-weight-bold"
            :size="checkoutSize"
            unelevated
            @click="$emit('checkout')"
            :disable="cart.length === 0 || !clientId || !warehouseId"
        />

        <!-- Quick Register Client Dialog -->
        <q-dialog
            v-model="showRegisterDialog"
            persistent
        >
            <q-card style="width: 450px; max-width: 90vw; border-radius: 12px;">
                <q-card-section class="bg-primary text-white">
                    <div class="text-h6 text-bold">Nuevo Cliente</div>
                </q-card-section>

                <q-card-section class="q-gutter-md">
                    <p>
                        El NIT <strong>{{ quickClientForm.nitCi }}</strong> no
                        está registrado.
                    </p>
                    <q-input
                        v-model="quickClientForm.name"
                        ref="newNameInput"
                        label="Nombre / Razón Social"
                        outlined
                        autofocus
                        @keyup.enter="onSaveQuickClient"
                    />
                    <q-input
                        v-model="quickClientForm.email"
                        label="Email (Opcional)"
                        outlined
                    />
                </q-card-section>

                <q-card-actions align="right">
                    <q-btn flat label="Cancelar" v-close-popup />
                    <q-btn
                        label="Registrar"
                        color="primary"
                        @click="onSaveQuickClient"
                        :disable="!quickClientForm.name"
                    />
                </q-card-actions>
            </q-card>
        </q-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, nextTick } from "vue";
import { useCurrency } from "@/composables/useCurrency";
import type { Client, Warehouse, SaleDetail } from "@/types";

const { formatCurrency } = useCurrency();

const props = withDefaults(
    defineProps<{
        cart: SaleDetail[];
        clients: Client[];
        warehouses: Warehouse[];
        getProductName: (id: number) => string;
        subtotal: number;
        total: number;
        formData: any;
        showHeader?: boolean;
        checkoutLabel?: string;
        checkoutSize?: string;
        isSearchingClient?: boolean;
    }>(),
    {
        showHeader: true,
        checkoutLabel: "PAGAR AHORA",
        checkoutSize: "lg",
        isSearchingClient: false,
    },
);

const emit = defineEmits([
    "update:clientId",
    "update:warehouseId",
    "update:taxRate",
    "update:discount",
    "update:shipping",
    "clear-cart",
    "fetch-stocks",
    "increment",
    "decrement",
    "remove",
    "calculate-totals",
    "checkout",
    "search-client",
    "register-client",
]);

const showRegisterDialog = ref(false);
const newNameInput = ref<any>(null);
const quickClientForm = ref({
    nitCi: "",
    name: "",
    email: "",
});

const onNitSearch = () => {
    const nit = props.formData.clientNit?.trim();
    if (!nit || nit === "0") {
        emit(
            "update:clientId",
            props.clients.find((c) => c.nitCi === "0")?.id || 0,
        );
        props.formData.clientName = "Sin nombre";
        return;
    }
    emit("search-client", nit, (found: boolean) => {
        if (!found) {
            quickClientForm.value = { nitCi: nit, name: "", email: "" };
            showRegisterDialog.value = true;
            nextTick(() => {
                setTimeout(() => newNameInput.value?.focus(), 100);
            });
        }
    });
};

const onSaveQuickClient = () => {
    if (!quickClientForm.value.name) return;
    emit("register-client", quickClientForm.value, () => {
        showRegisterDialog.value = false;
    });
};

const clientId = computed({
    get: () => props.formData.clientId,
    set: (val) => emit("update:clientId", val),
});

const warehouseId = computed({
    get: () => props.formData.warehouseId,
    set: (val) => emit("update:warehouseId", val),
});

const taxRate = computed({
    get: () => props.formData.taxRate,
    set: (val) => emit("update:taxRate", val),
});

const discount = computed({
    get: () => props.formData.discount,
    set: (val) => emit("update:discount", val),
});

const shipping = computed({
    get: () => props.formData.shipping,
    set: (val) => emit("update:shipping", val),
});
</script>

<style lang="scss" scoped>
@media (max-width: 1023px) {
    .touch-input-48 :deep(.q-field__control) {
        height: 48px !important;
        min-height: 48px !important;
    }
}

.body--dark .touch-input-48 :deep(.q-field__control) {
    background: rgba(255, 255, 255, 0.05);
}
</style>
