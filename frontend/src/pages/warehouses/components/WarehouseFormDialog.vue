<template>
    <FormDialog
        :model-value="modelValue"
        @update:model-value="emit('update:modelValue', $event)"
        :title="isEdit ? 'Editar Almacén' : 'Nuevo Almacén'"
        @submit="saveWarehouse"
        :saving="saving"
    >
        <div class="row q-col-gutter-sm">
            <div class="col-12 col-md-6">
                <q-input
                    v-model="formData.name"
                    label="Nombre del Almacén"
                    lazy-rules
                    :rules="[(val) => !!val || 'Requerido']"
                    outlined
                    dense
                />
            </div>
            <div class="col-12 col-md-6">
                <q-input
                    v-model="formData.city"
                    label="Ciudad"
                    outlined
                    dense
                />
            </div>
            <div class="col-12 col-md-6">
                <q-input
                    v-model="formData.mobile"
                    label="Teléfono/Móvil"
                    outlined
                    dense
                />
            </div>
            <div class="col-12 col-md-6">
                <q-input
                    v-model="formData.email"
                    label="Email"
                    type="email"
                    outlined
                    dense
                />
            </div>
            <div class="col-12">
                <q-input
                    v-model="formData.country"
                    label="País"
                    outlined
                    dense
                />
            </div>
        </div>
    </FormDialog>
</template>

<script setup lang="ts">
import { ref, watch, reactive, computed } from "vue";
import { useQuasar } from "quasar";
import { warehouseService } from "@/services/warehouse.service";
import FormDialog from "@/components/FormDialog.vue";

const props = defineProps<{
    modelValue: boolean;
    warehouseId?: number;
    initialData?: {
        id?: number;
        name?: string;
        city?: string;
        mobile?: string;
        email?: string;
        country?: string;
    };
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
    (e: "saved"): void;
}>();

const $q = useQuasar();
const saving = ref(false);

const isEdit = computed(() => !!props.warehouseId || !!props.initialData?.id);

const formData = reactive({
    name: "",
    city: "",
    mobile: "",
    email: "",
    country: "",
});

const populateForm = (data: any) => {
    Object.assign(formData, {
        name: data.name || "",
        city: data.city || "",
        mobile: data.mobile || "",
        email: data.email || "",
        country: data.country || "",
    });
};

const resetForm = () => {
    Object.assign(formData, {
        name: "",
        city: "",
        mobile: "",
        email: "",
        country: "",
    });
};

const saveWarehouse = async () => {
    saving.value = true;
    try {
        if (isEdit.value) {
            const updateId = props.warehouseId || props.initialData?.id;
            await warehouseService.update(updateId!, {
                ...formData,
                id: updateId
            } as any);
            $q.notify({
                color: "positive",
                message: "Almacén actualizado correctamente",
                icon: "check_circle",
            });
        } else {
            await warehouseService.create(formData as any);
            $q.notify({
                color: "positive",
                message: "Almacén creado correctamente",
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
