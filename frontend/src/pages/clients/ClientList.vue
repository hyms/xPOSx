<template>
    <q-page padding>
        <div class="row q-col-gutter-sm">
            <div class="col-12">
                <q-table
                    title="Clientes"
                    :rows="clients"
                    :columns="columns"
                    row-key="id"
                    :loading="loading"
                    >
                    <template v-slot:top-right>
                        <q-btn
                            color="primary"
                            label="Nuevo"
                            icon="add"
                            @click="openDialog()"
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
                                            {{ props.row.name }}
                                        </div>
                                        <div class="text-caption text-grey">
                                            NIT/CI: {{ props.row.nitCi }}
                                        </div>
                                    </div>
                                    <div class="q-mt-sm row items-center">
                                        <q-icon
                                            name="phone"
                                            size="xs"
                                            color="grey"
                                            class="q-mr-xs"
                                        />
                                        <div class="text-caption">
                                            {{ props.row.phone }}
                                        </div>
                                    </div>
                                    <div
                                        class="row items-center q-mt-xs"
                                        v-if="props.row.email"
                                    >
                                        <q-icon
                                            name="email"
                                            size="xs"
                                            color="grey"
                                            class="q-mr-xs"
                                        />
                                        <div class="text-caption">
                                            {{ props.row.email }}
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
                                        @click="openDialog(props.row)"
                                    />
                                    <q-btn
                                        flat
                                        round
                                        color="negative"
                                        icon="delete"
                                        size="sm"
                                        @click="confirmDelete(props.row)"
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

        <!-- Client Dialog -->
        <ClientFormDialog
            v-model="showDialog"
            :initial-data="selectedClientData"
            @saved="fetchClients"
        />
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useQuasar } from "quasar";
import { clientService } from "@/services/client.service";
import type { Client } from "@/types";
import ClientFormDialog from "./components/ClientFormDialog.vue";

const $q = useQuasar();
const clients = ref<Client[]>([]);
const loading = ref(true);
const showDialog = ref(false);
const selectedClientData = ref<any>(null);

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
        name: "nitCi",
        label: "NIT/CI",
        field: "nitCi",
        format: (val: string) => val || "",
        align: "left" as const,
    },
    {
        name: "phone",
        label: "Teléfono",
        field: "phone",
        align: "left" as const,
    },
    {
        name: "actions",
        label: "Acciones",
        field: "actions",
        align: "center" as const,
    },
];

const fetchClients = async () => {
    loading.value = true;
    try {
        const response = await clientService.getAll();
        clients.value = response.data;
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al cargar clientes" });
    } finally {
        loading.value = false;
    }
};

const openDialog = (client?: Client) => {
    selectedClientData.value = client ? { ...client } : null;
    showDialog.value = true;
};

const confirmDelete = (client: Client) => {
    $q.dialog({
        title: "Confirmar eliminación",
        message: `¿Eliminar a ${client.name}?`,
        cancel: true,
        persistent: true,
    }).onOk(async () => {
        try {
            await clientService.delete(client.id!);
            $q.notify({ color: "positive", message: "Cliente eliminado" });
            fetchClients();
        } catch (error) {
            $q.notify({
                color: "negative",
                message: "Error al eliminar cliente",
            });
        }
    });
};

onMounted(fetchClients);
</script>
