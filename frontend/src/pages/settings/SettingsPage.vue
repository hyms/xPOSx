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
              <q-form @submit="saveSettings" class="q-gutter-md">
                <div class="row q-col-gutter-md">
                  <div class="col-12 col-md-6">
                    <q-input
                      v-model="formData.companyName"
                      label="Nombre de la Empresa"
                      lazy-rules
                      :rules="[ val => val && val.length > 0 || 'Campo requerido']"
                    />
                  </div>
                  <div class="col-12 col-md-6">
                    <q-input
                      v-model="formData.email"
                      label="Email de la Empresa"
                      type="email"
                    />
                  </div>
                  <div class="col-12 col-md-6">
                    <q-input
                      v-model="formData.companyPhone"
                      label="Teléfono de la Empresa"
                    />
                  </div>
                  <div class="col-12 col-md-6">
                    <q-input
                      v-model="formData.version"
                      label="Versión del Sistema"
                      readonly
                    />
                  </div>
                  <div class="col-12">
                    <q-input
                      v-model="formData.companyAddress"
                      label="Dirección de la Empresa"
                      type="textarea"
                      autogrow
                    />
                  </div>
                  <div class="col-12 col-md-6">
                    <q-input
                      v-model.number="formData.days"
                      label="Días de Validez/Suscripción"
                      type="number"
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
              <q-form @submit="saveCurrencySettings" class="q-gutter-md">
                <q-input v-model="currencySettings.code" label="Código de Moneda (USD, EUR)" />
                <q-input v-model="currencySettings.symbol" label="Símbolo de Moneda ($, €)" />
                <div class="row justify-end q-mt-md">
                  <q-btn label="Guardar Cambios" color="primary" type="submit" :loading="saving" />
                </div>
              </q-form>
            </q-tab-panel>

            <q-tab-panel name="mail">
              <div class="text-subtitle1 q-mb-md">Configuración de Correo Electrónico</div>
              <q-form @submit="saveMailSettings" class="q-gutter-md">
                <q-input v-model="mailSettings.host" label="Host SMTP" />
                <q-input v-model="mailSettings.port" label="Puerto SMTP" type="number" />
                <q-input v-model="mailSettings.username" label="Usuario SMTP" />
                <q-input v-model="mailSettings.password" label="Contraseña SMTP" type="password" />
                <q-input v-model="mailSettings.encryption" label="Encriptación (tls, ssl)" />
                <q-input v-model="mailSettings.fromAddress" label="Desde Email" type="email" />
                <q-input v-model="mailSettings.fromName" label="Desde Nombre" />
                <div class="row justify-end q-mt-md">
                  <q-btn label="Guardar Cambios" color="primary" type="submit" :loading="saving" />
                </div>
              </q-form>
            </q-tab-panel>

            <q-tab-panel name="sms">
              <div class="text-subtitle1 q-mb-md">Configuración de SMS</div>
              <q-form @submit="saveSmsSettings" class="q-gutter-md">
                <q-input v-model="smsSettings.sid" label="Twilio SID" />
                <q-input v-model="smsSettings.token" label="Twilio Token" type="password" />
                <q-input v-model="smsSettings.fromNumber" label="Número Twilio" />
                <div class="row justify-end q-mt-md">
                  <q-btn label="Guardar Cambios" color="primary" type="submit" :loading="saving" />
                </div>
              </q-form>
            </q-tab-panel>

            <q-tab-panel name="payment_gateways">
              <div class="text-subtitle1 q-mb-md">Configuración de Pasarelas de Pago</div>
              <q-form @submit="savePaymentGatewaySettings" class="q-gutter-md">
                <q-input v-model="paymentGatewaySettings.stripeKey" label="Stripe Public Key" />
                <q-input v-model="paymentGatewaySettings.stripeSecret" label="Stripe Secret Key" type="password" />
                <q-input v-model="paymentGatewaySettings.paypalClientId" label="PayPal Client ID" />
                <q-input v-model="paymentGatewaySettings.paypalClientSecret" label="PayPal Client Secret" type="password" />
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
import { ref, onMounted, reactive } from 'vue'
import { useQuasar } from 'quasar'
import { settingService } from '@/services/setting.service';
import { notificationService } from '@/services/notification.service';
import { currencyService } from '@/services/currency.service';
import { mailService } from '@/services/mail.service';
import { smsService } from '@/services/sms.service';
import { paymentGatewayService } from '@/services/paymentGateway.service';
import type { Setting, NotificationSetting, CurrencySetting, MailSetting, SmsSetting, PaymentGatewaySetting } from '@/types'

const $q = useQuasar()
const saving = ref(false)
const tab = ref('general')
const notifSettings = ref<NotificationSetting[]>([])

const formData = reactive<Setting>({
  id: 0,
  companyName: '',
  email: '',
  companyPhone: '',
  companyAddress: '',
  currency: 'USD',
  version: '1.0.0',
  days: 365
})

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

const fetchSettings = async () => {
  try {
    const response = await settingService.get()
    if (response.data) {
      Object.assign(formData, response.data)
    }
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar configuración' })
  }
}

const fetchNotifSettings = async () => {
  try {
    const response = await notificationService.getSettings()
    notifSettings.value = response.data
  } catch (error) {
    console.error('Error fetching notification settings:', error)
  }
}

const saveSettings = async () => {
  saving.value = true
  try {
    await settingService.update(formData)
    $q.notify({ color: 'positive', message: 'Configuración actualizada correctamente' })
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar configuración' })
  } finally {
    saving.value = false
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
  fetchSettings()
  fetchNotifSettings()
  fetchCurrencySettings()
  fetchMailSettings()
  fetchSmsSettings()
  fetchPaymentGatewaySettings()
})
</script>
