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
        <FormDialog
            v-model="showDialog"
            :title="isEdit ? 'Editar Proveedor' : 'Nuevo Proveedor'"
            @submit="saveProvider"
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
                        v-model.number="formData.code"
                        label="Código"
                        type="number"
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
                <div class="col-12 col-md-6">
                    <q-input
                        v-model="formData.country"
                        label="País"
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
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from "vue";
import { useQuasar } from "quasar";
import { providerService } from "@/services/provider.service";
import type { Provider } from "@/types";
import FormDialog from "@/components/FormDialog.vue";

const $q = useQuasar();
const providers = ref<Provider[]>([]);
const loading = ref(true);
const saving = ref(false);
const showDialog = ref(false);
const isEdit = ref(false);

const formData = reactive<Provider>({
    name: "",
    code: 0,
    email: "",
    phone: "",
    city: "",
    country: "",
    address: "",
});

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
    if (provider) {
        isEdit.value = true;
        Object.assign(formData, { ...provider });
    } else {
        isEdit.value = false;
        Object.assign(formData, {
            name: "",
            code: 0,
            email: "",
            phone: "",
            city: "",
            country: "",
            address: "",
        });
    }
    showDialog.value = true;
};

const saveProvider = async () => {
    saving.value = true;
    try {
        if (isEdit.value) {
            await providerService.update(formData.id!, formData);
            $q.notify({ color: "positive", message: "Proveedor actualizado" });
        } else {
            await providerService.create(formData);
            $q.notify({ color: "positive", message: "Proveedor creado" });
        }
        showDialog.value = false;
        fetchProviders();
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al guardar proveedor" });
    } finally {
        saving.value = false;
    }
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
