<template>
    <q-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)">
        <q-card style="width: 800px; max-width: 90vw;">
            <q-card-section class="row items-center q-pb-none">
                <div class="text-h6 text-primary font-display text-weight-bold">
                    Detalle de Cotización: {{ selectedQuotation?.ref }}
                </div>
                <q-space />
                <q-btn icon="close" flat round dense v-close-popup />
            </q-card-section>

            <q-card-section v-if="loadingDetail" class="text-center q-pa-lg">
                <q-spinner color="primary" size="3em" />
                <div class="q-mt-md text-grey">Cargando detalles...</div>
            </q-card-section>

            <q-card-section v-else-if="selectedQuotation" class="q-pa-md">
                <!-- General Info Grid -->
                <div class="row q-col-gutter-md q-mb-md">
                    <div class="col-12 col-sm-6">
                        <div class="text-subtitle2 text-grey">Información General</div>
                        <div><strong>Fecha:</strong> {{ new Date(selectedQuotation.date).toLocaleDateString() }}</div>
                        <div class="row items-center q-mt-xs">
                            <strong>Estado:</strong>
                            <q-chip :color="getStatusColor(selectedQuotation.status)" text-color="white" dense size="sm" class="q-ml-xs">
                                {{ selectedQuotation.status }}
                            </q-chip>
                        </div>
                    </div>
                    <div class="col-12 col-sm-6">
                        <div class="text-subtitle2 text-grey">Asociación</div>
                        <div><strong>Cliente:</strong> {{ clientName || 'N/A' }}</div>
                        <div><strong>Almacén:</strong> {{ warehouseName || 'N/A' }}</div>
                        <div v-if="selectedQuotation.notes"><strong>Notas:</strong> {{ selectedQuotation.notes }}</div>
                    </div>
                </div>

                <q-separator class="q-my-md" />

                <!-- Items Table -->
                <div class="text-subtitle2 text-grey q-mb-sm">Ítems de la Cotización</div>
                <q-table
                    flat
                    bordered
                    dense
                    :rows="selectedQuotation.details"
                    :columns="[
                        { name: 'product', label: 'Producto', field: (row) => getProductName(row.productId), align: 'left' },
                        { name: 'price', label: 'Precio', field: 'price', format: (val) => formatCurrency(val), align: 'right' },
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
                        <div class="row justify-between" v-if="selectedQuotation.taxNet">
                            <span class="text-grey">Impuesto ({{ selectedQuotation.taxRate || 0 }}%):</span>
                            <span class="text-weight-bold">{{ formatCurrency(selectedQuotation.taxNet || 0) }}</span>
                        </div>
                        <div class="row justify-between" v-if="selectedQuotation.discount">
                            <span class="text-grey">Descuento General:</span>
                            <span class="text-weight-bold text-negative">-{{ formatCurrency(selectedQuotation.discount || 0) }}</span>
                        </div>
                        <div class="row justify-between" v-if="selectedQuotation.shipping">
                            <span class="text-grey">Envío:</span>
                            <span class="text-weight-bold">{{ formatCurrency(selectedQuotation.shipping || 0) }}</span>
                        </div>
                        <q-separator class="q-my-xs" />
                        <div class="row justify-between text-subtitle1 text-primary text-weight-bolder">
                            <span>Total General:</span>
                            <span>{{ formatCurrency(selectedQuotation.grandTotal) }}</span>
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
import { quotationService } from "@/services/quotation.service";
import { productService } from "@/services/product.service";
import type { Quotation, Product } from "@/types";
import { useCurrency } from "@/composables/useCurrency";

const props = defineProps<{
    modelValue: boolean;
    quotationId: number | null;
    clientName?: string;
    warehouseName?: string;
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
}>();

const $q = useQuasar();
const { formatCurrency } = useCurrency();

const selectedQuotation = ref<Quotation | null>(null);
const loadingDetail = ref(false);
const products = ref<Product[]>([]);

const getProductName = (id: number) => {
    return products.value.find((p) => p.id === id)?.name || `Producto #${id}`;
};

const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
        case "sent":
            return "positive";
        case "pending":
            return "warning";
        default:
            return "grey";
    }
};

const loadQuotationDetail = async (id: number) => {
    loadingDetail.value = true;
    try {
        if (products.value.length === 0) {
            const pRes = await productService.getAll();
            products.value = pRes.data;
        }
        const qRes = await quotationService.getById(id);
        selectedQuotation.value = qRes.data;
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al cargar el detalle de la cotización",
        });
        emit("update:modelValue", false);
    } finally {
        loadingDetail.value = false;
    }
};

watch(
    () => props.quotationId,
    (newId) => {
        if (newId && props.modelValue) {
            loadQuotationDetail(newId);
        } else {
            selectedQuotation.value = null;
        }
    }
);

watch(
    () => props.modelValue,
    (isOpen) => {
        if (isOpen && props.quotationId) {
            loadQuotationDetail(props.quotationId);
        }
    }
);
</script>
