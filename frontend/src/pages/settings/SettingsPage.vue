<template>
    <q-page padding>
        <div class="row q-col-gutter-sm justify-center">
            <div class="col-12 col-md-10">
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
                        <q-tab name="general" label="General" icon="settings" />
                        <q-tab name="identity" label="Identidad" icon="palette" />
                        <q-tab
                            name="notifications"
                            label="Notificaciones"
                            icon="notifications"
                        />
                        <q-tab name="currency" label="Moneda" icon="payments" />
                        <q-tab name="mail" label="Correo" icon="mail" />
                        <q-tab name="backup" label="Respaldos" icon="cloud_sync" />
                    </q-tabs>

                    <q-separator />

                    <q-tab-panels v-model="tab" animated>
                        <q-tab-panel name="general">
                            <q-form @submit="saveSettings" class="q-pa-md">
                                <div class="row q-col-gutter-sm">
                                    <div class="col-12 col-md-6">
                                        <q-input
                                            v-model="formData.companyName"
                                            label="Nombre de la Empresa"
                                            lazy-rules
                                            :rules="[
                                                (val) =>
                                                    (val && val.length > 0) ||
                                                    'Campo requerido',
                                            ]"
                                            outlined
                                            dense
                                        />
                                    </div>
                                    <div class="col-12 col-md-6">
                                        <q-input
                                            v-model="formData.email"
                                            label="Email de la Empresa"
                                            type="email"
                                            outlined
                                            dense
                                        />
                                    </div>
                                    <div class="col-12 col-md-6">
                                        <q-input
                                            v-model="formData.companyPhone"
                                            label="Teléfono de la Empresa"
                                            outlined
                                            dense
                                        />
                                    </div>
                                    <div class="col-12 col-md-6">
                                        <q-input
                                            v-model="formData.version"
                                            label="Versión del Sistema"
                                            readonly
                                            outlined
                                            dense
                                        />
                                    </div>
                                    <div class="col-12">
                                        <q-input
                                            v-model="formData.companyAddress"
                                            label="Dirección de la Empresa"
                                            type="textarea"
                                            autogrow
                                            outlined
                                            dense
                                        />
                                    </div>
                                    <div class="col-12 col-md-6">
                                        <q-input
                                            v-model.number="formData.days"
                                            label="Días de Validez/Suscripción"
                                            type="number"
                                            outlined
                                            dense
                                        />
                                    </div>
                                </div>

                                <div class="row justify-end q-mt-md">
                                    <q-btn
                                        label="Guardar Cambios"
                                        color="primary"
                                        type="submit"
                                        :loading="saving"
                                    />
                                </div>
                            </q-form>
                        </q-tab-panel>

                        <q-tab-panel name="identity">
                            <div class="row q-col-gutter-md justify-center">
                                <!-- Logo Upload -->
                                <div class="col-12 col-sm-6">
                                    <q-card class="glass-card q-pa-lg text-center">
                                        <div class="text-h6 q-mb-md text-primary">Logo Principal</div>
                                        <div class="preview-container q-mb-md">
                                            <q-img
                                                :src="settingsStore.getLogoUrl"
                                                class="rounded-borders shadow-2"
                                                style="max-height: 150px; max-width: 100%; object-fit: contain"
                                            >
                                                <template v-slot:error>
                                                    <div class="absolute-full flex flex-center bg-grey-3 text-grey-7">
                                                        Sin Logo
                                                    </div>
                                                </template>
                                            </q-img>
                                        </div>
                                        <q-file
                                            v-model="logoFile"
                                            label="Subir Logo (.png, .jpg)"
                                            outlined
                                            dense
                                            accept=".png, .jpg, .jpeg"
                                            @update:model-value="uploadMedia('logo')"
                                            :loading="uploadingLogo"
                                        >
                                            <template v-slot:prepend>
                                                <q-icon name="image" />
                                            </template>
                                        </q-file>
                                        <div class="text-caption q-mt-sm text-grey-7">Máx 2MB (PNG, JPG)</div>
                                    </q-card>
                                </div>

                                <!-- Favicon Upload -->
                                <div class="col-12 col-sm-6">
                                    <q-card class="glass-card q-pa-lg text-center">
                                        <div class="text-h6 q-mb-md text-primary">Favicon</div>
                                        <div class="preview-container q-mb-md flex flex-center">
                                            <div class="favicon-preview shadow-2 rounded-borders q-pa-sm bg-white">
                                                <q-img
                                                    :src="settingsStore.getFaviconUrl"
                                                    style="width: 48px; height: 48px"
                                                >
                                                    <template v-slot:error>
                                                        <q-icon name="help" size="32px" color="grey-5" />
                                                    </template>
                                                </q-img>
                                            </div>
                                        </div>
                                        <q-file
                                            v-model="faviconFile"
                                            label="Subir Favicon (.png, .ico)"
                                            outlined
                                            dense
                                            accept=".png, .ico"
                                            @update:model-value="uploadMedia('favicon')"
                                            :loading="uploadingFavicon"
                                        >
                                            <template v-slot:prepend>
                                                <q-icon name="add_photo_alternate" />
                                            </template>
                                        </q-file>
                                        <div class="text-caption q-mt-sm text-grey-7">Máx 500KB (PNG, ICO)</div>
                                    </q-card>
                                </div>
                            </div>
                        </q-tab-panel>

                        <q-tab-panel name="notifications">
                            <div class="text-subtitle1 q-mb-md">
                                Configuración de Alertas
                            </div>
                            <q-list bordered separator>
                                <q-item
                                    v-for="setting in notifSettings"
                                    :key="setting.key"
                                    tag="label"
                                    v-ripple
                                >
                                    <q-item-section avatar>
                                        <q-icon
                                            :name="
                                                getCategoryIcon(
                                                    setting.category,
                                                )
                                            "
                                            color="primary"
                                        />
                                    </q-item-section>
                                    <q-item-section>
                                        <q-item-label>{{
                                            setting.label
                                        }}</q-item-label>
                                        <q-item-label caption>{{
                                            setting.category
                                        }}</q-item-label>
                                    </q-item-section>
                                    <q-item-section side>
                                        <q-toggle
                                            v-model="setting.value"
                                            color="primary"
                                            @update:model-value="
                                                updateNotifSetting(setting)
                                            "
                                        />
                                    </q-item-section>
                                </q-item>
                            </q-list>
                            <div
                                v-if="notifSettings.length === 0"
                                class="text-center q-pa-lg text-grey"
                            >
                                Cargando configuración...
                            </div>
                        </q-tab-panel>

                        <q-tab-panel name="currency">
                            <div class="text-subtitle1 q-mb-md">
                                Configuración de Moneda
                            </div>
                            <q-form
                                @submit="saveCurrencySettings"
                                class="q-pa-md"
                            >
                                <q-input
                                    v-model="currencySettings.code"
                                    label="Código de Moneda (USD, EUR)"
                                    outlined
                                    dense
                                />
                                <q-input
                                    v-model="currencySettings.symbol"
                                    label="Símbolo de Moneda ($, €)"
                                    outlined
                                    dense
                                />
                                <div class="row justify-end q-mt-md">
                                    <q-btn
                                        label="Guardar Cambios"
                                        color="primary"
                                        type="submit"
                                        :loading="saving"
                                    />
                                </div>
                            </q-form>
                        </q-tab-panel>

<q-tab-panel name="mail">
                            <div class="text-subtitle1 q-mb-md">
                                Configuración de Correo Electrónico
                            </div>
                            <q-form @submit="saveMailSettings" class="q-pa-md">
                                <q-input
                                    v-model="mailSettings.host"
                                    label="Host SMTP"
                                    outlined
                                    dense
                                />
                                <q-input
                                    v-model="mailSettings.port"
                                    label="Puerto SMTP"
                                    type="number"
                                    outlined
                                    dense
                                />
                                <q-input
                                    v-model="mailSettings.username"
                                    label="Usuario SMTP"
                                    outlined
                                    dense
                                />
                                <q-input
                                    v-model="mailSettings.password"
                                    label="Contraseña SMTP"
                                    type="password"
                                    outlined
                                    dense
                                />
                                <q-input
                                    v-model="mailSettings.encryption"
                                    label="Encriptación (tls, ssl)"
                                    outlined
                                    dense
                                />
                                <q-input
                                    v-model="mailSettings.fromAddress"
                                    label="Desde Email"
                                    type="email"
                                    outlined
                                    dense
                                />
                                <q-input
                                    v-model="mailSettings.fromName"
                                    label="Desde Nombre"
                                    outlined
                                    dense
                                />
                                <div class="row justify-end q-mt-md">
                                    <q-btn
                                        label="Guardar Cambios"
                                        color="primary"
                                        type="submit"
                                        :loading="saving"
                                    />
                                </div>
                            </q-form>
                        </q-tab-panel>

                        <q-tab-panel name="backup">
                             <div class="row q-col-gutter-md justify-center">
                                <div class="col-12 col-md-8">
                                    <q-card class="glass-card q-pa-lg">
                                        <div class="text-h6 text-primary q-mb-md">Gestión de Respaldo</div>
                                        <div class="text-body1 q-mb-md">
                                            Genera una copia de seguridad completa de la base de datos o restaura el sistema desde un archivo de respaldo.
                                        </div>

                                        <q-separator class="q-my-lg" />

                                        <div class="row items-center q-col-gutter-md">
                                            <div class="col-12 col-sm-6">
                                                <div class="text-subtitle1 q-mb-sm">Copia de Seguridad</div>
                                                <div class="text-caption q-mb-md">Descarga todos los datos del sistema en un archivo seguro .xpos.</div>
                                            </div>
                                            <div class="col-12 col-sm-6 text-right">
                                                <q-btn
                                                    label="Generar Copia"
                                                    color="primary"
                                                    icon="cloud_download"
                                                    :loading="backupLoading"
                                                    @click="handleBackup"
                                                    unelevated
                                                    class="full-width"
                                                />
                                            </div>
                                        </div>

                                        <q-separator class="q-my-lg" />

                                        <div class="row items-center q-col-gutter-md">
                                             <div class="col-12 col-sm-6">
                                                <div class="text-subtitle1 q-mb-sm">Restaurar Sistema</div>
                                                <div class="text-caption q-mb-md">Sube un archivo .xpos para restaurar o actualizar la base de datos.</div>
                                             </div>
                                            <div class="col-12 col-sm-6">
                                                 <q-file
                                                    v-model="restoreFile"
                                                    label="Seleccionar archivo .xpos"
                                                    outlined
                                                    dense
                                                    accept=".xpos"
                                                    class="q-mb-sm"
                                                >
                                                    <template v-slot:prepend>
                                                        <q-icon name="attach_file" />
                                                    </template>
                                                </q-file>
                                                <q-btn
                                                    label="Restaurar"
                                                    color="deep-orange"
                                                    icon="settings_backup_restore"
                                                    :loading="restoreLoading"
                                                    :disable="!restoreFile"
                                                    @click="confirmRestore"
                                                    unelevated
                                                    class="full-width"
                                                />
                                            </div>
                                        </div>

                                    </q-card>
                                </div>
                            </div>
                        </q-tab-panel>
                    </q-tab-panels>
                </q-card>
            </div>
        </div>
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, watch } from "vue";
import { useQuasar } from "quasar";
import { useSettingsStore } from "@/stores/settings";
import { api } from "boot/axios";
import { notificationService } from "@/services/notification.service";
import { currencyService } from "@/services/currency.service";
import { mailService } from "@/services/mail.service";
import type {
    NotificationSetting,
    CurrencySetting,
    MailSetting,
} from "@/types";

const settingsStore = useSettingsStore();
const $q = useQuasar();

const saving = ref(false);
const tab = ref("general");
const logoFile = ref<File | null>(null);
const faviconFile = ref<File | null>(null);
const uploadingLogo = ref(false);
const uploadingFavicon = ref(false);
const backupLoading = ref(false);
const restoreLoading = ref(false);
const restoreFile = ref<File | null>(null);

const formData = reactive({
    id: 0,
    companyName: "",
    email: "",
    companyPhone: "",
    companyAddress: "",
    version: "1.0.0",
    days: 365,
});

const notifSettings = ref<NotificationSetting[]>([]);

const currencySettings = reactive<CurrencySetting>({
    id: 0,
    code: "",
    symbol: "",
});

const mailSettings = reactive<MailSetting>({
    id: 0,
    host: "",
    port: 587,
    username: "",
    password: "",
    encryption: "tls",
    fromAddress: "",
    fromName: "",
});

watch(
    () => settingsStore.settings,
    (newSettings) => {
        if (newSettings) {
            formData.id = newSettings.id;
            formData.companyName = newSettings.companyName || "";
            formData.email = newSettings.email || "";
            formData.companyPhone = newSettings.companyPhone || "";
            formData.companyAddress = newSettings.companyAddress || "";
            formData.version = newSettings.version || "1.0.0";
            formData.days = (newSettings as any).days || 365;
        }
    },
    { immediate: true, deep: true },
);

const uploadMedia = async (type: "logo" | "favicon") => {
    const file = type === "logo" ? logoFile.value : faviconFile.value;
    if (!file) return;

    if (type === "logo") uploadingLogo.value = true;
    else uploadingFavicon.value = true;

    try {
        const formDataUpload = new FormData();
        formDataUpload.append("file", file);
        formDataUpload.append("type", type);

        const response = await api.post("/settings/upload-media", formDataUpload, {
            headers: { "Content-Type": "multipart/form-data" },
        });

        $q.notify({
            color: "positive",
            message: `${type === "logo" ? "Logo" : "Favicon"} actualizado correctamente`,
            icon: "check",
        });

        if (type === "logo") {
            logoFile.value = null;
        } else {
            faviconFile.value = null;
        }
        
        // El interceptor ya actualizará la versión, pero forzamos el refresco local
        settingsStore.fetchSettings();

    } catch (error: any) {
        $q.notify({
            color: "negative",
            message: error.response?.data || "Error al subir el archivo",
            icon: "error",
        });
    } finally {
        if (type === "logo") uploadingLogo.value = false;
        else uploadingFavicon.value = false;
    }
};

const saveSettings = async () => {
    saving.value = true;
    try {
        await api.put(`/settings/${formData.id}`, {
            ...settingsStore.settings,
            ...formData
        });
        $q.notify({
            color: "positive",
            message: "Configuración general actualizada",
        });
        settingsStore.fetchSettings();
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al guardar la configuración",
        });
    } finally {
        saving.value = false;
    }
};

const fetchNotifSettings = async () => {
    try {
        const response = await notificationService.getSettings();
        notifSettings.value = response.data;
    } catch (error) {
        console.error("Error fetching notification settings:", error);
    }
};

const updateNotifSetting = async (setting: NotificationSetting) => {
    try {
        await notificationService.updateSetting(setting.key, setting.value);
        $q.notify({
            color: "positive",
            message: `${setting.label} ${setting.value ? "activada" : "desactivada"}`,
            timeout: 1000,
        });
    } catch (error) {
        setting.value = !setting.value; // Revert
        $q.notify({
            color: "negative",
            message: "Error al actualizar configuración de notificación",
        });
    }
};

const fetchCurrencySettings = async () => {
    try {
        const response = await currencyService.get();
        if (response.data) {
            Object.assign(currencySettings, response.data);
        }
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al cargar configuración de moneda",
        });
    }
};

const saveCurrencySettings = async () => {
    saving.value = true;
    try {
        await currencyService.update(currencySettings);
        $q.notify({
            color: "positive",
            message: "Configuración de moneda actualizada correctamente",
        });
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al guardar configuración de moneda",
        });
    } finally {
        saving.value = false;
    }
};

const fetchMailSettings = async () => {
    try {
        const response = await mailService.get();
        if (response.data) {
            Object.assign(mailSettings, response.data);
        }
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al cargar configuración de correo",
        });
    }
};

const saveMailSettings = async () => {
    saving.value = true;
    try {
        await mailService.update(mailSettings);
        $q.notify({
            color: "positive",
            message: "Configuración de correo actualizada correctamente",
        });
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al guardar configuración de correo",
        });
    } finally {
        saving.value = false;
    }
};

const getCategoryIcon = (category: string) => {
    switch (category) {
        case "Inventory": return "inventory_2";
        case "Sales": return "shopping_cart";
        default: return "notifications";
    }
};

const handleBackup = async () => {
    backupLoading.value = true;
    try {
        const response = await api.get('/settings/backup', {
            responseType: 'blob', // Importante para recibir el archivo
        });

        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        
        const contentDisposition = response.headers['content-disposition'];
        let fileName = 'xpos_backup.xpos';
        if (contentDisposition) {
            const fileNameMatch = contentDisposition.match(/filename="?(.+)"?/);
            if (fileNameMatch.length === 2) fileName = fileNameMatch[1];
        }
        
        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);

        $q.notify({
            color: 'positive',
            message: 'Copia de seguridad generada correctamente.',
            icon: 'check_circle',
        });

    } catch (error: any) {
        $q.notify({
            color: 'negative',
            message: error.response?.data?.message || 'Error al generar el backup.',
            icon: 'error',
        });
    } finally {
        backupLoading.value = false;
    }
};

const confirmRestore = () => {
    $q.dialog({
        title: 'Confirmar Restauración',
        message: 'Esta acción reescribirá o actualizará datos existentes y puede alterar la numeración actual. ¿Deseas continuar?',
        cancel: true,
        persistent: true,
        ok: {
            label: 'Continuar',
            color: 'deep-orange',
            unelevated: true,
        },
        cancel: {
            label: 'Cancelar',
            color: 'grey',
            flat: true,
        }
    }).onOk(() => {
        handleRestore();
    });
};

const handleRestore = async () => {
    if (!restoreFile.value) return;

    restoreLoading.value = true;
    const formData = new FormData();
    formData.append('file', restoreFile.value);

    try {
        const response = await api.post('/settings/restore', formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });

        $q.notify({
            color: 'positive',
            message: response.data.message || 'Sistema restaurado con éxito.',
            icon: 'verified',
        });
        
        restoreFile.value = null;

    } catch (error: any) {
        $q.notify({
            color: 'negative',
            message: error.response?.data || 'Error durante la restauración. La base de datos no ha sido modificada.',
            icon: 'gpp_bad',
            timeout: 7000,
        });
    } finally {
        restoreLoading.value = false;
    }
};

onMounted(() => {
    settingsStore.fetchSettings();
    fetchNotifSettings();
    fetchCurrencySettings();
    fetchMailSettings();
});
</script>

<style lang="scss" scoped>
.glass-card {
    background: rgba(255, 255, 255, 0.2);
    backdrop-filter: blur(10px);
    border: 1px solid rgba(255, 255, 255, 0.3);
    box-shadow: 0 8px 32px 0 rgba(31, 38, 135, 0.15);
    border-radius: 16px;
}

.preview-container {
    min-height: 150px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.favicon-preview {
    border: 1px solid #eee;
}
</style>
