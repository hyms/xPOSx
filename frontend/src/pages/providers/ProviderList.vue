<template>
    <q-page padding>
        <div class="row q-col-gutter-sm">
            <div class="col-12">
                <q-table
                    title="Proveedores"
                    :rows="providers"
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
                                            Código: {{ props.row.code }}
                                        </div>
                                    </div>
                                    <div class="q-mt-sm row items-center">
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
                                    <div class="row items-center q-mt-xs">
                                        <q-icon
                                            name="phone"
                                            size="xs"
                                            color="grey"
                                            class="q-mr-xs"
                                        />
                                        <div class="text-caption">
                                            {{ props.row.phone || "N/A" }}
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

        <!-- Provider Dialog -->
        <ProviderFormDialog
            v-model="showDialog"
            :initial-data="selectedProviderData"
            @saved="fetchProviders"
        />
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useQuasar } from "quasar";
import { providerService } from "@/services/provider.service";
import type { Provider } from "@/types";
import ProviderFormDialog from "./components/ProviderFormDialog.vue";

const $q = useQuasar();
const providers = ref<Provider[]>([]);
const loading = ref(true);
const showDialog = ref(false);
const selectedProviderData = ref<any>(null);

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
    { name: "email", label: "Email", field: "email", align: "left" as const },
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

const fetchProviders = async () => {
    loading.value = true;
    try {
        const response = await providerService.getAll();
        providers.value = response.data;
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al cargar proveedores",
        });
    } finally {
        loading.value = false;
    }
};

const openDialog = (provider?: Provider) => {
    selectedProviderData.value = provider ? { ...provider } : null;
    showDialog.value = true;
};

const confirmDelete = (provider: Provider) => {
    $q.dialog({
        title: "Confirmar eliminación",
        message: `¿Eliminar a ${provider.name}?`,
        cancel: true,
        persistent: true,
    }).onOk(async () => {
        try {
            await providerService.delete(provider.id!);
            $q.notify({ color: "positive", message: "Proveedor eliminado" });
            fetchProviders();
        } catch (error) {
            $q.notify({
                color: "negative",
                message: "Error al eliminar proveedor",
            });
        }
    });
};

onMounted(fetchProviders);
</script>
