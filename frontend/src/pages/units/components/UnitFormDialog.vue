<template>
    <FormDialog
        :model-value="modelValue"
        @update:model-value="emit('update:modelValue', $event)"
        :title="isEdit ? 'Editar Unidad' : 'Nueva Unidad'"
        @submit="saveUnit"
        :saving="saving"
    >
        <div class="row q-col-gutter-sm">
            <div class="col-12 col-md-6">
                <q-input
                    v-model="formData.name"
                    label="Nombre"
                    lazy-rules
                    :rules="[(val) => !!val || 'Requerido']"
                    outlined
                    dense
                />
            </div>
            <div class="col-12 col-md-6">
                <q-input
                    v-model="formData.shortName"
                    label="Nombre Corto"
                    lazy-rules
                    :rules="[(val) => !!val || 'Requerido']"
                    outlined
                    dense
                />
            </div>
            <div class="col-12">
                <q-select
                    v-model="formData.baseUnit"
                    :options="unitOptions"
                    label="Unidad Base"
                    option-value="id"
                    option-label="name"
                    emit-value
                    map-options
                    clearable
                    outlined
                    dense
                />
            </div>
            <div class="col-12 col-md-6">
                <q-select
                    v-model="formData.operator"
                    :options="['*', '/']"
                    label="Operador"
                    outlined
                    dense
                />
            </div>
            <div class="col-12 col-md-6">
                <q-input
                    v-model.number="formData.operatorValue"
                    label="Valor"
                    type="number"
                    outlined
                    dense
                />
            </div>
        </div>
    </FormDialog>
</template>

<script setup lang="ts">
import { ref, watch, reactive, computed, onMounted } from "vue";
import { useQuasar } from "quasar";
import { unitService } from "@/services/unit.service";
import FormDialog from "@/components/FormDialog.vue";

const props = defineProps<{
    modelValue: boolean;
    unitId?: number;
    initialData?: {
        id?: number;
        name?: string;
        shortName?: string;
        baseUnit?: number;
        operator?: string;
        operatorValue?: number;
    };
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
    (e: "saved"): void;
}>();

const $q = useQuasar();
const saving = ref(false);
const unitOptions = ref<any[]>([]);

const isEdit = computed(() => !!props.unitId || !!props.initialData?.id);

const formData = reactive({
    name: "",
    shortName: "",
    baseUnit: undefined as number | undefined,
    operator: "*",
    operatorValue: 1,
});

const populateForm = (data: any) => {
    Object.assign(formData, {
        name: data.name || "",
        shortName: data.shortName || "",
        baseUnit: data.baseUnit || undefined,
        operator: data.operator || "*",
        operatorValue: data.operatorValue !== undefined ? data.operatorValue : 1,
    });
};

const resetForm = () => {
    Object.assign(formData, {
        name: "",
        shortName: "",
        baseUnit: undefined,
        operator: "*",
        operatorValue: 1,
    });
};

const fetchOptions = async () => {
    try {
        const response = await unitService.getAll();
        unitOptions.value = response.data.filter((u: any) => !u.baseUnit);
    } catch (error) {
        console.error("Error fetching units:", error);
    }
};

const saveUnit = async () => {
    saving.value = true;
    try {
        if (isEdit.value) {
            const updateId = props.unitId || props.initialData?.id;
            await unitService.update(updateId!, {
                ...formData,
                id: updateId
            } as any);
            $q.notify({
                color: "positive",
                message: "Unidad actualizada correctamente",
                icon: "check_circle",
            });
        } else {
            await unitService.create(formData as any);
            $q.notify({
                color: "positive",
                message: "Unidad creada correctamente",
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
            fetchOptions();
            if (data) {
                populateForm(data);
            } else {
                resetForm();
            }
        }
    },
    { immediate: true }
);

onMounted(fetchOptions);
</script>
