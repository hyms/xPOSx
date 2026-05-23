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

        <ExpenseCategoryFormDialog
            v-model="showDialog"
            :initial-data="selectedCategoryData"
            @saved="fetchItems"
        />
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useQuasar } from "quasar";
import { expenseService } from "@/services/expense.service";
import type { ExpenseCategory } from "@/types";
import { useTable } from "@/composables/useTable";
import { useConfirm } from "@/composables/useConfirm";
import BaseTable from "@/components/base/BaseTable.vue";
import BaseSearch from "@/components/base/BaseSearch.vue";
import BaseButton from "@/components/base/BaseButton.vue";
import ExpenseCategoryFormDialog from "./components/ExpenseCategoryFormDialog.vue";

const $q = useQuasar();
const { confirmDelete } = useConfirm();

const {
    data: categories,
    loading,
    pagination,
    fetchItems,
} = useTable<ExpenseCategory>("/expense-categories");

const showDialog = ref(false);
const selectedCategoryData = ref<any>(null);

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
    selectedCategoryData.value = category ? { ...category } : null;
    showDialog.value = true;
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