<template>
    <q-scroll-area class="col">
        <div class="row q-col-gutter-sm">
            <div
                v-for="product in products"
                :key="product.id"
                class="col-6 col-sm-4 col-md-3"
            >
                <q-card
                    class="product-card cursor-pointer"
                    @click="$emit('add-to-cart', product)"
                >
                    <q-img
                        :src="product.image || 'https://cdn.quasar.dev/img/parallax2.jpg'"
                        :ratio="ratio"
                        class="bg-grey-3"
                    >
                        <div
                            class="absolute-bottom text-subtitle2 text-center q-pa-xs"
                            :style="{
                                backgroundColor: 'rgba(var(--color-background-dark-rgb), 0.6)',
                                color: 'var(--color-text-dark)',
                            }"
                        >
                            {{ formatCurrency(product.price) }}
                        </div>
                    </q-img>
                    <q-card-section class="q-pa-sm text-center">
                        <div
                            class="text-caption text-weight-bold ellipsis"
                            :style="{ fontFamily: 'var(--font-family-body)' }"
                        >
                            {{ product.name }}
                        </div>
                        <div
                            class="text-grey-7"
                            style="font-size: 10px"
                            :style="{ fontFamily: 'var(--font-family-body)' }"
                        >
                            Stock: {{ getStock(product.id!) }}
                        </div>
                    </q-card-section>
                </q-card>
            </div>
        </div>
    </q-scroll-area>
</template>

<script setup lang="ts">
import { useCurrency } from "@/composables/useCurrency";
import type { Product } from "@/types";

const { formatCurrency } = useCurrency();

defineProps<{
    products: Product[];
    getStock: (id: number) => number;
    ratio?: number;
}>();

defineEmits(['add-to-cart']);
</script>

<style lang="scss" scoped>
.product-card {
    transition: transform 0.2s cubic-bezier(0.4, 0, 0.2, 1), box-shadow 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    border-radius: 12px;
    overflow: hidden;
    border: 1px solid var(--color-border);
    background-color: var(--color-background-elevated);

    &:hover {
        transform: translateY(-2px) scale(1.01);
        box-shadow: 0 6px 12px rgba(0, 0, 0, 0.1);
        z-index: 1;
    }
    .body--dark & {
        border-color: var(--color-border-dark);
        background-color: var(--color-background-elevated-dark);
        &:hover {
            box-shadow: 0 8px 16px rgba(0, 0, 0, 0.3);
        }
    }
}
</style>
