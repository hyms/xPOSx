<template>
    <FormDialog
        :model-value="modelValue"
        @update:model-value="emit('update:modelValue', $event)"
        :title="isEdit ? 'Editar Cliente' : 'Nuevo Cliente'"
        @submit="saveClient"
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
                    v-model="formData.nitCi"
                    label="NIT/CI"
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
                    outlined
                    dense
                />
            </div>
            <div class="col-12">
                <q-input
                    v-model="formData.companyName"
                    label="Empresa"
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
import { clientService } from "@/services/client.service";
import FormDialog from "@/components/FormDialog.vue";

const props = defineProps<{
    modelValue: boolean;
    clientId?: number;
    initialData?: {
        id?: number;
        name?: string;
        code?: number | null;
        nitCi?: string;
        phone?: string;
        email?: string;
        companyName?: string;
        city?: string;
        address?: string;
    };
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
    (e: "saved"): void;
}>();

const $q = useQuasar();
const saving = ref(false);

const isEdit = computed(() => !!props.clientId || !!props.initialData?.id);

const formData = reactive({
    name: "",
    code: null as number | null,
    nitCi: "",
    phone: "",
    email: "",
    companyName: "",
    city: "",
    address: "",
});

const populateForm = (data: any) => {
    Object.assign(formData, {
        name: data.name || "",
        code: data.code || null,
        nitCi: data.nitCi || "",
        phone: data.phone || "",
        email: data.email || "",
        companyName: data.companyName || "",
        city: data.city || "",
        address: data.address || "",
    });
};

const resetForm = () => {
    Object.assign(formData, {
        name: "",
        code: null,
        nitCi: "",
        phone: "",
        email: "",
        companyName: "",
        city: "",
        address: "",
    });
};

const saveClient = async () => {
    saving.value = true;
    try {
        if (isEdit.value) {
            const updateId = props.clientId || props.initialData?.id;
            await clientService.update(updateId!, {
                ...formData,
                id: updateId
            } as any);
            $q.notify({
                color: "positive",
                message: "Cliente actualizado correctamente",
                icon: "check_circle",
            });
        } else {
            await clientService.create(formData as any);
            $q.notify({
                color: "positive",
                message: "Cliente creado correctamente",
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
