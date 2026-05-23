<template>
    <q-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)">
        <q-card style="width: 500px; max-width: 90vw;">
            <q-card-section class="row items-center q-pb-none">
                <div class="text-h6 text-primary font-display text-weight-bold">
                    Detalle de Gasto: {{ selectedExpense?.ref }}
                </div>
                <q-space />
                <q-btn icon="close" flat round dense v-close-popup />
            </q-card-section>

            <q-card-section v-if="loadingDetail" class="text-center q-pa-lg">
                <q-spinner color="primary" size="3em" />
                <div class="q-mt-md text-grey">Cargando detalles...</div>
            </q-card-section>

            <q-card-section v-else-if="selectedExpense" class="q-pa-md">
                <div class="q-gutter-sm">
                    <div class="row justify-between">
                        <span class="text-grey">Fecha:</span>
                        <span class="text-weight-bold">{{ new Date(selectedExpense.date).toLocaleDateString() }}</span>
                    </div>
                    <div class="row justify-between">
                        <span class="text-grey">Categoría:</span>
                        <span class="text-weight-bold">{{ categoryName || 'Gasto' }}</span>
                    </div>
                    <div class="row justify-between">
                        <span class="text-grey">Almacén:</span>
                        <span class="text-weight-bold">{{ warehouseName || 'Almacén' }}</span>
                    </div>
                    <div class="row justify-between text-subtitle1 text-negative text-weight-bolder">
                        <span>Monto:</span>
                        <span>{{ formatCurrency(selectedExpense.amount) }}</span>
                    </div>

                    <q-separator class="q-my-md" />

                    <div class="text-subtitle2 q-mb-xs">Detalles / Notas:</div>
                  <q-card flat bordered>
                      <q-card-section>
                      {{ selectedExpense.details || 'Sin detalles adicionales.' }}
                      </q-card-section>
                    </q-card>
                </div>
            </q-card-section>

            <q-card-actions align="right" class="q-pa-md">
                <q-btn label="Cerrar" color="primary" flat v-close-popup />
            </q-card-actions>
        </q-card>
    </q-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import { useQuasar } from "quasar";
import { expenseService } from "@/services/expense.service";
import type { Expense } from "@/types";
import { useCurrency } from "@/composables/useCurrency";

const props = defineProps<{
    modelValue: boolean;
    expenseId: number | null;
    categoryName?: string;
    warehouseName?: string;
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
}>();

const $q = useQuasar();
const { formatCurrency } = useCurrency();

const selectedExpense = ref<Expense | null>(null);
const loadingDetail = ref(false);

const loadExpenseDetail = async (id: number) => {
    loadingDetail.value = true;
    try {
        const response = await expenseService.getById(id);
        selectedExpense.value = response.data;
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al cargar el detalle del gasto",
        });
        emit("update:modelValue", false);
    } finally {
        loadingDetail.value = false;
    }
};

watch(
    () => props.expenseId,
    (newId) => {
        if (newId && props.modelValue) {
            loadExpenseDetail(newId);
        } else {
            selectedExpense.value = null;
        }
    }
);

watch(
    () => props.modelValue,
    (isOpen) => {
        if (isOpen && props.expenseId) {
            loadExpenseDetail(props.expenseId);
        }
    }
);
</script>
