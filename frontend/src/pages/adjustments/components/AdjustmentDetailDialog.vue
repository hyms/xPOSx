<template>
    <q-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)">
        <q-card style="width: 700px; max-width: 90vw;">
            <q-card-section class="row items-center q-pb-none">
                <div class="text-h6 text-primary font-display text-weight-bold">
                    Detalle de Ajuste de Inventario: {{ selectedAdjustment?.ref }}
                </div>
                <q-space />
                <q-btn icon="close" flat round dense v-close-popup />
            </q-card-section>

            <q-card-section v-if="loadingDetail" class="text-center q-pa-lg">
                <q-spinner color="primary" size="3em" />
                <div class="q-mt-md text-grey">Cargando detalles...</div>
            </q-card-section>

            <q-card-section v-else-if="selectedAdjustment" class="q-pa-md">
                <!-- General Info Grid -->
                <div class="row q-col-gutter-md q-mb-md">
                    <div class="col-12 col-sm-6">
                        <div class="text-subtitle2 text-grey">Información General</div>
                        <div><strong>Fecha:</strong> {{ new Date(selectedAdjustment.date).toLocaleDateString() }}</div>
                        <div><strong>Almacén:</strong> {{ warehouseName || 'Almacén' }}</div>
                    </div>
                    <div class="col-12 col-sm-6">
                        <div class="text-subtitle2 text-grey">Detalles de Auditoría</div>
                        <div><strong>Cantidad de Ítems:</strong> {{ selectedAdjustment.items }}</div>
                        <div v-if="selectedAdjustment.notes"><strong>Notas / Observaciones:</strong> {{ selectedAdjustment.notes }}</div>
                    </div>
                </div>

                <q-separator class="q-my-md" />

                <!-- Items Table -->
                <div class="text-subtitle2 text-grey q-mb-sm">Ítems Ajustados</div>
                <q-table
                    flat
                    bordered
                    dense
                    :rows="selectedAdjustment.details"
                    :columns="[
                        { name: 'product', label: 'Producto', field: (row) => getProductName(row.productId), align: 'left' },
                        { name: 'type', label: 'Tipo de Ajuste', field: 'type', align: 'center' },
                        { name: 'quantity', label: 'Cantidad', field: 'quantity', align: 'center' }
                    ]"
                    row-key="productId"
                    hide-bottom
                    :rows-per-page-options="[0]"
                >
                    <template v-slot:body-cell-type="props">
                        <q-td :props="props">
                            <q-chip :color="props.row.type === 'add' ? 'positive' : 'negative'" text-color="white" dense size="sm">
                                {{ props.row.type === 'add' ? 'Suma (+)' : 'Resta (-)' }}
                            </q-chip>
                        </q-td>
                    </template>
                </q-table>
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
import { adjustmentService } from "@/services/adjustment.service";
import { productService } from "@/services/product.service";
import type { Adjustment, Product } from "@/types";

const props = defineProps<{
    modelValue: boolean;
    adjustmentId: number | null;
    warehouseName?: string;
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
}>();

const $q = useQuasar();

const selectedAdjustment = ref<Adjustment | null>(null);
const loadingDetail = ref(false);
const products = ref<Product[]>([]);

const getProductName = (id: number) => {
    return products.value.find((p) => p.id === id)?.name || `Producto #${id}`;
};

const loadAdjustmentDetail = async (id: number) => {
    loadingDetail.value = true;
    try {
        if (products.value.length === 0) {
            const pRes = await productService.getAll();
            products.value = pRes.data;
        }
        const aRes = await adjustmentService.getById(id);
        selectedAdjustment.value = aRes.data;
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al cargar el detalle del ajuste",
        });
        emit("update:modelValue", false);
    } finally {
        loadingDetail.value = false;
    }
};

watch(
    () => props.adjustmentId,
    (newId) => {
        if (newId && props.modelValue) {
            loadAdjustmentDetail(newId);
        } else {
            selectedAdjustment.value = null;
        }
    }
);

watch(
    () => props.modelValue,
    (isOpen) => {
        if (isOpen && props.adjustmentId) {
            loadAdjustmentDetail(props.adjustmentId);
        }
    }
);
</script>
