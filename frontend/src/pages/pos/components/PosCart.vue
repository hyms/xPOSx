<template>
    <div class="column full-height">
        <div v-if="showHeader" class="text-h6 q-mb-md flex items-center" :style="{ fontFamily: 'var(--font-family-display)' }">
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
            <q-select
                v-model="clientId"
                :options="clients"
                option-label="name"
                option-value="id"
                label="Cliente"
                dense
                outlined
                emit-value
                map-options
            />

            <q-select
                v-model="warehouseId"
                :options="warehouses"
                option-label="name"
                option-value="id"
                label="Almacén"
                dense
                outlined
                emit-value
                map-options
                @update:model-value="$emit('fetch-stocks')"
            />
        </div>

        <q-scroll-area class="col q-mb-md">
            <div v-if="cart.length === 0" class="full-height flex flex-center text-grey-6 q-pa-md text-center">
                <div :style="{ fontFamily: 'var(--font-family-body)' }">
                    <q-icon name="add_shopping_cart" size="64px" class="q-mb-sm opacity-50" :style="{ color: 'var(--color-text-primary)', opacity: 0.4 }" />
                    <div class="text-subtitle1" :style="{ color: 'var(--color-text-primary)', opacity: 0.6 }">
                        El carrito está vacío
                    </div>
                </div>
            </div>
            <q-list v-else separator>
                <q-item v-for="(item, index) in cart" :key="item.productId" class="q-px-sm">
                    <q-item-section>
                        <q-item-label class="text-weight-medium" :style="{ fontFamily: 'var(--font-family-body)' }">
                            {{ getProductName(item.productId) }}
                        </q-item-label>
                        <q-item-label caption class="text-tabular-nums" :style="{ fontFamily: 'var(--font-family-body)' }">
                            {{ formatCurrency(item.price) }} x {{ item.quantity }}
                        </q-item-label>
                    </q-item-section>
                    <q-item-section side>
                        <div class="row items-center no-wrap q-gutter-x-xs">
                            <q-btn size="13px" flat round icon="remove" color="primary" @click="$emit('decrement', index)" padding="8px" />
                            <div class="q-mx-sm text-weight-bold text-subtitle1 text-tabular-nums" style="min-width: 24px; text-align: center; fontFamily: 'var(--font-family-body)'">
                                {{ item.quantity }}
                            </div>
                            <q-btn size="13px" flat round icon="add" color="primary" @click="$emit('increment', index)" padding="8px" />
                            <q-btn size="sm" flat round color="negative" icon="close" @click="$emit('remove', index)" />
                        </div>
                    </q-item-section>
                </q-item>
            </q-list>
        </q-scroll-area>

        <q-separator :style="{ backgroundColor: 'var(--color-border)' }" />

        <div class="q-py-md">
            <div class="row justify-between text-subtitle1 q-mb-sm" :style="{ fontFamily: 'var(--font-family-body)' }">
                <span>Subtotal:</span>
                <span class="text-tabular-nums">{{ formatCurrency(subtotal) }}</span>
            </div>

            <div class="row q-col-gutter-sm q-mb-sm">
                <div class="col-4">
                    <q-input v-model.number="taxRate" label="Impuesto %" type="number" dense outlined input-class="text-right" @update:model-value="$emit('calculate-totals')" />
                </div>
                <div class="col-4">
                    <q-input v-model.number="discount" label="Descuento $" type="number" dense outlined input-class="text-right" @update:model-value="$emit('calculate-totals')" />
                </div>
                <div class="col-4">
                    <q-input v-model.number="shipping" label="Envío $" type="number" dense outlined input-class="text-right" @update:model-value="$emit('calculate-totals')" />
                </div>
            </div>

            <q-separator class="q-my-sm" :style="{ backgroundColor: 'var(--color-border)' }" />
            <div class="row justify-between text-h5 text-weight-bolder" :style="{ fontFamily: 'var(--font-family-display)', color: 'var(--color-primary)' }">
                <span>Total:</span>
                <span class="text-tabular-nums">{{ formatCurrency(total) }}</span>
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
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useCurrency } from "@/composables/useCurrency";
import type { Client, Warehouse, SaleDetail } from "@/types";

const { formatCurrency } = useCurrency();

const props = withDefaults(defineProps<{
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
}>(), {
    showHeader: true,
    checkoutLabel: 'PAGAR AHORA',
    checkoutSize: 'lg'
});

const emit = defineEmits([
    'update:clientId', 'update:warehouseId', 'update:taxRate', 'update:discount', 'update:shipping',
    'clear-cart', 'fetch-stocks', 'increment', 'decrement', 'remove', 'calculate-totals', 'checkout'
]);

const clientId = computed({
    get: () => props.formData.clientId,
    set: (val) => emit('update:clientId', val)
});

const warehouseId = computed({
    get: () => props.formData.warehouseId,
    set: (val) => emit('update:warehouseId', val)
});

const taxRate = computed({
    get: () => props.formData.taxRate,
    set: (val) => emit('update:taxRate', val)
});

const discount = computed({
    get: () => props.formData.discount,
    set: (val) => emit('update:discount', val)
});

const shipping = computed({
    get: () => props.formData.shipping,
    set: (val) => emit('update:shipping', val)
});
</script>
