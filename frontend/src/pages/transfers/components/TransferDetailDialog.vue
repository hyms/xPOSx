<template>
    <q-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)">
        <q-card style="width: 800px; max-width: 90vw;">
            <q-card-section class="row items-center q-pb-none">
                <div class="text-h6 text-primary font-display text-weight-bold">
                    Detalle de Transferencia: {{ selectedTransfer?.ref }}
                </div>
                <q-space />
                <q-btn icon="close" flat round dense v-close-popup />
            </q-card-section>

            <q-card-section v-if="loadingDetail" class="text-center q-pa-lg">
                <q-spinner color="primary" size="3em" />
                <div class="q-mt-md text-grey">Cargando detalles...</div>
            </q-card-section>

            <q-card-section v-else-if="selectedTransfer" class="q-pa-md">
                <!-- General Info Grid -->
                <div class="row q-col-gutter-md q-mb-md">
                    <div class="col-12 col-sm-6">
                        <div class="text-subtitle2 text-grey">Información General</div>
                        <div><strong>Fecha:</strong> {{ new Date(selectedTransfer.date).toLocaleDateString() }}</div>
                        <div class="row items-center q-mt-xs">
                            <strong>Estado:</strong>
                            <q-badge :color="getStatusColor(selectedTransfer.status)" class="q-ml-xs">
                                {{ selectedTransfer.status }}
                            </q-badge>
                        </div>
                    </div>
                    <div class="col-12 col-sm-6">
                        <div class="text-subtitle2 text-grey">Almacenes</div>
                        <div><strong>Origen (Desde):</strong> {{ fromWarehouseName || 'Almacén Origen' }}</div>
                        <div><strong>Destino (Hacia):</strong> {{ toWarehouseName || 'Almacén Destino' }}</div>
                        <div v-if="selectedTransfer.notes"><strong>Notas:</strong> {{ selectedTransfer.notes }}</div>
                    </div>
                </div>

                <q-separator class="q-my-md" />

                <!-- Items Table -->
                <div class="text-subtitle2 text-grey q-mb-sm">Ítems de la Transferencia</div>
                <q-table
                    flat
                    bordered
                    dense
                    :rows="selectedTransfer.details"
                    :columns="[
                        { name: 'product', label: 'Producto', field: (row) => getProductName(row.productId), align: 'left' },
                        { name: 'cost', label: 'Costo', field: 'cost', format: (val) => formatCurrency(val), align: 'right' },
                        { name: 'quantity', label: 'Cantidad', field: 'quantity', align: 'center' },
                        { name: 'total', label: 'Total', field: 'total', format: (val) => formatCurrency(val), align: 'right' }
                    ]"
                    row-key="productId"
                    hide-bottom
                    :rows-per-page-options="[0]"
                />

                <!-- Financial Summary -->
                <div class="row justify-end q-mt-md">
                    <div class="col-12 col-sm-6 text-right q-gutter-xs">
                        <div class="row justify-between" v-if="selectedTransfer.taxNet">
                            <span class="text-grey">Impuesto ({{ selectedTransfer.taxRate || 0 }}%):</span>
                            <span class="text-weight-bold">{{ formatCurrency(selectedTransfer.taxNet || 0) }}</span>
                        </div>
                        <div class="row justify-between" v-if="selectedTransfer.discount">
                            <span class="text-grey">Descuento General:</span>
                            <span class="text-weight-bold text-negative">-{{ formatCurrency(selectedTransfer.discount || 0) }}</span>
                        </div>
                        <div class="row justify-between" v-if="selectedTransfer.shipping">
                            <span class="text-grey">Envío:</span>
                            <span class="text-weight-bold">{{ formatCurrency(selectedTransfer.shipping || 0) }}</span>
                        </div>
                        <q-separator class="q-my-xs" />
                        <div class="row justify-between text-subtitle1 text-primary text-weight-bolder">
                            <span>Total Transferencia:</span>
                            <span>{{ formatCurrency(selectedTransfer.grandTotal) }}</span>
                        </div>
                    </div>
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
import { transferService } from "@/services/transfer.service";
import { productService } from "@/services/product.service";
import type { Transfer, Product } from "@/types";
import { useCurrency } from "@/composables/useCurrency";

const props = defineProps<{
    modelValue: boolean;
    transferId: number | null;
    fromWarehouseName?: string;
    toWarehouseName?: string;
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
}>();

const $q = useQuasar();
const { formatCurrency } = useCurrency();

const selectedTransfer = ref<Transfer | null>(null);
const loadingDetail = ref(false);
const products = ref<Product[]>([]);

const getProductName = (id: number) => {
    return products.value.find((p) => p.id === id)?.name || `Producto #${id}`;
};

const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
        case "completed":
            return "positive";
        case "sent":
            return "warning";
        case "pending":
            return "grey";
        default:
            return "primary";
    }
};

const loadTransferDetail = async (id: number) => {
    loadingDetail.value = true;
    try {
        if (products.value.length === 0) {
            const pRes = await productService.getAll();
            products.value = pRes.data;
        }
        const tRes = await transferService.getById(id);
        selectedTransfer.value = tRes.data;
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al cargar el detalle de la transferencia",
        });
        emit("update:modelValue", false);
    } finally {
        loadingDetail.value = false;
    }
};

watch(
    () => props.transferId,
    (newId) => {
        if (newId && props.modelValue) {
            loadTransferDetail(newId);
        } else {
            selectedTransfer.value = null;
        }
    }
);

watch(
    () => props.modelValue,
    (isOpen) => {
        if (isOpen && props.transferId) {
            loadTransferDetail(props.transferId);
        }
    }
);
</script>
