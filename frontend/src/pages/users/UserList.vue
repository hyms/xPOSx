<template>
    <q-page padding>
        <div class="row q-col-gutter-sm">
            <div class="col-12">
                <q-table
                    title="Usuarios"
                    :rows="users"
                    :columns="columns"
                    row-key="id"
                    :loading="loading"
                    :filter="filter"
                >
                    <template v-slot:top-right>
                        <BaseSearch @search="filter = $event" />
                        <q-btn
                            color="primary"
                            label="Nuevo Usuario"
                            icon="add"
                            @click="openDialog()"
                            class="q-ml-md"
                        />
                    </template>

                    <template v-slot:body-cell-status="props">
                        <q-td :props="props">
                            <q-toggle
                                :model-value="props.value"
                                checked-icon="check"
                                unchecked-icon="clear"
                                color="positive"
                                @update:model-value="
                                    handleToggleStatus(props.row)
                                "
                            />
                        </q-td>
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
                                @click="confirmDeleteAction(props.row)"
                            />
                        </q-td>
                    </template>
                </q-table>
            </div>
        </div>

        <!-- User Dialog -->
        <UserFormDialog
            v-model="showDialog"
            :initial-data="selectedUserData"
            @saved="fetchUsers"
        />
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useQuasar } from "quasar";
import { userService } from "@/services/user.service";
import type { User } from "@/types";
import { warehouseService } from "@/services/warehouse.service";
import BaseSearch from "@/components/base/BaseSearch.vue";
import UserFormDialog from "./components/UserFormDialog.vue";
import { useConfirm } from "@/composables/useConfirm";

const $q = useQuasar();
const { confirmDelete } = useConfirm();
const users = ref<User[]>([]);
const loading = ref(true);
const filter = ref("");
const showDialog = ref(false);
const warehouseOptions = ref<any[]>([]);
const selectedUserData = ref<any>(null);

const columns = [
    {
        name: "fullName",
        label: "Nombre Completo",
        field: (row: User) => `${row.firstName} ${row.lastName}`,
        sortable: true,
        align: "left" as const,
    },
    {
        name: "username",
        label: "Usuario",
        field: "username",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "email",
        label: "Email",
        field: "email",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "roleName",
        label: "Rol",
        field: (row: User) => row.roleDetails?.name,
        sortable: true,
        align: "left" as const,
    },
    {
        name: "defaultWarehouse",
        label: "Almacén por Defecto",
        field: (row: User) => {
            const wh = warehouseOptions.value.find((w) => w.value === row.defaultWarehouseId);
            return wh ? wh.label : "-";
        },
        sortable: true,
        align: "left" as const,
    },
    {
        name: "warehouses",
        label: "Almacenes Permitidos",
        field: (row: User) => {
            if (row.allWarehousesAccess) return "Todos";
            if (!row.warehouseIds || row.warehouseIds.length === 0) return "-";
            return row.warehouseIds
                .map((id) => warehouseOptions.value.find((w) => w.value === id)?.label)
                .filter(Boolean)
                .join(", ");
        },
        sortable: false,
        align: "left" as const,
    },
    {
        name: "status",
        label: "Estado",
        field: "isActive",
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

const fetchUsers = async () => {
    loading.value = true;
    try {
        const response = await userService.getAll();
        users.value = response.data;
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al cargar usuarios" });
    } finally {
        loading.value = false;
    }
};

const fetchWarehouses = async () => {
    try {
        const response = await warehouseService.getAll();
        warehouseOptions.value = response.data.map((w) => ({
            label: w.name,
            value: w.id,
        }));
    } catch (error) {
        console.error("Error fetching warehouses:", error);
    }
};

const openDialog = (user?: User) => {
    selectedUserData.value = user ? { ...user } : null;
    showDialog.value = true;
};

const confirmDeleteAction = (user: User) => {
    confirmDelete(user.username, async () => {
        await userService.delete(user.id!);
        fetchUsers();
    });
};

const handleToggleStatus = (user: User) => {
    const action = user.isActive ? "desactivar" : "activar";
    const color = user.isActive ? "negative" : "positive";

    $q.dialog({
        title: "Confirmar",
        message: `¿Estás seguro de que quieres ${action} al usuario ${user.username}?`,
        cancel: true,
        persistent: true,
        ok: {
            color: color,
            label: action.charAt(0).toUpperCase() + action.slice(1),
        },
    }).onOk(async () => {
        try {
            await userService.toggleStatus(user.id!);
            $q.notify({
                color: "positive",
                message: `Usuario ${action}do correctamente.`,
            });
            fetchUsers();
        } catch (error) {
            $q.notify({
                color: "negative",
                message: `Error al ${action} el usuario.`,
            });
            // Revert toggle on error if needed, though fetchUsers() will handle it
        }
    });
};

onMounted(() => {
    fetchUsers();
    fetchWarehouses();
});
</script>
