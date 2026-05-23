<template>
    <q-page padding>
        <div class="row q-col-gutter-sm">
            <div class="col-12">
                <q-table
                    title="Almacenes"
                    :rows="warehouses"
                    :columns="columns"
                    row-key="id"
                    :loading="loading"
                    :filter="filter"
                >
                    <template v-slot:top-right>
                        <BaseSearch @search="filter = $event" />
                        <q-btn
                            color="primary"
                            label="Nuevo Almacén"
                            icon="add"
                            @click="openDialog()"
                            class="q-ml-md"
                        />
                    </template>

                    <template v-slot:body-cell-actions="props">
                        <q-td :props="props">
                            <q-btn
                                flat
                                round
                                color="primary"
                                icon="edit"
                                @click="openDialog(props.row)"
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
            </div>
        </div>

        <!-- Warehouse Dialog -->
        <WarehouseFormDialog
            v-model="showDialog"
            :initial-data="selectedWarehouseData"
            @saved="fetchWarehouses"
        />
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useQuasar } from "quasar";
import { warehouseService } from "@/services/warehouse.service";
import type { Warehouse } from "@/types";
import { useConfirm } from "@/composables/useConfirm";
import BaseSearch from "@/components/base/BaseSearch.vue";
import WarehouseFormDialog from "./components/WarehouseFormDialog.vue";

const $q = useQuasar();
const { confirmDelete } = useConfirm();
const warehouses = ref<Warehouse[]>([]);
const loading = ref(true);
const filter = ref("");
const showDialog = ref(false);
const selectedWarehouseData = ref<any>(null);

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
        label: "Nombre",
        field: "name",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "city",
        label: "Ciudad",
        field: "city",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "mobile",
        label: "Teléfono",
        field: "mobile",
        align: "left" as const,
    },
    {
        name: "actions",
        label: "Acciones",
        field: "actions",
        align: "center" as const,
    },
];

const fetchWarehouses = async () => {
    loading.value = true;
    try {
        const response = await warehouseService.getAll();
        warehouses.value = response.data;
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al cargar almacenes" });
    } finally {
        loading.value = false;
    }
};

const openDialog = (warehouse?: Warehouse) => {
    selectedWarehouseData.value = warehouse ? { ...warehouse } : null;
    showDialog.value = true;
};

const confirmDeleteAction = (warehouse: Warehouse) => {
    confirmDelete(warehouse.name, async () => {
        await warehouseService.delete(warehouse.id!);
        fetchWarehouses();
    });
};

onMounted(fetchWarehouses);
</script>
