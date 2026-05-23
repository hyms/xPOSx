<template>
    <FormDialog
        :model-value="modelValue"
        @update:model-value="emit('update:modelValue', $event)"
        :title="isEdit ? 'Editar Proveedor' : 'Nuevo Proveedor'"
        @submit="saveProvider"
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
                    v-model.number="formData.code"
                    label="Código"
                    type="number"
                    lazy-rules
                    :rules="[(val) => !!val || 'Requerido']"
                    outlined
                    dense
                />
            </div>
            <div class="col-12 col-md-6">
                <q-input
                    v-model="formData.email"
                    label="Email"
                    type="email"
                    lazy-rules
                    :rules="[(val) => !!val || 'Requerido']"
                    outlined
                    dense
                />
            </div>
            <div class="col-12 col-md-6">
                <q-input
                    v-model="formData.phone"
                    label="Teléfono"
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
                    v-model="formData.country"
                    label="País"
                    outlined
                    dense
                />
            </div>
            <div class="col-12">
                <q-input
                    v-model="formData.address"
                    label="Dirección"
                    type="textarea"
                    autogrow
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
import { providerService } from "@/services/provider.service";
import FormDialog from "@/components/FormDialog.vue";

const props = defineProps<{
    modelValue: boolean;
    providerId?: number;
    initialData?: {
        id?: number;
        name?: string;
        code?: number;
        email?: string;
        phone?: string;
        city?: string;
        country?: string;
        address?: string;
    };
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
    (e: "saved"): void;
}>();

const $q = useQuasar();
const saving = ref(false);

const isEdit = computed(() => !!props.providerId || !!props.initialData?.id);

const formData = reactive({
    name: "",
    code: 0,
    email: "",
    phone: "",
    city: "",
    country: "",
    address: "",
});

const populateForm = (data: any) => {
    Object.assign(formData, {
        name: data.name || "",
        code: data.code !== undefined ? data.code : 0,
        email: data.email || "",
        phone: data.phone || "",
        city: data.city || "",
        country: data.country || "",
        address: data.address || "",
    });
};

const resetForm = () => {
    Object.assign(formData, {
        name: "",
        code: 0,
        email: "",
        phone: "",
        city: "",
        country: "",
        address: "",
    });
};

const saveProvider = async () => {
    saving.value = true;
    try {
        if (isEdit.value) {
            const updateId = props.providerId || props.initialData?.id;
            await providerService.update(updateId!, {
                ...formData,
                id: updateId
            } as any);
            $q.notify({
                color: "positive",
                message: "Proveedor actualizado correctamente",
                icon: "check_circle",
            });
        } else {
            await providerService.create(formData as any);
            $q.notify({
                color: "positive",
                message: "Proveedor creado correctamente",
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
