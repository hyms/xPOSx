<template>
    <q-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)">
        <q-card style="width: 800px; max-width: 90vw;">
            <q-card-section class="row items-center q-pb-none">
                <div class="text-h6 text-primary font-display text-weight-bold">
                    Detalle de Venta: {{ selectedSale?.ref }}
                </div>
                <q-space />
                <q-btn icon="close" flat round dense v-close-popup />
            </q-card-section>

            <q-card-section v-if="loadingDetail" class="text-center q-pa-lg">
                <q-spinner color="primary" size="3em" />
                <div class="q-mt-md text-grey">Cargando detalles...</div>
            </q-card-section>

            <q-card-section v-else-if="selectedSale" class="q-pa-md">
                <!-- General Info Grid -->
                <div class="row q-col-gutter-md q-mb-md">
                    <div class="col-12 col-sm-6">
                        <div class="text-subtitle2 text-grey">Información General</div>
                        <div><strong>Fecha:</strong> {{ new Date(selectedSale.date).toLocaleString() }}</div>
                        <div><strong>Tipo de Venta:</strong> {{ selectedSale.isPos ? 'POS (Punto de Venta)' : 'Venta Estándar' }}</div>
                        <div class="row items-center q-mt-xs">
                            <strong>Estado:</strong> 
                            <q-badge :color="getStatusColor(selectedSale.status)" class="q-ml-xs">
                                {{ selectedSale.status }}
                            </q-badge>
                        </div>
                        <div class="row items-center q-mt-xs">
                            <strong>Pago:</strong> 
                            <q-badge :color="getPaymentStatusColor(selectedSale.paymentStatus)" class="q-ml-xs">
                                {{ selectedSale.paymentStatus }}
                            </q-badge>
                        </div>
                    </div>
                    <div class="col-12 col-sm-6">
                        <div class="text-subtitle2 text-grey">Asociación</div>
                        <div><strong>Cliente:</strong> {{ clientName || 'N/A' }}</div>
                        <div><strong>Almacén:</strong> {{ warehouseName || 'N/A' }}</div>
                        <div v-if="selectedSale.notes"><strong>Notas:</strong> {{ selectedSale.notes }}</div>
                    </div>
                </div>

                <q-separator class="q-my-md" />

                <!-- Items Table -->
                <div class="text-subtitle2 text-grey q-mb-sm">Ítems de la Venta</div>
                <q-table
                    flat
                    bordered
                    dense
                    :rows="selectedSale.details"
                    :columns="[
                        { name: 'product', label: 'Producto', field: 'productName', align: 'left' },
                        { name: 'price', label: 'Precio', field: 'price', format: (val) => formatCurrency(val), align: 'right' },
                        { name: 'quantity', label: 'Cantidad', field: 'quantity', align: 'center' },
                        { name: 'discount', label: 'Desc.', field: (row) => row.discount ? `${row.discount}${row.discountMethod === 'percentage' ? '%' : ''}` : '0', align: 'right' },
                        { name: 'total', label: 'Total', field: 'total', format: (val) => formatCurrency(val), align: 'right' }
                    ]"
                    row-key="id"
                    hide-bottom
                    :rows-per-page-options="[0]"
                />

                <!-- Financial Summary -->
                <div class="row justify-end q-mt-md">
                    <div class="col-12 col-sm-6 text-right q-gutter-xs">
                        <div class="row justify-between">
                            <span class="text-grey">Impuesto ({{ selectedSale.taxRate || 0 }}%):</span>
                            <span class="text-weight-bold">{{ formatCurrency(selectedSale.taxNet || 0) }}</span>
                        </div>
                        <div class="row justify-between">
                            <span class="text-grey">Descuento General:</span>
                            <span class="text-weight-bold text-negative">-{{ formatCurrency(selectedSale.discount || 0) }}</span>
                        </div>
                        <div class="row justify-between">
                            <span class="text-grey">Envío:</span>
                            <span class="text-weight-bold">{{ formatCurrency(selectedSale.shipping || 0) }}</span>
                        </div>
                        <q-separator class="q-my-xs" />
                        <div class="row justify-between text-subtitle1 text-primary text-weight-bolder">
                            <span>Total General:</span>
                            <span>{{ formatCurrency(selectedSale.grandTotal) }}</span>
                        </div>
                        <div class="row justify-between text-subtitle2 text-positive text-weight-bold">
                            <span>Monto Pagado:</span>
                            <span>{{ formatCurrency(selectedSale.paidAmount) }}</span>
                        </div>
                        <div class="row justify-between text-subtitle2 text-warning text-weight-bold" v-if="selectedSale.grandTotal - selectedSale.paidAmount > 0">
                            <span>Saldo Restante:</span>
                            <span>{{ formatCurrency(selectedSale.grandTotal - selectedSale.paidAmount) }}</span>
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
import { saleService } from "@/services/sale.service";
import type { Sale } from "@/types";
import { useCurrency } from "@/composables/useCurrency";

const props = defineProps<{
    modelValue: boolean;
    saleId: number | null;
    clientName?: string;
    warehouseName?: string;
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
}>();

const $q = useQuasar();
const { formatCurrency } = useCurrency();

const selectedSale = ref<Sale | null>(null);
const loadingDetail = ref(false);

const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
        case "completed":
            return "positive";
        case "pending":
            return "warning";
        case "ordered":
            return "info";
        default:
            return "primary";
    }
};

const getPaymentStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
        case "paid":
            return "positive";
        case "partial":
            return "warning";
        case "unpaid":
            return "negative";
        default:
            return "grey";
    }
};

const loadSaleDetail = async (id: number) => {
    loadingDetail.value = true;
    try {
        const response = await saleService.getById(id);
        selectedSale.value = response.data;
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al cargar el detalle de la venta",
        });
        emit("update:modelValue", false);
    } finally {
        loadingDetail.value = false;
    }
};

watch(
    () => props.saleId,
    (newId) => {
        if (newId && props.modelValue) {
            loadSaleDetail(newId);
        } else {
            selectedSale.value = null;
        }
    }
);

watch(
    () => props.modelValue,
    (isOpen) => {
        if (isOpen && props.saleId) {
            loadSaleDetail(props.saleId);
        }
    }
);
</script>
