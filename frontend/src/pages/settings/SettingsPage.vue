<template>
  <q-page padding>
    <div class="row q-col-gutter-md justify-center">
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
            <q-tab name="notifications" label="Notificaciones" icon="notifications" />
            <q-tab name="currency" label="Moneda" icon="payments" />
            <q-tab name="mail" label="Correo" icon="mail" />
            <q-tab name="sms" label="SMS" icon="sms" />
            <q-tab name="payment_gateways" label="Pasarelas Pago" icon="credit_card" />
          </q-tabs>

          <q-separator />

          <q-tab-panels v-model="tab" animated>
            <q-tab-panel name="general">
              <q-form @submit="saveSettings" class="q-pa-md">
                <div class="row q-col-gutter-md">
                  <div class="col-12 col-md-6">
                    <q-input
                      v-model="formData.companyName"
                      label="Nombre de la Empresa"
                      lazy-rules
                      :rules="[ val => val && val.length > 0 || 'Campo requerido']"
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
                  <q-btn label="Guardar Cambios" color="primary" type="submit" :loading="saving" />
                </div>
              </q-form>
            </q-tab-panel>

            <q-tab-panel name="notifications">
              <div class="text-subtitle1 q-mb-md">Configuración de Alertas</div>
              <q-list bordered separator>
                <q-item v-for="setting in notifSettings" :key="setting.key" tag="label" v-ripple>
                  <q-item-section avatar>
                    <q-icon :name="getCategoryIcon(setting.category)" color="primary" />
                  </q-item-section>
                  <q-item-section>
                    <q-item-label>{{ setting.label }}</q-item-label>
                    <q-item-label caption>{{ setting.category }}</q-item-label>
                  </q-item-section>
                  <q-item-section side>
                    <q-toggle
                      v-model="setting.value"
                      color="primary"
                      @update:model-value="updateNotifSetting(setting)"
                    />
                  </q-item-section>
                </q-item>
              </q-list>
              <div v-if="notifSettings.length === 0" class="text-center q-pa-lg text-grey">
                Cargando configuración...
              </div>
            </q-tab-panel>

            <q-tab-panel name="currency">
              <div class="text-subtitle1 q-mb-md">Configuración de Moneda</div>
              <q-form @submit="saveCurrencySettings" class="q-pa-md">
                <q-input v-model="currencySettings.code" label="Código de Moneda (USD, EUR)" outlined dense />
                <q-input v-model="currencySettings.symbol" label="Símbolo de Moneda ($, €)" outlined dense />
                <div class="row justify-end q-mt-md">
                  <q-btn label="Guardar Cambios" color="primary" type="submit" :loading="saving" />
                </div>
              </q-form>
            </q-tab-panel>

            <q-tab-panel name="mail">
              <div class="text-subtitle1 q-mb-md">Configuración de Correo Electrónico</div>
              <q-form @submit="saveMailSettings" class="q-pa-md">
                <q-input v-model="mailSettings.host" label="Host SMTP" outlined dense />
                <q-input v-model="mailSettings.port" label="Puerto SMTP" type="number" outlined dense />
                <q-input v-model="mailSettings.username" label="Usuario SMTP" outlined dense />
                <q-input v-model="mailSettings.password" label="Contraseña SMTP" type="password" outlined dense />
                <q-input v-model="mailSettings.encryption" label="Encriptación (tls, ssl)" outlined dense />
                <q-input v-model="mailSettings.fromAddress" label="Desde Email" type="email" outlined dense />
                <q-input v-model="mailSettings.fromName" label="Desde Nombre" outlined dense />
                <div class="row justify-end q-mt-md">
                  <q-btn label="Guardar Cambios" color="primary" type="submit" :loading="saving" />
                </div>
              </q-form>
            </q-tab-panel>

            <q-tab-panel name="sms">
              <div class="text-subtitle1 q-mb-md">Configuración de SMS</div>
              <q-form @submit="saveSmsSettings" class="q-pa-md">
                <q-input v-model="smsSettings.sid" label="Twilio SID" outlined dense />
                <q-input v-model="smsSettings.token" label="Twilio Token" type="password" outlined dense />
                <q-input v-model="smsSettings.fromNumber" label="Número Twilio" outlined dense />
                <div class="row justify-end q-mt-md">
                  <q-btn label="Guardar Cambios" color="primary" type="submit" :loading="saving" />
                </div>
              </q-form>
            </q-tab-panel>

            <q-tab-panel name="payment_gateways">
              <div class="text-subtitle1 q-mb-md">Configuración de Pasarelas de Pago</div>
              <q-form @submit="savePaymentGatewaySettings" class="q-pa-md">
                <q-input v-model="paymentGatewaySettings.stripeKey" label="Stripe Public Key" outlined dense />
                <q-input v-model="paymentGatewaySettings.stripeSecret" label="Stripe Secret Key" type="password" outlined dense />
                <q-input v-model="paymentGatewaySettings.paypalClientId" label="PayPal Client ID" outlined dense />
                <q-input v-model="paymentGatewaySettings.paypalClientSecret" label="PayPal Client Secret" type="password" outlined dense />
                <div class="row justify-end q-mt-md">
                  <q-btn label="Guardar Cambios" color="primary" type="submit" :loading="saving" />
                </div>
              </q-form>
            </q-tab-panel>
          </q-tab-panels>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, watch } from 'vue'
import { useQuasar } from 'quasar'
import { useSettings } from '@/services/settings.service';
// ... (import other services)
import { notificationService } from '@/services/notification.service';
import { currencyService } from '@/services/currency.service';
import { mailService } from '@/services/mail.service';
import { smsService } from '@/services/sms.service';
import { paymentGatewayService } from '@/services/paymentGateway.service';
import type { NotificationSetting, CurrencySetting, MailSetting, SmsSetting, PaymentGatewaySetting } from '@/types'

const { settings, updateSingleSetting } = useSettings();

const $q = useQuasar()
const saving = ref(false)
const tab = ref('general')
const notifSettings = ref<NotificationSetting[]>([])

const formData = reactive({
  companyName: '',
  email: '',
  companyPhone: '',
  companyAddress: '',
  currency: 'USD',
  version: '1.0.0',
  days: 365
})

watch(settings, (newSettings) => {
  if (newSettings) {
    formData.companyName = newSettings.companyName || ''
    formData.email = newSettings.email || ''
    formData.companyPhone = newSettings.companyPhone || ''
    formData.companyAddress = newSettings.companyAddress || ''
    formData.currency = newSettings.currency || 'USD'
    formData.version = newSettings.version || '1.0.0'
    formData.days = newSettings.days ? parseInt(newSettings.days, 10) : 365
  }
}, { immediate: true, deep: true });


const currencySettings = reactive<CurrencySetting>({
  id: 0,
  code: '',
  symbol: ''
})

const mailSettings = reactive<MailSetting>({
  id: 0,
  host: '',
  port: 587,
  username: '',
  password: '',
  encryption: 'tls',
  fromAddress: '',
  fromName: ''
})

const smsSettings = reactive<SmsSetting>({
  id: 0,
  sid: '',
  token: '',
  fromNumber: ''
})

const paymentGatewaySettings = reactive<PaymentGatewaySetting>({
  id: 0,
  stripeKey: '',
  stripeSecret: '',
  paypalClientId: '',
  paypalClientSecret: ''
})

const saveSettings = async () => {
  saving.value = true;
  try {
    const updatePromises = [];
    for (const key in formData) {
      if (Object.prototype.hasOwnProperty.call(formData, key)) {
        const localValue = formData[key as keyof typeof formData];
        const remoteValue = settings.value[key];
        
        // Check if value has changed before sending update request
        if (String(localValue) !== String(remoteValue)) {
          updatePromises.push(updateSingleSetting(key, String(localValue)));
        }
      }
    }

    await Promise.all(updatePromises);
    $q.notify({ color: 'positive', message: 'Configuración general actualizada' });
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar la configuración' });
  } finally {
    saving.value = false;
  }
};

const fetchNotifSettings = async () => {
  try {
    const response = await notificationService.getSettings()
    notifSettings.value = response.data
  } catch (error) {
    console.error('Error fetching notification settings:', error)
  }
}

const updateNotifSetting = async (setting: NotificationSetting) => {
  try {
    await notificationService.updateSetting(setting.key, setting.value)
    $q.notify({
      color: 'positive',
      message: `${setting.label} ${setting.value ? 'activada' : 'desactivada'}`,
      timeout: 1000
    })
  } catch (error) {
    setting.value = !setting.value // Revert
    $q.notify({ color: 'negative', message: 'Error al actualizar configuración de notificación' })
  }
}


// Currency Settings
const fetchCurrencySettings = async () => {
  try {
    const response = await currencyService.get()
    if (response.data) {
      Object.assign(currencySettings, response.data)
    }
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar configuración de moneda' })
  }
}

const saveCurrencySettings = async () => {
  saving.value = true
  try {
    await currencyService.update(currencySettings)
    $q.notify({ color: 'positive', message: 'Configuración de moneda actualizada correctamente' })
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar configuración de moneda' })
  } finally {
    saving.value = false
  }
}

// Mail Settings
const fetchMailSettings = async () => {
  try {
    const response = await mailService.get()
    if (response.data) {
      Object.assign(mailSettings, response.data)
    }
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar configuración de correo' })
  }
}

const saveMailSettings = async () => {
  saving.value = true
  try {
    await mailService.update(mailSettings)
    $q.notify({ color: 'positive', message: 'Configuración de correo actualizada correctamente' })
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar configuración de correo' })
  } finally {
    saving.value = false
  }
}

// SMS Settings
const fetchSmsSettings = async () => {
  try {
    const response = await smsService.get()
    if (response.data) {
      Object.assign(smsSettings, response.data)
    }
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar configuración de SMS' })
  }
}

const saveSmsSettings = async () => {
  saving.value = true
  try {
    await smsService.update(smsSettings)
    $q.notify({ color: 'positive', message: 'Configuración de SMS actualizada correctamente' })
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar configuración de SMS' })
  } finally {
    saving.value = false
  }
}

// Payment Gateway Settings
const fetchPaymentGatewaySettings = async () => {
  try {
    const response = await paymentGatewayService.get()
    if (response.data) {
      Object.assign(paymentGatewaySettings, response.data)
    }
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar configuración de pasarelas de pago' })
  }
}

const savePaymentGatewaySettings = async () => {
  saving.value = true
  try {
    await paymentGatewayService.update(paymentGatewaySettings)
    $q.notify({ color: 'positive', message: 'Configuración de pasarelas de pago actualizada correctamente' })
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar configuración de pasarelas de pago' })
  } finally {
    saving.value = false
  }
}

const getCategoryIcon = (category: string) => {
  switch (category) {
    case 'Inventory': return 'inventory_2'
    case 'Sales': return 'shopping_cart'
    default: return 'notifications'
  }
}

onMounted(() => {
  fetchNotifSettings()
  fetchCurrencySettings()
  fetchMailSettings()
  fetchSmsSettings()
  fetchPaymentGatewaySettings()
})
</script>
