<template>
    <FormDialog
        :model-value="modelValue"
        @update:model-value="emit('update:modelValue', $event)"
        :title="isEdit ? 'Editar Usuario' : 'Nuevo Usuario'"
        @submit="saveUser"
        :saving="saving"
    >
        <div class="row q-col-gutter-sm">
            <div class="col-12 col-md-6">
                <BaseInput
                    v-model="formData.username"
                    label="Nombre de Usuario"
                    lazy-rules
                    :rules="[rules.required]"
                />
            </div>
            <div class="col-12 col-md-6">
                <BaseInput
                    v-model="formData.email"
                    label="Email"
                    type="email"
                    lazy-rules
                    :rules="[rules.required, rules.email]"
                />
            </div>
            <div class="col-12 col-md-6">
                <BaseInput
                    v-model="formData.firstName"
                    label="Nombre"
                    lazy-rules
                    :rules="[rules.required]"
                />
            </div>
            <div class="col-12 col-md-6">
                <BaseInput v-model="formData.lastName" label="Apellido" />
            </div>
            <div class="col-12 col-md-6">
                <q-select
                    v-model="formData.role"
                    :options="roleOptions"
                    label="Rol"
                    emit-value
                    map-options
                    lazy-rules
                    :rules="[rules.required]"
                    outlined
                    dense
                    hide-bottom-space
                />
            </div>
            <div class="col-12 col-md-6">
                <q-select
                    v-model="formData.defaultWarehouseId"
                    :options="warehouseOptions"
                    label="Almacén por Defecto"
                    emit-value
                    map-options
                    outlined
                    dense
                    clearable
                    hide-bottom-space
                />
            </div>
            <div class="col-12 col-md-6 flex items-center">
                <q-toggle
                    v-model="formData.allWarehousesAccess"
                    label="Acceso a Todos los Almacenes"
                    color="primary"
                />
            </div>
            <div class="col-12 col-md-6" v-if="!formData.allWarehousesAccess">
                <q-select
                    v-model="formData.warehouseIds"
                    :options="warehouseOptions"
                    label="Almacenes Permitidos"
                    emit-value
                    map-options
                    multiple
                    use-chips
                    outlined
                    dense
                    hide-bottom-space
                />
            </div>
            <div class="col-12 col-md-6" v-if="!isEdit">
                <BaseInput
                    v-model="formData.password"
                    label="Contraseña"
                    type="password"
                    lazy-rules
                    :rules="[rules.required]"
                />
            </div>
        </div>
    </FormDialog>
</template>

<script setup lang="ts">
import { ref, watch, reactive, computed, onMounted } from "vue";
import { useQuasar } from "quasar";
import { userService } from "@/services/user.service";
import { roleService } from "@/services/role.service";
import { warehouseService } from "@/services/warehouse.service";
import FormDialog from "@/components/FormDialog.vue";
import BaseInput from "@/components/base/BaseInput.vue";
import { rules } from "@/utils/validations";

const props = defineProps<{
    modelValue: boolean;
    userId?: number;
    initialData?: {
        id?: number;
        username?: string;
        email?: string;
        password?: string;
        firstName?: string;
        lastName?: string;
        role?: number;
        isActive?: boolean;
        defaultWarehouseId?: number | null;
        warehouseIds?: number[];
        allWarehousesAccess?: boolean;
    };
}>();

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
    (e: "saved"): void;
}>();

const $q = useQuasar();
const saving = ref(false);
const roleOptions = ref<{ label: string; value: number }[]>([]);
const warehouseOptions = ref<{ label: string; value: number }[]>([]);

const isEdit = computed(() => !!props.userId || !!props.initialData?.id);

const formData = reactive({
    username: "",
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    role: 0,
    isActive: true,
    defaultWarehouseId: null as number | null,
    warehouseIds: [] as number[],
    allWarehousesAccess: true,
});

const populateForm = (data: any) => {
    Object.assign(formData, {
        username: data.username || "",
        email: data.email || "",
        password: "",
        firstName: data.firstName || "",
        lastName: data.lastName || "",
        role: data.role || 0,
        isActive: data.isActive !== false,
        defaultWarehouseId: data.defaultWarehouseId || null,
        warehouseIds: data.warehouseIds || [],
        allWarehousesAccess: data.allWarehousesAccess !== false,
    });
};

const resetForm = () => {
    Object.assign(formData, {
        username: "",
        email: "",
        password: "",
        firstName: "",
        lastName: "",
        role: 0,
        isActive: true,
        defaultWarehouseId: null,
        warehouseIds: [],
        allWarehousesAccess: true,
    });
};

const fetchOptions = async () => {
    try {
        const [rolesRes, warehousesRes] = await Promise.all([
            roleService.getAll(),
            warehouseService.getAll(),
        ]);
        roleOptions.value = rolesRes.data.map((r) => ({
            label: r.name,
            value: r.id,
        }));
        warehouseOptions.value = warehousesRes.data.map((w) => ({
            label: w.name,
            value: w.id,
        }));
    } catch (error) {
        console.error("Error fetching form options:", error);
    }
};

const saveUser = async () => {
    saving.value = true;
    try {
        if (isEdit.value) {
            const updateId = props.userId || props.initialData?.id;
            await userService.update(updateId!, {
                ...formData,
                id: updateId
            } as any);
            $q.notify({
                color: "positive",
                message: "Usuario actualizado correctamente",
                icon: "check_circle",
            });
        } else {
            await userService.create(formData as any);
            $q.notify({
                color: "positive",
                message: "Usuario creado correctamente",
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

onMounted(fetchOptions);
</script>
