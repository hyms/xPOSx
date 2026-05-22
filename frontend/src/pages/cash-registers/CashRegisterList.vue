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
        <FormDialog
            v-model="showDialog"
            :title="isEdit ? 'Editar Caja' : 'Nueva Caja'"
            @submit="saveRegister"
            :saving="saving"
        >
            <div class="row q-col-gutter-md">
                <div class="col-12 col-md-6">
                    <q-input
                        v-model="formData.name"
                        label="Nombre de la Caja"
                        lazy-rules
                        :rules="[(val) => !!val || 'El nombre es requerido']"
                        outlined
                        dense
                    />
                </div>
                <div class="col-12 col-md-6">
                    <q-select
                        v-model="formData.warehouseId"
                        :options="warehouseOptions"
                        label="Almacén Asociado"
                        emit-value
                        map-options
                        lazy-rules
                        :rules="[(val) => !!val || 'El almacén es requerido']"
                        outlined
                        dense
                    />
                </div>
                <div class="col-12 col-sm-6 flex items-center">
                    <q-toggle
                        v-model="formData.isActive"
                        label="Caja Activa (Disponible para ventas)"
                        color="primary"
                    />
                </div>
                <div class="col-12 col-sm-6 flex items-center">
                    <q-toggle
                        v-model="formData.isMatriz"
                        label="¿Es Caja Principal del Almacén?"
                        color="primary"
                    />
                </div>
            </div>
        </FormDialog>
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, computed } from "vue";
import { useQuasar } from "quasar";
import { cashShiftService, type CashRegister } from "@/services/cashShift.service";
import { useWarehouseStore } from "@/stores/warehouse";
import { useConfirm } from "@/composables/useConfirm";
import FormDialog from "@/components/FormDialog.vue";
import BaseSearch from "@/components/base/BaseSearch.vue";

const $q = useQuasar();
const { confirmDelete } = useConfirm();
const warehouseStore = useWarehouseStore();

const registers = ref<CashRegister[]>([]);
const loading = ref(true);
const filter = ref("");
const saving = ref(false);
const showDialog = ref(false);
const isEdit = ref(false);

const formData = reactive<{
    id?: number;
    name: string;
    warehouseId: number;
    isActive: boolean;
    isMatriz: boolean;
}>({
    name: "",
    warehouseId: 0,
    isActive: true,
    isMatriz: false,
});

const warehouseOptions = computed(() => {
    return warehouseStore.warehouses.map((w) => ({
        label: w.name,
        value: w.id,
    }));
});

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
        // Handled globally, fallback notification
    } finally {
        loading.value = false;
    }
};

const openDialog = (register?: CashRegister) => {
    if (register) {
        isEdit.value = true;
        Object.assign(formData, { ...register });
    } else {
        isEdit.value = false;
        Object.assign(formData, {
            id: undefined,
            name: "",
            warehouseId: warehouseStore.activeWarehouseId || (warehouseOptions.value[0]?.value as number) || 0,
            isActive: true,
            isMatriz: false,
        });
    }
    showDialog.value = true;
};

const saveRegister = async () => {
    saving.value = true;
    try {
        if (isEdit.value) {
            const payload = { ...formData };
            await cashShiftService.updateRegister(formData.id!, payload as CashRegister);
            $q.notify({
                color: "positive",
                message: "Caja actualizada correctamente",
                icon: "check_circle",
            });
        } else {
            const { id, ...payload } = formData;
            await cashShiftService.createRegister(payload);
            $q.notify({
                color: "positive",
                message: "Caja creada correctamente",
                icon: "check_circle",
            });
        }
        showDialog.value = false;
        await fetchRegisters();
    } catch (error) {
        // Handled globally by the interceptor
    } finally {
        saving.value = false;
    }
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