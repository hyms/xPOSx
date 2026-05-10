<template>
    <q-page padding>
        <div class="row q-col-gutter-md">
            <div class="col-12">
                <q-table
                    title="Unidades"
                    :rows="units"
                    :columns="columns"
                    row-key="id"
                    :loading="loading"
                >
                    <template v-slot:top-right>
                        <q-btn
                            color="primary"
                            label="Nueva Unidad"
                            icon="add"
                            @click="openDialog()"
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

        <!-- Unit Dialog -->
        <q-dialog v-model="showDialog" persistent>
            <q-card style="min-width: 400px">
                <q-card-section>
                    <div class="text-h6">
                        {{ isEdit ? "Editar Unidad" : "Nueva Unidad" }}
                    </div>
                </q-card-section>

                <q-card-section class="q-pt-none">
                    <q-form @submit="saveUnit" class="q-gutter-md">
                        <q-input
                            v-model="formData.name"
                            label="Nombre"
                            lazy-rules
                            :rules="[(val) => !!val || 'Requerido']"
                        />
                        <q-input
                            v-model="formData.shortName"
                            label="Nombre Corto"
                            lazy-rules
                            :rules="[(val) => !!val || 'Requerido']"
                        />

                        <q-select
                            v-model="formData.baseUnit"
                            :options="unitOptions"
                            label="Unidad Base"
                            option-value="id"
                            option-label="name"
                            emit-value
                            map-options
                            clearable
                        />

                        <div
                            class="row q-col-gutter-sm"
                        >
                            <div class="col-6">
                                <q-select
                                    v-model="formData.operator"
                                    :options="['*', '/']"
                                    label="Operador"
                                />
                            </div>
                            <div class="col-6">
                                <q-input
                                    v-model.number="formData.operatorValue"
                                    label="Valor"
                                    type="number"
                                />
                            </div>
                        </div>

                        <div class="row justify-end q-mt-md">
                            <q-btn
                                label="Cancelar"
                                color="primary"
                                flat
                                v-close-popup
                            />
                            <q-btn
                                :label="isEdit ? 'Actualizar' : 'Guardar'"
                                color="primary"
                                type="submit"
                                :loading="saving"
                            />
                        </div>
                    </q-form>
                </q-card-section>
            </q-card>
        </q-dialog>
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, computed } from "vue";
import { useQuasar } from "quasar";
import { unitService } from "@/services/unit.service";
import type { Unit } from "@/types";

const $q = useQuasar();
const units = ref<Unit[]>([]);
const loading = ref(true);
const saving = ref(false);
const showDialog = ref(false);
const isEdit = ref(false);

const formData = reactive<Unit>({
    name: "",
    shortName: "",
    baseUnit: undefined,
    operator: "*",
    operatorValue: 1,
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

const unitOptions = computed(() => units.value.filter((u) => !u.baseUnit));

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
    if (unit) {
        isEdit.value = true;
        Object.assign(formData, { ...unit });
    } else {
        isEdit.value = false;
        Object.assign(formData, {
            name: "",
            shortName: "",
            baseUnit: undefined,
            operator: "*",
            operatorValue: 1,
        });
    }
    showDialog.value = true;
};

const saveUnit = async () => {
    saving.value = true;
    try {
        if (isEdit.value) {
            await unitService.update(formData.id!, formData);
            $q.notify({ color: "positive", message: "Unidad actualizada" });
        } else {
            await unitService.create(formData);
            $q.notify({ color: "positive", message: "Unidad creada" });
        }
        showDialog.value = false;
        fetchUnits();
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al guardar unidad" });
    } finally {
        saving.value = false;
    }
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
