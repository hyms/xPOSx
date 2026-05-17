<template>
    <q-page padding>
        <q-card>
            <q-tabs
                v-model="tab"
                dense
                class="text-grey"
                active-color="primary"
                indicator-color="primary"
                align="justify"
                narrow-indicator
            >
                <q-tab name="expenses" label="Gastos" icon="receipt" />
                <q-tab name="categories" label="Categorías" icon="category" />
            </q-tabs>

            <q-separator />

            <q-tab-panels v-model="tab" animated>
                <!-- Expenses Tab -->
                <q-tab-panel name="expenses">
                    <q-table
                        title="Lista de Gastos"
                        :rows="expenses"
                        :columns="expenseColumns"
                        row-key="id"
                        :loading="loadingExpenses"
                        >
                        <template v-slot:top-right>
                            <q-btn
                                color="primary"
                                label="Nuevo"
                                icon="add"
                                @click="openExpenseDialog()"
                                class="full-width-xs"
                            />
                        </template>

                        <template v-slot:item="props">
                            <div class="q-pa-xs col-xs-12 col-sm-6 col-md-4">
                                <q-card flat bordered>
                                    <q-card-section>
                                        <div
                                            class="row items-center justify-between"
                                        >
                                            <div
                                                class="text-subtitle2 text-weight-bold"
                                            >
                                                {{ props.row.ref }}
                                            </div>
                                            <div class="text-caption text-grey">
                                                {{
                                                    props.row.date.split("T")[0]
                                                }}
                                            </div>
                                        </div>
                                        <div
                                            class="q-mt-sm text-caption text-grey"
                                        >
                                            Categoría:
                                            {{ props.row.categoryName }}
                                        </div>
                                        <div
                                            class="row q-mt-sm items-center justify-between"
                                        >
                                            <div class="text-caption text-grey">
                                                {{ props.row.warehouseName }}
                                            </div>
                                            <div
                                                class="text-subtitle1 text-weight-bolder text-negative"
                                            >
                                                {{
                                                    formatCurrency(
                                                        props.row.amount,
                                                    )
                                                }}
                                            </div>
                                        </div>
                                    </q-card-section>
                                    <q-separator />
                                    <q-card-actions align="right">
                                        <q-btn
                                            flat
                                            round
                                            color="primary"
                                            icon="edit"
                                            size="sm"
                                            @click="
                                                openExpenseDialog(props.row)
                                            "
                                        />
                                        <q-btn
                                            flat
                                            round
                                            color="negative"
                                            icon="delete"
                                            size="sm"
                                            @click="
                                                confirmDeleteExpense(props.row)
                                            "
                                        />
                                    </q-card-actions>
                                </q-card>
                            </div>
                        </template>

                        <template v-slot:body-cell-amount="props">
                            <q-td :props="props">
                                {{ formatCurrency(props.row.amount) }}
                            </q-td>
                        </template>

                        <template v-slot:body-cell-actions="props">
                            <q-td :props="props">
                                <q-btn
                                    flat
                                    round
                                    color="primary"
                                    icon="edit"
                                    @click="openExpenseDialog(props.row)"
                                />
                                <q-btn
                                    flat
                                    round
                                    color="negative"
                                    icon="delete"
                                    @click="confirmDeleteExpense(props.row)"
                                />
                            </q-td>
                        </template>
                    </q-table>
                </q-tab-panel>

                <!-- Categories Tab -->
                <q-tab-panel name="categories">
                    <q-table
                        title="Categorías de Gastos"
                        :rows="categories"
                        :columns="categoryColumns"
                        row-key="id"
                        :loading="loadingCategories"
                        >
                        <template v-slot:top-right>
                            <q-btn
                                color="primary"
                                label="Nueva Categoría"
                                icon="add"
                                @click="openCategoryDialog()"
                            />
                        </template>
                        <template v-slot:item="props">
                            <div class="q-pa-xs col-xs-12 col-sm-6 col-md-4">
                                <q-card flat bordered>
                                    <q-card-section>
                                        <div
                                            class="text-subtitle2 text-weight-bold"
                                        >
                                            {{ props.row.name }}
                                        </div>
                                        <div
                                            class="text-caption text-grey ellipsis"
                                        >
                                            {{
                                                props.row.description ||
                                                "Sin descripción"
                                            }}
                                        </div>
                                    </q-card-section>
                                    <q-card-actions align="right">
                                        <q-btn
                                            flat
                                            round
                                            color="primary"
                                            icon="edit"
                                            size="sm"
                                            @click="
                                                openCategoryDialog(props.row)
                                            "
                                        />
                                        <q-btn
                                            flat
                                            round
                                            color="negative"
                                            icon="delete"
                                            size="sm"
                                            @click="
                                                confirmDeleteCategory(props.row)
                                            "
                                        />
                                    </q-card-actions>
                                </q-card>
                            </div>
                        </template>
                        <template v-slot:body-cell-actions="props">
                            <q-td :props="props">
                                <q-btn
                                    flat
                                    round
                                    color="primary"
                                    icon="edit"
                                    @click="openCategoryDialog(props.row)"
                                />
                                <q-btn
                                    flat
                                    round
                                    color="negative"
                                    icon="delete"
                                    @click="confirmDeleteCategory(props.row)"
                                />
                            </q-td>
                        </template>
                    </q-table>
                </q-tab-panel>
            </q-tab-panels>
        </q-card>

        <!-- Expense Dialog -->
        <FormDialog
            v-model="showExpenseDialog"
            :title="isEditExpense ? 'Editar Gasto' : 'Nuevo Gasto'"
            @submit="saveExpense"
            :saving="saving"
        >
            <div class="row q-col-gutter-sm">
                <div class="col-12 col-md-6">
                    <q-input
                        v-model="expenseForm.date"
                        label="Fecha"
                        type="date"
                        stack-label
                        lazy-rules
                        :rules="[(val) => !!val || 'Requerido']"
                        outlined
                        dense
                    />
                </div>
                <div class="col-12 col-md-6">
                    <q-input
                        v-model.number="expenseForm.amount"
                        label="Monto"
                        type="number"
                        step="0.01"
                        :prefix="currencySymbol"
                        lazy-rules
                        :rules="[
                            (val) => val > 0 || 'Monto debe ser mayor a 0',
                        ]"
                        outlined
                        dense
                    />
                </div>
                <div class="col-12 col-md-6">
                    <q-select
                        v-model="expenseForm.expenseCategoryId"
                        :options="categoryOptions"
                        label="Categoría"
                        emit-value
                        map-options
                        lazy-rules
                        :rules="[(val) => !!val || 'Requerido']"
                        outlined
                        dense
                    />
                </div>
                <div class="col-12 col-md-6">
                    <q-select
                        v-model="expenseForm.warehouseId"
                        :options="warehouseOptions"
                        label="Almacén"
                        emit-value
                        map-options
                        lazy-rules
                        :rules="[(val) => !!val || 'Requerido']"
                        outlined
                        dense
                    />
                </div>
                <div class="col-12">
                    <q-input
                        v-model="expenseForm.details"
                        label="Detalles / Referencia"
                        type="textarea"
                        autogrow
                        outlined
                        dense
                    />
                </div>
            </div>
        </FormDialog>
        <!-- Category Dialog -->
        <FormDialog
            v-model="showCategoryDialog"
            :title="isEditCategory ? 'Editar Categoría' : 'Nueva Categoría'"
            @submit="saveCategory"
            :saving="saving"
        >
            <q-input
                v-model="categoryForm.name"
                label="Nombre"
                lazy-rules
                :rules="[(val) => !!val || 'Requerido']"
                outlined
                dense
            />
            <q-input
                v-model="categoryForm.description"
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
import { ref, onMounted, reactive } from "vue";
import { useQuasar } from "quasar";
import { expenseService } from "@/services/expense.service";
import { warehouseService } from "@/services/warehouse.service";
import { useConfirm } from "@/composables/useConfirm";
import type { Expense, ExpenseCategory, ExpenseReadDto } from "@/types";
import FormDialog from "@/components/FormDialog.vue";

import { useCurrency } from "@/composables/useCurrency";

const $q = useQuasar();
const { confirmDelete } = useConfirm();
const tab = ref("expenses");
const { formatCurrency, currencySymbol } = useCurrency();

const loadingExpenses = ref(false);
const loadingCategories = ref(false);
const saving = ref(false);

// Data
const expenses = ref<ExpenseReadDto[]>([]);
const categories = ref<ExpenseCategory[]>([]);
const warehouseOptions = ref<any[]>([]);
const categoryOptions = ref<any[]>([]);

// Forms
const showExpenseDialog = ref(false);
const isEditExpense = ref(false);
const expenseForm = reactive<Expense>({
    date: new Date().toISOString().substr(0, 10),
    expenseCategoryId: 0,
    warehouseId: 0,
    details: "",
    amount: 0,
});

const showCategoryDialog = ref(false);
const isEditCategory = ref(false);
const categoryForm = reactive<ExpenseCategory>({
    name: "",
    description: "",
});

// Columns
const expenseColumns = [
    {
        name: "date",
        label: "Fecha",
        field: "date",
        format: (val: string) => val.split("T")[0],
        sortable: true,
        align: "left" as const,
    },
    {
        name: "ref",
        label: "Referencia",
        field: "ref",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "categoryName",
        label: "Categoría",
        field: "categoryName",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "warehouseName",
        label: "Almacén",
        field: "warehouseName",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "amount",
        label: "Monto",
        field: "amount",
        sortable: true,
        align: "right" as const,
    },
    {
        name: "actions",
        label: "Acciones",
        field: "actions",
        align: "center" as const,
    },
];

const categoryColumns = [
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

// Methods
const fetchExpenses = async () => {
    loadingExpenses.value = true;
    try {
        const response = await expenseService.getAll();
        expenses.value = response.data;
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al cargar gastos" });
    } finally {
        loadingExpenses.value = false;
    }
};

const fetchCategories = async () => {
    loadingCategories.value = true;
    try {
        const response = await expenseService.getCategories();
        categories.value = response.data;
        categoryOptions.value = response.data.map((c) => ({
            label: c.name,
            value: c.id,
        }));
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al cargar categorías" });
    } finally {
        loadingCategories.value = false;
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

// Expense Actions
const openExpenseDialog = (expense?: any) => {
    if (expense) {
        isEditExpense.value = true;
        // Need to fetch full expense to get IDs
        expenseService.getById(expense.id).then((res) => {
            Object.assign(expenseForm, {
                ...res.data,
                date: res.data.date.split("T")[0],
            });
        });
    } else {
        isEditExpense.value = false;
        Object.assign(expenseForm, {
            id: undefined,
            date: new Date().toISOString().substr(0, 10),
            expenseCategoryId: 0,
            warehouseId: 0,
            details: "",
            amount: 0,
        });
    }
    showExpenseDialog.value = true;
};

const saveExpense = async () => {
    saving.value = true;
    try {
        if (isEditExpense.value) {
            await expenseService.update(expenseForm.id!, expenseForm);
            $q.notify({ color: "positive", message: "Gasto actualizado" });
        } else {
            await expenseService.create(expenseForm);
            $q.notify({ color: "positive", message: "Gasto registrado" });
        }
        showExpenseDialog.value = false;
        fetchExpenses();
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al guardar gasto" });
    } finally {
        saving.value = false;
    }
};

const confirmDeleteExpense = (expense: ExpenseReadDto) => {
    confirmDelete(expense.ref, async () => {
        await expenseService.delete(expense.id);
        fetchExpenses();
    });
};

// Category Actions
const openCategoryDialog = (category?: ExpenseCategory) => {
    if (category) {
        isEditCategory.value = true;
        Object.assign(categoryForm, category);
    } else {
        isEditCategory.value = false;
        Object.assign(categoryForm, {
            id: undefined,
            name: "",
            description: "",
        });
    }
    showCategoryDialog.value = true;
};

const saveCategory = async () => {
    saving.value = true;
    try {
        if (isEditCategory.value) {
            await expenseService.updateCategory(categoryForm.id!, categoryForm);
            $q.notify({ color: "positive", message: "Categoría actualizada" });
        } else {
            await expenseService.createCategory(categoryForm);
            $q.notify({ color: "positive", message: "Categoría creada" });
        }
        showCategoryDialog.value = false;
        fetchCategories();
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al guardar categoría" });
    } finally {
        saving.value = false;
    }
};

const confirmDeleteCategory = (category: ExpenseCategory) => {
    confirmDelete(category.name, async () => {
        await expenseService.deleteCategory(category.id!);
        fetchCategories();
    });
};

onMounted(() => {
    fetchExpenses();
    fetchCategories();
    fetchWarehouses();
});
</script>
