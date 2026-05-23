<template>
    <FormDialog
        :model-value="modelValue"
        @update:model-value="emit('update:modelValue', $event)"
        :title="isEdit ? 'Editar Categoría' : 'Nueva Categoría'"
        @submit="saveCategory"
        :saving="saving"
    >
        <q-input
            v-model="formData.name"
            label="Nombre"
            lazy-rules
            :rules="[(val) => !!val || 'Requerido']"
            outlined
            dense
        />
        <q-input
            v-model="formData.description"
            label="Descripción"
            type="textarea"
            autogrow
            outlined
            dense
        />
    </FormDialog>
</template>

<script setup lang="ts">
import { ref, watch, reactive, computed } from "vue";
import { useQuasar } from "quasar";
import { expenseService } from "@/services/expense.service";
import FormDialog from "@/components/FormDialog.vue";

const props = defineProps<{
    modelValue: boolean;
    categoryId?: number;
    initialData?: {
        id?: number;
        name?: string;
        description?: string;
    };
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
    (e: "saved"): void;
}>();

const $q = useQuasar();
const saving = ref(false);

const isEdit = computed(() => !!props.categoryId || !!props.initialData?.id);

const formData = reactive({
    name: "",
    description: "",
});

const populateForm = (data: any) => {
    Object.assign(formData, {
        name: data.name || "",
        description: data.description || "",
    });
};

const resetForm = () => {
    Object.assign(formData, {
        name: "",
        description: "",
    });
};

const saveCategory = async () => {
    saving.value = true;
    try {
        if (isEdit.value) {
            const updateId = props.categoryId || props.initialData?.id;
            await expenseService.updateCategory(updateId!, {
                ...formData,
                id: updateId
            } as any);
            $q.notify({
                color: "positive",
                message: "Categoría actualizada correctamente",
                icon: "check_circle",
            });
        } else {
            await expenseService.createCategory(formData as any);
            $q.notify({
                color: "positive",
                message: "Categoría creada correctamente",
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
                populateForm(data);
            } else {
                resetForm();
            }
        }
    },
    { immediate: true }
);
</script>
