<template>
    <q-page padding>
        <BaseTable
            title="Categorías de Gastos"
            :rows="categories"
            :columns="columns"
            row-key="id"
            :loading="loading"
            :pagination="pagination"
            @request="fetchItems"
        >
            <template #top-right>
                <div class="row q-gutter-sm items-center full-width-xs">
                    <BaseSearch
                        @search="fetchItems({ pagination, filter: $event })"
                        class="col-grow"
                    />
                    <BaseButton
                        label="Nueva Categoría"
                        icon="add"
                        @click="openDialog()"
                        class="full-width-xs mobile-only-mt"
                    />
                </div>
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
        </BaseTable>

        <FormDialog
            v-model="showDialog"
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
    </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { useQuasar } from "quasar";
import { expenseService } from "@/services/expense.service";
import type { ExpenseCategory } from "@/types";
import { useTable } from "@/composables/useTable";
import { useConfirm } from "@/composables/useConfirm";
import BaseTable from "@/components/base/BaseTable.vue";
import BaseSearch from "@/components/base/BaseSearch.vue";
import BaseButton from "@/components/base/BaseButton.vue";
import FormDialog from "@/components/FormDialog.vue";

const $q = useQuasar();
const { confirmDelete } = useConfirm();

const {
    data: categories,
    loading,
    pagination,
    fetchItems,
} = useTable<ExpenseCategory>("/expense-categories");

const saving = ref(false);
const showDialog = ref(false);
const isEdit = ref(false);

const formData = reactive<ExpenseCategory>({
    name: "",
    description: "",
});

const columns = [
    {
        name: "name",
        label: "Nombre",
        field: "name",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "description",
        label: "Descripción",
        field: "description",
        align: "left" as const,
    },
    {
        name: "actions",
        label: "Acciones",
        field: "actions",
        align: "center" as const,
    },
];

const openDialog = (category?: ExpenseCategory) => {
    if (category) {
        isEdit.value = true;
        Object.assign(formData, category);
    } else {
        isEdit.value = false;
        Object.assign(formData, {
            id: undefined,
            name: "",
            description: "",
        });
    }
    showDialog.value = true;
};

const saveCategory = async () => {
    saving.value = true;
    try {
        if (isEdit.value) {
            await expenseService.updateCategory(formData.id!, formData);
            $q.notify({ color: "positive", message: "Categoría actualizada" });
        } else {
            await expenseService.createCategory(formData);
            $q.notify({ color: "positive", message: "Categoría creada" });
        }
        showDialog.value = false;
        fetchItems();
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al guardar categoría" });
    } finally {
        saving.value = false;
    }
};

const confirmDeleteAction = (category: ExpenseCategory) => {
    confirmDelete(category.name, async () => {
        try {
            await expenseService.deleteCategory(category.id!);
            $q.notify({ color: "positive", message: "Categoría eliminada" });
            fetchItems();
        } catch (error) {
            $q.notify({ color: "negative", message: "Error al eliminar categoría" });
        }
    });
};

onMounted(fetchItems);
</script>