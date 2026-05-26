<template>
    <q-dialog v-model="internalModel" persistent>
        <q-card style="width: 500px; max-width: 90vw; border-radius: 12px;">
            <q-card-section class="bg-primary text-white">
                <div
                    class="text-h6 text-bold"
                    :style="{ fontFamily: 'var(--font-family-display)' }"
                >
                    Finalizar Venta
                </div>
            </q-card-section>

            <q-card-section class="q-pa-lg">
                <div
                    class="text-h4 text-center q-mb-lg"
                    :style="{
                        fontFamily: 'var(--font-family-display)',
                        color: 'var(--color-primary)',
                    }"
                >
                    Total: {{ formatCurrency(total) }}
                </div>

                <!-- SIN Restriction Warning -->
                <q-slide-transition>
                    <div
                        v-if="isInvoiceBlocked"
                        class="warning-box q-pa-md row no-wrap items-center q-mb-md"
                    >
                        <q-icon
                            name="warning"
                            color="warning"
                            size="md"
                            class="q-mr-sm"
                        />
                        <div
                            class="text-caption text-warning leading-tight text-bold"
                        >
                            RESTRICCIÓN SIN: El SIN exige registrar un cliente
                            con Razón Social y NIT/CI real para ventas
                            superiores a Bs. 1.000.
                        </div>
                    </div>
                </q-slide-transition>

                <div class="row q-col-gutter-sm">
                    <div class="col-12">
                        <q-input
                            v-model="notes"
                            label="Notas de la Venta (Opcional)"
                            type="textarea"
                            autogrow
                            outlined
                            dense
                            class="q-mb-md"
                        />
                    </div>
                    <div class="col-12">
                        <q-input
                            v-model.number="paidAmount"
                            label="Monto Recibido"
                            type="number"
                            outlined
                            dense
                            prefix="$"
                            input-class="text-h6"
                            @update:model-value="$emit('calculate-change')"
                        />
                    </div>

                    <div class="col-12 flex q-gutter-sm justify-center q-mb-md">
                        <q-btn
                            v-for="amt in quickAmounts"
                            :key="amt"
                            :label="formatCurrency(amt)"
                            color="primary"
                            outline
                            @click="$emit('set-quick-amount', amt)"
                        />
                        <q-btn
                            :label="`Exacto (${formatCurrency(total)})`"
                            color="primary"
                            outline
                            @click="$emit('set-quick-amount', total)"
                        />
                    </div>

                    <div class="col-12">
                        <div
                            class="row justify-between text-h6"
                            :style="{ fontFamily: 'var(--font-family-body)' }"
                        >
                            <span>Cambio:</span>
                            <span
                                :class="
                                    change < 0
                                        ? 'text-negative'
                                        : 'text-positive'
                                "
                                >{{ formatCurrency(change) }}</span
                            >
                        </div>
                    </div>
                </div>
            </q-card-section>

            <q-card-actions align="right" class="q-pa-md">
                <q-btn flat label="Regresar" color="primary" v-close-popup />
                <q-btn
                    :label="
                        isInvoiceBlocked
                            ? 'FACTURACIÓN BLOQUEADA'
                            : 'CONFIRMAR VENTA'
                    "
                    color="primary"
                    unelevated
                    @click="$emit('submit')"
                    :loading="saving"
                    :disable="paidAmount < total || isInvoiceBlocked"
                />
            </q-card-actions>
        </q-card>
    </q-dialog>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useCurrency } from "@/composables/useCurrency";

const { formatCurrency } = useCurrency();

const props = defineProps<{
    modelValue: boolean;
    total: number;
    paidAmount: number;
    change: number;
    notes: string;
    quickAmounts: number[];
    saving: boolean;
    isInvoiceBlocked?: boolean;
    sinLimit?: number;
}>();

const emit = defineEmits([
    "update:modelValue",
    "update:paidAmount",
    "update:notes",
    "calculate-change",
    "set-quick-amount",
    "submit",
]);

const internalModel = computed({
    get: () => props.modelValue,
    set: (val) => emit("update:modelValue", val),
});

const paidAmount = computed({
    get: () => props.paidAmount,
    set: (val) => emit("update:paidAmount", val),
});

const notes = computed({
    get: () => props.notes,
    set: (val) => emit("update:notes", val),
});
</script>

<style lang="scss" scoped>
.warning-box {
    background: rgba(255, 183, 0, 0.15);
    border: 1px dashed var(--q-warning, #f5c000);
    border-radius: 8px;
    color: var(--q-warning, #f5c000);
}
.leading-tight {
    line-height: 1.25;
}
</style>
