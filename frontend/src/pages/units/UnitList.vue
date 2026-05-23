<template>
    <q-page padding>
        <div class="row q-col-gutter-sm">
            <div class="col-12">
                <q-table
                    title="Unidades"
                    :rows="units"
                    :columns="columns"
                    row-key="id"
                    :loading="loading"
                    :filter="filter"
                >
                    <template v-slot:top-right>
                        <BaseSearch @search="filter = $event" />
                        <q-btn
                            color="primary"
                            label="Nueva Unidad"
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
                                @click="confirmDelete(props.row)"
                            />
                        </q-td>
                    </template>
                </q-table>
            </div>
        </div>

        <UnitFormDialog
            v-model="showDialog"
            :initial-data="selectedUnitData"
            @saved="fetchUnits"
        />
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useQuasar } from "quasar";
import { unitService } from "@/services/unit.service";
import type { Unit } from "@/types";
import BaseSearch from "@/components/base/BaseSearch.vue";
import UnitFormDialog from "./components/UnitFormDialog.vue";

const $q = useQuasar();
const units = ref<Unit[]>([]);
const loading = ref(true);
const filter = ref("");
const showDialog = ref(false);
const selectedUnitData = ref<any>(null);

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
        name: "shortName",
        label: "Corto",
        field: "shortName",
        align: "left" as const,
    },
    {
        name: "operator",
        label: "Operador",
        field: "operator",
        align: "center" as const,
    },
    {
        name: "operatorValue",
        label: "Valor",
        field: "operatorValue",
        align: "center" as const,
    },
    {
        name: "actions",
        label: "Acciones",
        field: "actions",
        align: "center" as const,
    },
];

const fetchUnits = async () => {
    loading.value = true;
    try {
        const response = await unitService.getAll();
        units.value = response.data;
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al cargar unidades" });
    } finally {
        loading.value = false;
    }
};

const openDialog = (unit?: Unit) => {
    selectedUnitData.value = unit ? { ...unit } : null;
    showDialog.value = true;
};

const confirmDelete = (unit: Unit) => {
    $q.dialog({
        title: "Confirmar eliminación",
        message: `¿Eliminar la unidad ${unit.name}?`,
        cancel: true,
        persistent: true,
    }).onOk(async () => {
        try {
            await unitService.delete(unit.id!);
            $q.notify({ color: "positive", message: "Unidad eliminada" });
            fetchUnits();
        } catch (error) {
            $q.notify({
                color: "negative",
                message: "Error al eliminar unidad",
            });
        }
    });
};

onMounted(fetchUnits);
</script>
