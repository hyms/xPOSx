<template>
    <q-page padding>
        <div class="row q-col-gutter-sm">
            <div class="col-12">
                <q-table
                    title="Categorías"
                    :rows="categories"
                    :columns="columns"
                    row-key="id"
                    :loading="loading"
                    :filter="filter"
                >
                    <template v-slot:top-right>
                        <BaseSearch @search="filter = $event" />
                        <q-btn
                            color="primary"
                            label="Nueva Categoría"
                            icon="add"
                            @click="openDialog()"
                            class="q-ml-md"
                        />
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
                                @click="confirmDelete(props.row)"
                            />
                        </q-td>
                    </template>
                </q-table>
            </div>
        </div>

        <CategoryFormDialog
            v-model="showDialog"
            :initial-data="selectedCategoryData"
            @saved="fetchCategories"
        />
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useQuasar } from "quasar";
import { categoryService } from "@/services/category.service";
import type { Category } from "@/types";
import BaseSearch from "@/components/base/BaseSearch.vue";
import CategoryFormDialog from "./components/CategoryFormDialog.vue";

const $q = useQuasar();
const categories = ref<Category[]>([]);
const loading = ref(true);
const filter = ref("");
const showDialog = ref(false);
const selectedCategoryData = ref<any>(null);

const columns = [
    {
        name: "id",
        label: "ID",
        field: "id",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "code",
        label: "Código",
        field: "code",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "name",
        label: "Nombre",
        field: "name",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "actions",
        label: "Acciones",
        field: "actions",
        align: "center" as const,
    },
];

const fetchCategories = async () => {
    loading.value = true;
    try {
        const response = await categoryService.getAll();
        categories.value = response.data;
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al cargar categorías" });
    } finally {
        loading.value = false;
    }
};

const openDialog = (category?: Category) => {
    selectedCategoryData.value = category ? { ...category } : null;
    showDialog.value = true;
};

const confirmDelete = (category: Category) => {
    $q.dialog({
        title: "Confirmar eliminación",
        message: `¿Eliminar la categoría ${category.name}?`,
        cancel: true,
        persistent: true,
    }).onOk(async () => {
        try {
            await categoryService.delete(category.id!);
            $q.notify({ color: "positive", message: "Categoría eliminada" });
            fetchCategories();
        } catch (error) {
            $q.notify({
                color: "negative",
                message: "Error al eliminar categoría",
            });
        }
    });
};

onMounted(fetchCategories);
</script>
