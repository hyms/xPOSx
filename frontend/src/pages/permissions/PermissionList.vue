<template>
    <q-page padding>
        <div class="row q-col-gutter-sm">
            <div class="col-12">
                <q-table
                    title="Permisos"
                    :rows="permissions"
                    :columns="columns"
                    row-key="id"
                    :loading="loading"
                    >
                    <template v-slot:item="props">
                        <div class="q-pa-xs col-xs-12 col-sm-6 col-md-4">
                            <q-card flat bordered>
                                <q-card-section>
                                    <div
                                        class="text-subtitle2 text-weight-bold"
                                    >
                                        {{ props.row.name }}
                                    </div>
                                    <div class="text-caption text-grey">
                                        {{ props.row.description }}
                                    </div>
                                </q-card-section>
                            </q-card>
                        </div>
                    </template>
                </q-table>
            </div>
        </div>
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import api from "@/api";

const permissions = ref<any[]>([]);
const loading = ref(true);

const columns = [
    {
        name: "id",
        label: "ID",
        field: "id",
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
        name: "description",
        label: "Descripción",
        field: "description",
        sortable: true,
        align: "left" as const,
    },
];

const fetchPermissions = async () => {
    loading.value = true;
    try {
        const response = await api.get("/permissions");
        permissions.value = response.data;
    } catch (error) {
        console.error("Error fetching permissions:", error);
    } finally {
        loading.value = false;
    }
};

onMounted(fetchPermissions);
</script>
