<template>
    <q-page padding>
        <div class="row q-col-gutter-sm">
            <div class="col-12">
                <q-table
                    title="Transferencias"
                    :rows="transfers"
                    :columns="columns"
                    row-key="id"
                    :loading="loading"
                    :filter="filter"
                >
                    <template v-slot:top-right>
                        <BaseSearch
                            @search="filter = $event"
                            class="full-width-xs"
                        />
                        <q-btn
                            color="primary"
                            label="Nueva Transferencia"
                            icon="add"
                            to="/transfers/create"
                            class="q-ml-md full-width-xs mobile-only-mt"
                        />
                    </template>
                    <template v-slot:body-cell-status="props">
                        <q-td :props="props">
                            <q-badge :color="getStatusColor(props.value)">
                                {{ props.value }}
                            </q-badge>
                        </q-td>
                    </template>

                    <template v-slot:body-cell-actions="props">
                        <q-td :props="props">
                            <q-btn
                                flat
                                round
                                color="primary"
                                icon="visibility"
                                @click="viewTransfer(props.row.id)"
                            />
                            <q-btn
                                flat
                                round
                                color="negative"
                                icon="delete"
                                @click="confirmDeleteAction(props.row)"
                            />
                        </q-td>
                    </template>
                </q-table>
        <!-- Transfer Detail Dialog -->
        <TransferDetailDialog
            v-model="detailDialog"
            :transfer-id="selectedTransferId"
            :from-warehouse-name="selectedFromWarehouseName"
            :to-warehouse-name="selectedToWarehouseName"
        />
            </div>
        </div>
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from "vue";
import { useQuasar } from "quasar";
import { transferService } from "@/services/transfer.service";
import { useCurrency } from "@/composables/useCurrency";
import type { TransferReadDto } from "@/types";
import { useConfirm } from "@/composables/useConfirm";
import BaseSearch from "@/components/base/BaseSearch.vue";
import TransferDetailDialog from "./components/TransferDetailDialog.vue";

const $q = useQuasar();
const { confirmDelete } = useConfirm();
const { formatCurrency } = useCurrency();
const transfers = ref<TransferReadDto[]>([]);
const loading = ref(true);
const filter = ref("");

const detailDialog = ref(false);
const selectedTransferId = ref<number | null>(null);
const selectedFromWarehouseName = ref("");
const selectedToWarehouseName = ref("");

const columns = [
    {
        name: "date",
        label: "Fecha",
        field: "date",
        format: (val: string) => new Date(val).toLocaleDateString(),
        sortable: true,
        align: "left" as const,
    },
    {
        name: "ref",
        label: "Referencia",
        field: "ref",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "fromWarehouse",
        label: "Desde",
        field: "fromWarehouseName",
        align: "left" as const,
    },
    {
        name: "toWarehouse",
        label: "Hacia",
        field: "toWarehouseName",
        align: "left" as const,
    },
    { name: "items", label: "Items", field: "items", align: "center" as const },
    {
        name: "grandTotal",
        label: "Total",
        field: "grandTotal",
        format: (val: number) => `$${val.toFixed(2)}`,
        align: "right" as const,
    },
    {
        name: "status",
        label: "Estado",
        field: "status",
        align: "center" as const,
    },
    {
        name: "actions",
        label: "Acciones",
        field: "actions",
        align: "center" as const,
    },
];

const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
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

const fetchTransfers = async (filter?: string) => {
    loading.value = true;
    try {
        const response = await transferService.getAll(filter);
        transfers.value = response.data;
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al cargar transferencias",
        });
    } finally {
        loading.value = false;
    }
};

watch(filter, (newFilter) => {
    fetchTransfers(newFilter);
});

const viewTransfer = (id: number) => {
    const row = transfers.value.find((t) => t.id === id);
    selectedTransferId.value = id;
    selectedFromWarehouseName.value = row?.fromWarehouseName || "";
    selectedToWarehouseName.value = row?.toWarehouseName || "";
    detailDialog.value = true;
};

const confirmDeleteAction = (transfer: TransferReadDto) => {
    confirmDelete(transfer.ref, async () => {
        await transferService.delete(transfer.id);
        fetchTransfers();
    });
};

onMounted(fetchTransfers);
</script>
