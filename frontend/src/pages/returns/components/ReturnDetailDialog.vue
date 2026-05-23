<template>
    <q-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)">
        <q-card style="width: 800px; max-width: 90vw;">
            <q-card-section class="row items-center q-pb-none">
                <div class="text-h6 text-primary font-display text-weight-bold">
                    Detalle de Devolución de {{ returnType === 'sale' ? 'Venta' : 'Compra' }}: {{ selectedReturn?.ref }}
                </div>
                <q-space />
                <q-btn icon="close" flat round dense v-close-popup />
            </q-card-section>

            <q-card-section v-if="loadingDetail" class="text-center q-pa-lg">
                <q-spinner color="primary" size="3em" />
                <div class="q-mt-md text-grey">Cargando detalles...</div>
            </q-card-section>

            <q-card-section v-else-if="selectedReturn" class="q-pa-md">
                <!-- General Info Grid -->
                <div class="row q-col-gutter-md q-mb-md">
                    <div class="col-12 col-sm-6">
                        <div class="text-subtitle2 text-grey">Información General</div>
                        <div><strong>Fecha:</strong> {{ new Date(selectedReturn.date).toLocaleDateString() }}</div>
                        <div class="row items-center q-mt-xs">
                            <strong>Estado:</strong>
                            <q-chip :color="getStatusColor(selectedReturn.status)" text-color="white" dense size="sm" class="q-ml-xs">
                                {{ selectedReturn.status }}
                            </q-chip>
                        </div>
                    </div>
                    <div class="col-12 col-sm-6">
                        <div class="text-subtitle2 text-grey">Asociación</div>
                        <div v-if="returnType === 'sale'"><strong>Cliente:</strong> {{ associateName || 'N/A' }}</div>
                        <div v-else><strong>Proveedor:</strong> {{ associateName || 'N/A' }}</div>
                        <div><strong>Almacén:</strong> {{ warehouseName || 'N/A' }}</div>
                        <div v-if="selectedReturn.notes"><strong>Notas:</strong> {{ selectedReturn.notes }}</div>
                    </div>
                </div>

                <q-separator class="q-my-md" />

                <!-- Items Table -->
                <div class="text-subtitle2 text-grey q-mb-sm">Ítems de la Devolución</div>
                <q-table
                    flat
                    bordered
                    dense
                    :rows="selectedReturn.details"
                    :columns="[
                        { name: 'product', label: 'Producto', field: 'productName', align: 'left' },
                        { name: 'price', label: returnType === 'sale' ? 'Precio' : 'Costo', field: returnType === 'sale' ? 'price' : 'cost', format: (val) => formatCurrency(val), align: 'right' },
                        { name: 'quantity', label: 'Cantidad', field: 'quantity', align: 'center' },
                        { name: 'discount', label: 'Desc.', field: (row) => row.discount ? `${row.discount}${row.discountMethod === 'percentage' ? '%' : ''}` : '0', align: 'right' },
                        { name: 'total', label: 'Total', field: 'total', format: (val) => formatCurrency(val), align: 'right' }
                    ]"
                    row-key="productId"
                    hide-bottom
                    :rows-per-page-options="[0]"
                />

                <!-- Financial Summary -->
                <div class="row justify-end q-mt-md">
                    <div class="col-12 col-sm-6 text-right q-gutter-xs">
                        <div class="row justify-between" v-if="selectedReturn.taxNet">
                            <span class="text-grey">Impuesto ({{ selectedReturn.taxRate || 0 }}%):</span>
                            <span class="text-weight-bold">{{ formatCurrency(selectedReturn.taxNet || 0) }}</span>
                        </div>
                        <div class="row justify-between" v-if="selectedReturn.discount">
                            <span class="text-grey">Descuento General:</span>
                            <span class="text-weight-bold text-negative">-{{ formatCurrency(selectedReturn.discount || 0) }}</span>
                        </div>
                        <div class="row justify-between" v-if="selectedReturn.shipping">
                            <span class="text-grey">Envío:</span>
                            <span class="text-weight-bold">{{ formatCurrency(selectedReturn.shipping || 0) }}</span>
                        </div>
                        <q-separator class="q-my-xs" />
                        <div class="row justify-between text-subtitle1 text-primary text-weight-bolder">
                            <span>Total Devolución:</span>
                            <span>{{ formatCurrency(selectedReturn.grandTotal) }}</span>
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
import { returnService } from "@/services/return.service";
import { useCurrency } from "@/composables/useCurrency";

const props = defineProps<{
    modelValue: boolean;
    returnId: number | null;
    returnType: 'sale' | 'purchase';
    associateName?: string; // Client or Provider name
    warehouseName?: string;
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
}>();

const $q = useQuasar();
const { formatCurrency } = useCurrency();

const selectedReturn = ref<any | null>(null);
const loadingDetail = ref(false);

const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
        case "completed":
            return "positive";
        case "pending":
            return "warning";
        default:
            return "grey";
    }
};

const loadReturnDetail = async (id: number, type: 'sale' | 'purchase') => {
    loadingDetail.value = true;
    try {
        const response = type === 'sale' 
            ? await returnService.getSaleReturnById(id)
            : await returnService.getPurchaseReturnById(id);
        selectedReturn.value = response.data;
    } catch (error) {
        $q.notify({
            color: "negative",
            message: `Error al cargar el detalle de la devolución de ${type === 'sale' ? 'venta' : 'compra'}`,
        });
        emit("update:modelValue", false);
    } finally {
        loadingDetail.value = false;
    }
};

watch(
    () => props.returnId,
    (newId) => {
        if (newId && props.modelValue) {
            loadReturnDetail(newId, props.returnType);
        } else {
            selectedReturn.value = null;
        }
    }
);

watch(
    () => props.modelValue,
    (isOpen) => {
        if (isOpen && props.returnId) {
            loadReturnDetail(props.returnId, props.returnType);
        }
    }
);
</script>
