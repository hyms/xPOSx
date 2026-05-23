<template>
    <FormDialog
        :model-value="modelValue"
        @update:model-value="$emit('update:modelValue', $event)"
        :title="registerId ? 'Editar Caja' : 'Nueva Caja'"
        @submit="saveRegister"
        :saving="saving"
    >
        <div class="row q-col-gutter-sm">
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
</template>

<script setup lang="ts">
import { ref, watch, reactive, computed } from "vue";
import { useQuasar } from "quasar";
import { cashShiftService, type CashRegister } from "@/services/cashShift.service";
import { useWarehouseStore } from "@/stores/warehouse";
import FormDialog from "@/components/FormDialog.vue";

const props = defineProps<{
    modelValue: boolean;
    registerId?: number;
    initialData?: {
        id: number;
        name: string;
        warehouseId: number;
        isActive: boolean;
        isMatriz: boolean;
    };
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
    (e: "saved"): void;
}>();

const $q = useQuasar();
const warehouseStore = useWarehouseStore();
const saving = ref(false);

const formData = reactive({
    id: undefined as number | undefined,
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

const resetForm = () => {
    Object.assign(formData, {
        id: undefined,
        name: "",
        warehouseId: warehouseStore.activeWarehouseId || (warehouseOptions.value[0]?.value as number) || 0,
        isActive: true,
        isMatriz: false,
    });
};

const populateForm = (data: CashRegister) => {
    Object.assign(formData, { ...data });
};

const saveRegister = async () => {
    saving.value = true;
    try {
        if (props.registerId) {
            const payload = { ...formData };
            await cashShiftService.updateRegister(props.registerId, payload as CashRegister);
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
        emit("saved");
        emit("update:modelValue", false);
    } catch (error) {
        // Handled globally
    } finally {
        saving.value = false;
    }
};

watch(
    [() => props.modelValue, () => props.initialData],
    ([isOpen, data]) => {
        if (isOpen) {
            if (data) {
                populateForm(data as any);
            } else {
                resetForm();
            }
        }
    },
    { immediate: true }
);
</script>
