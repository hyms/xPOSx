<template>
    <q-dialog v-model="internalModel" persistent position="bottom">
        <q-card style="min-width: 350px" class="bg-primary text-white">
            <q-card-section class="row items-center q-pb-none">
                <div class="text-h6">Producto Añadido</div>
                <q-space />
                <q-icon name="check_circle" size="32px" />
            </q-card-section>

            <q-card-section class="q-pt-md text-center">
                <div class="text-subtitle1 text-weight-bold" v-if="product">
                    {{ product.name }}
                </div>
                <div class="text-h5 q-mt-sm" v-if="product">
                    {{ formatCurrency(product.price) }}
                </div>
            </q-card-section>

            <q-card-actions align="around" class="q-pb-md q-px-md">
                <q-btn 
                    label="Seguir Escaneando" 
                    color="white" 
                    text-color="primary" 
                    unelevated
                    v-close-popup 
                />
                <q-btn 
                    label="Ir a Pagar" 
                    color="accent" 
                    unelevated
                    @click="$emit('go-to-checkout')"
                />
            </q-card-actions>
        </q-card>
    </q-dialog>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useCurrency } from "@/composables/useCurrency";
import type { Product } from "@/types";

const { formatCurrency } = useCurrency();

const props = defineProps<{
    modelValue: boolean;
    product: Product | null;
}>();

const emit = defineEmits(['update:modelValue', 'go-to-checkout']);

const internalModel = computed({
    get: () => props.modelValue,
    set: (val) => emit('update:modelValue', val)
});
</script>
