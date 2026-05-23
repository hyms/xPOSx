<template>
    <q-page padding>
        <div class="row q-col-gutter-sm">
            <div class="col-12">
                <q-table
                    title="Cajas (Puntos de Venta)"
                    :rows="registers"
                    :columns="columns"
                    row-key="id"
                    :loading="loading"
                    :filter="filter"
                    card-class="glass-card"
                >
                    <template v-slot:top-right>
                        <BaseSearch @search="filter = $event" />
                        <q-btn
                            color="primary"
                            label="Nueva Caja"
                            icon="add"
                            @click="openDialog()"
                            class="q-ml-md text-weight-bold"
                            unelevated
                        />
                    </template>

                    <template v-slot:body-cell-warehouseId="props">
                        <q-td :props="props">
                            <span class="text-weight-medium">
                                {{ getWarehouseName(props.row.warehouseId) }}
                            </span>
                        </q-td>
                    </template>

                    <template v-slot:body-cell-isActive="props">
                        <q-td :props="props" align="center">
                            <q-chip
                                :color="props.row.isActive ? 'positive' : 'negative'"
                                text-color="white"
                                dense
                                square
                                class="text-weight-bold text-uppercase"
                            >
                                {{ props.row.isActive ? 'Activo' : 'Inactivo' }}
                            </q-chip>
                        </q-td>
                    </template>

                    <template v-slot:body-cell-isMatriz="props">
                        <q-td :props="props" align="center">
                            <q-chip
                                v-if="props.row.isMatriz"
                                color="primary"
                                text-color="white"
                                dense
                                square
                                icon="star"
                                class="text-weight-bold text-uppercase"
                            >
                                Principal
                            </q-chip>
                            <q-chip
                                v-else
                                color="grey-4"
                                text-color="grey-8"
                                dense
                                square
                                class="text-weight-bold text-uppercase"
                            >
                                Secundaria
                            </q-chip>
                        </q-td>
                    </template>

                    <template v-slot:body-cell-actions="props">
                        <q-td :props="props" align="center">
                            <q-btn
                                flat
                                round
                                color="primary"
                                icon="edit"
                                @click="openDialog(props.row)"
                            >
                                <q-tooltip>Editar Caja</q-tooltip>
                            </q-btn>
                            <q-btn
                                flat
                                round
                                color="negative"
                                icon="delete"
                                @click="confirmDeleteAction(props.row)"
                            >
                                <q-tooltip>Eliminar Caja</q-tooltip>
                            </q-btn>
                        </q-td>
                    </template>
                </q-table>
            </div>
        </div>

        <!-- Cash Register Dialog -->
        <CashRegisterFormDialog
            v-model="showDialog"
            :register-id="selectedRegisterId"
            :initial-data="selectedRegisterData"
            @saved="fetchRegisters"
        />
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { cashShiftService, type CashRegister } from "@/services/cashShift.service";
import { useWarehouseStore } from "@/stores/warehouse";
import { useConfirm } from "@/composables/useConfirm";
import BaseSearch from "@/components/base/BaseSearch.vue";
import CashRegisterFormDialog from "./components/CashRegisterFormDialog.vue";

const { confirmDelete } = useConfirm();
const warehouseStore = useWarehouseStore();

const registers = ref<CashRegister[]>([]);
const loading = ref(true);
const filter = ref("");
const showDialog = ref(false);
const selectedRegisterId = ref<number | undefined>(undefined);
const selectedRegisterData = ref<CashRegister | undefined>(undefined);

const columns = [
    {
        name: "id",
        label: "ID",
        field: "id",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "name",
        label: "Nombre de Caja",
        field: "name",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "warehouseId",
        label: "Almacén",
        field: "warehouseId",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "isActive",
        label: "Estado",
        field: "isActive",
        sortable: true,
        align: "center" as const,
    },
    {
        name: "isMatriz",
        label: "Tipo",
        field: "isMatriz",
        sortable: true,
        align: "center" as const,
    },
    {
        name: "actions",
        label: "Acciones",
        field: "actions",
        align: "center" as const,
    },
];

const getWarehouseName = (id: number) => {
    const wh = warehouseStore.warehouses.find((w) => w.id === id);
    return wh ? wh.name : `Almacén #${id}`;
};

const fetchRegisters = async () => {
    loading.value = true;
    try {
        const response = await cashShiftService.getAllRegisters();
        registers.value = response.data;
    } catch (error) {
        // Handled globally
    } finally {
        loading.value = false;
    }
};

const openDialog = (register?: CashRegister) => {
    if (register) {
        selectedRegisterId.value = register.id;
        selectedRegisterData.value = register;
    } else {
        selectedRegisterId.value = undefined;
        selectedRegisterData.value = undefined;
    }
    showDialog.value = true;
};

const confirmDeleteAction = (register: CashRegister) => {
    confirmDelete(`la caja "${register.name}"`, async () => {
        await cashShiftService.deleteRegister(register.id);
        await fetchRegisters();
    });
};

onMounted(async () => {
    if (warehouseStore.warehouses.length === 0) {
        await warehouseStore.fetchWarehouses();
    }
    await fetchRegisters();
});
</script>
