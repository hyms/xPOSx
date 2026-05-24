<template>
  <q-page class="q-py-xl container q-mx-auto q-px-md max-width-lg" :class="$q.dark.isActive ? 'bg-dark text-white' : 'bg-grey-1 text-grey-9'">
    
    <!-- Page Header -->
    <div class="text-center q-mb-xl">
      <h3 class="text-weight-bold q-my-none text-primary">Resumen del Carrito & Pago</h3>
      <p class="text-subtitle1 text-grey-6 q-mt-xs">Completa tu información de facturación y sube tu comprobante QR para reservar tu pedido</p>
    </div>

    <!-- Empty Cart State -->
    <div v-if="cartStore.items.length === 0 && !orderSubmitted" class="text-center q-py-xl glass-card max-width-md q-mx-auto q-px-md">
      <q-icon name="shopping_cart_checkout" size="70px" color="grey-5" />
      <p class="text-h6 text-grey-6 q-mt-md">Tu carrito de compras está vacío.</p>
      <q-btn 
        label="Ir a la Tienda / Catálogo" 
        color="primary" 
        unelevated 
        no-caps 
        size="lg" 
        class="q-mt-md touch-btn-48 text-weight-bold" 
        to="/productos" 
      />
    </div>

    <!-- Order Submitted Success State -->
    <div v-else-if="orderSubmitted" class="text-center q-py-xl glass-card q-px-lg max-width-sm q-mx-auto">
      <q-icon name="check_circle" size="85px" color="positive" class="text-glow-success q-mb-md animate-bounce" />
      <h4 class="text-weight-bold q-my-none text-success">¡Pedido Registrado con Éxito!</h4>
      <p class="text-subtitle1 text-grey-6 q-mt-md">
        Su pedido con referencia <b class="text-primary">{{ submittedRef }}</b> se ha registrado y está en estado <b class="text-warning">Pendiente de Verificación</b>. 
      </p>
      <p class="text-body2 text-grey-6 q-mt-sm">
        Nuestro administrador validará el depósito y procesará el despacho a la brevedad posible. ¡Gracias por su compra!
      </p>
      <q-btn 
        label="Seguir Comprando" 
        color="primary" 
        no-caps 
        unelevated 
        size="lg" 
        class="q-mt-xl touch-btn-48 text-weight-bold" 
        to="/productos" 
      />
    </div>

    <!-- Active Checkout Flow -->
    <div v-else class="row q-col-gutter-lg">
      
      <!-- Left Column: Cart Summary and Form -->
      <div class="col-12 col-md-7">
        
        <!-- Cart Items List -->
        <q-card flat class="glass-card q-pa-md q-mb-lg">
          <span class="text-weight-bold text-h6 block q-mb-md text-primary">Detalle de Productos</span>
          
          <q-list separator class="q-pl-none">
            <q-item v-for="item in cartStore.items" :key="item.id" class="q-px-none q-py-md">
              <q-item-section avatar>
                <q-img 
                  :src="item.image || '/icons/placeholder_product.png'" 
                  width="64px" 
                  height="64px" 
                  fit="cover" 
                  class="rounded-borders" 
                />
              </q-item-section>
              
              <q-item-section>
                <q-item-label class="text-weight-bold text-subtitle1">{{ item.name }}</q-item-label>
                <q-item-label caption class="text-primary text-weight-medium">{{ formatPrice(item.price) }} c/u</q-item-label>
              </q-item-section>

              <!-- Quantity adjustments with minimum 48px height target -->
              <q-item-section side class="row no-wrap items-center">
                <div class="row items-center q-gutter-sm">
                  <q-btn 
                    flat 
                    round 
                    dense 
                    icon="remove" 
                    color="primary" 
                    class="touch-btn-48"
                    @click="cartStore.updateQuantity(item.id, item.quantity - 1)" 
                  />
                  <span class="text-subtitle1 text-weight-bold q-px-sm">{{ item.quantity }}</span>
                  <q-btn 
                    flat 
                    round 
                    dense 
                    icon="add" 
                    color="primary" 
                    class="touch-btn-48"
                    @click="cartStore.updateQuantity(item.id, item.quantity + 1)" 
                  />
                  <q-btn 
                    flat 
                    round 
                    dense 
                    icon="delete" 
                    color="red" 
                    class="touch-btn-48 q-ml-md" 
                    @click="cartStore.removeFromCart(item.id)" 
                  />
                </div>
              </q-item-section>
            </q-item>
          </q-list>

          <q-separator class="q-my-md" />

          <!-- Subtotals & Grand Totals -->
          <div class="row justify-between items-center q-py-sm">
            <span class="text-subtitle1 text-grey-7">Subtotal:</span>
            <span class="text-subtitle1 text-weight-medium">{{ formatPrice(cartStore.totalAmount) }}</span>
          </div>
          <div class="row justify-between items-center q-py-sm text-h6 text-weight-bold">
            <span>Total a pagar:</span>
            <span class="text-primary">{{ formatPrice(cartStore.totalAmount) }}</span>
          </div>
        </q-card>

        <!-- Billing Form & Receipt QUploader -->
        <q-card flat class="glass-card q-pa-md">
          <span class="text-weight-bold text-h6 block q-mb-md text-primary">Datos de Facturación y Envío</span>
          
          <q-form @submit="submitOrder" class="q-gutter-md">
            <div class="row q-col-gutter-md">
              <div class="col-12 col-sm-6">
                <q-input 
                  v-model="billingForm.nit" 
                  outlined 
                  dense 
                  label="NIT / C.I." 
                  placeholder="Ej: 1234567" 
                  lazy-rules
                  class="touch-btn-48-input"
                  :rules="[val => !!val || 'El NIT o C.I. es requerido']"
                />
              </div>
              <div class="col-12 col-sm-6">
                <q-input 
                  v-model="billingForm.razonSocial" 
                  outlined 
                  dense 
                  label="Razón Social / Nombre" 
                  placeholder="Ej: Juan Pérez" 
                  lazy-rules
                  class="touch-btn-48-input"
                  :rules="[val => !!val || 'El nombre o razón social es requerido']"
                />
              </div>
            </div>

            <q-input 
              v-model="billingForm.notes" 
              outlined 
              type="textarea" 
              dense 
              rows="3" 
              label="Notas Adicionales (Dirección de envío, celular de contacto, etc.)" 
              placeholder="Ej: Calle 15 de Calacoto #123. Contacto: 78945612..." 
            />

            <!-- QUploader Component with client-side validations -->
            <div class="q-mt-lg">
              <span class="text-weight-bold text-subtitle1 block q-mb-sm text-primary">Comprobante de Pago (QR Bancario)</span>
              
              <q-uploader
                ref="uploaderRef"
                label="Seleccionar o Capturar Comprobante"
                accept="image/*"
                max-file-size="512000"
                hide-upload-btn
                @added="onFileAdded"
                @rejected="onFileRejected"
                @removed="onFileRemoved"
                class="full-width q-uploader-responsive"
                flat
                bordered
                color="primary"
                text-color="white"
              >
                <!-- Custom header to explain constraints nicely -->
                <template v-slot:header="scope">
                  <div class="row no-wrap items-center q-pa-sm q-gutter-xs">
                    <q-btn v-if="scope.queuedFiles.length > 0" icon="clear_all" @click="scope.removeQueuedFiles" round dense flat class="touch-btn-48" />
                    <div class="col">
                      <div class="q-uploader__title">Sube tu comprobante de depósito</div>
                      <div class="q-uploader__subtitle">Formatos: PNG, JPG • Máximo 500KB</div>
                    </div>
                    <q-btn icon="add_a_photo" @click="scope.pickFiles" round dense flat class="touch-btn-48">
                      <q-uploader-add-trigger />
                    </q-btn>
                  </div>
                </template>
              </q-uploader>
              
              <div v-if="receiptRequiredError" class="text-negative text-caption q-mt-xs q-pl-sm">
                * Debe subir una imagen del comprobante de pago para procesar la preventa.
              </div>
            </div>

            <!-- Submit Button (Comfortable 48px tactile height) -->
            <q-btn 
              type="submit" 
              color="primary" 
              unelevated 
              no-caps 
              label="Confirmar Preventa y Enviar Comprobante" 
              class="full-width text-weight-bold q-py-md q-mt-lg touch-btn-48" 
              :loading="submitting"
              style="font-size: 1.1rem;"
            />
          </q-form>
        </q-card>
      </div>

      <!-- Right Column: QR and Instructions -->
      <div class="col-12 col-md-5">
        <q-card flat class="glass-card q-pa-md sticky-top text-center">
          <span class="text-weight-bold text-h6 block q-mb-md text-primary">Pasos para realizar tu Pago</span>
          
          <div class="q-py-md">
            <!-- QR Display Container -->
            <div class="qr-container bg-white q-pa-md inline-block shadow-2 rounded-borders q-mb-md">
              <q-img 
                :src="getQrCodeUrl" 
                width="220px" 
                height="220px" 
                fit="contain"
              >
                <template #error>
                  <div class="absolute-full flex flex-center bg-grey-3 text-grey-8 text-caption">
                    <q-icon name="qr_code" size="90px" color="primary" class="q-mb-xs" />
                    <span class="text-weight-bold">QR del Comercio</span>
                  </div>
                </template>
              </q-img>
            </div>

            <h5 class="text-weight-bold text-primary q-my-sm">Pago Simple / Escaneo QR</h5>
            
            <q-list class="text-left q-px-sm">
              <q-item class="q-px-none">
                <q-item-section avatar>
                  <q-avatar color="primary" text-color="white" size="30px">1</q-avatar>
                </q-item-section>
                <q-item-section>
                  <div class="text-weight-medium">Escanea o guarda el QR bancario superior.</div>
                </q-item-section>
              </q-item>
              
              <q-item class="q-px-none">
                <q-item-section avatar>
                  <q-avatar color="primary" text-color="white" size="30px">2</q-avatar>
                </q-item-section>
                <q-item-section>
                  <div class="text-weight-medium">Transfiere el monto exacto: <b class="text-primary">{{ formatPrice(cartStore.totalAmount) }}</b>.</div>
                </q-item-section>
              </q-item>

              <q-item class="q-px-none">
                <q-item-section avatar>
                  <q-avatar color="primary" text-color="white" size="30px">3</q-avatar>
                </q-item-section>
                <q-item-section>
                  <div class="text-weight-medium">Guarda la captura de pantalla del comprobante de transferencia en tu dispositivo.</div>
                </q-item-section>
              </q-item>

              <q-item class="q-px-none">
                <q-item-section avatar>
                  <q-avatar color="primary" text-color="white" size="30px">4</q-avatar>
                </q-item-section>
                <q-item-section>
                  <div class="text-weight-medium">Súbela en el cargador de la izquierda y presiona el botón "Confirmar Preventa" para enviar tu pedido.</div>
                </q-item-section>
              </q-item>
            </q-list>
          </div>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue';
import { useQuasar, QUploader } from 'quasar';
import api from '@/api';
import { useCartStore } from '@/stores/cart';
import { useSettingsStore } from '@/stores/settings';
import { useCurrency } from '@/composables/useCurrency';

const $q = useQuasar();
const cartStore = useCartStore();
const settingsStore = useSettingsStore();
const { formatCurrency } = useCurrency();

const uploaderRef = ref<QUploader | null>(null);
const submitting = ref(false);
const orderSubmitted = ref(false);
const submittedRef = ref('');
const receiptRequiredError = ref(false);

const billingForm = reactive({
  nit: '',
  razonSocial: '',
  notes: ''
});

const selectedFile = ref<File | null>(null);

onMounted(async () => {
  if (!settingsStore.settings) {
    await settingsStore.fetchSettings();
  }
});

const formatPrice = (price: number) => {
  return formatCurrency ? formatCurrency(price) : `${price} BOB`;
};

const getQrCodeUrl = computed(() => {
  if (!settingsStore.settings?.qrCodePath) return '';
  return settingsStore.settings.qrCodePath.startsWith('http')
    ? settingsStore.settings.qrCodePath
    : `${process.env.VITE_API_URL ? process.env.VITE_API_URL.replace('/api', '') : ''}${settingsStore.settings.qrCodePath}`;
});

// File events from QUploader
const onFileAdded = (files: readonly any[]) => {
  if (files.length > 0) {
    selectedFile.value = files[0];
    receiptRequiredError.value = false;
  }
};

const onFileRemoved = () => {
  selectedFile.value = null;
};

const onFileRejected = (rejectedEntries: readonly any[]) => {
  rejectedEntries.forEach(entry => {
    if (entry.failedPropValidation === 'max-file-size') {
      $q.notify({
        color: 'negative',
        message: 'La imagen excede el límite de 500 KB permitido.',
        icon: 'warning',
        position: 'top-right'
      });
    } else {
      $q.notify({
        color: 'negative',
        message: 'Archivo inválido. Asegúrese de subir una imagen.',
        icon: 'error',
        position: 'top-right'
      });
    }
  });
};

const submitOrder = async () => {
  // Check if file is selected
  if (!selectedFile.value) {
    receiptRequiredError.value = true;
    $q.notify({
      color: 'negative',
      message: 'Por favor, suba el comprobante de pago.',
      icon: 'warning'
    });
    return;
  }

  submitting.value = true;
  $q.loading.show({ message: 'Procesando tu preventa y cargando comprobante...' });

  try {
    // 1. Upload the receipt file using FormData
    const uploadForm = new FormData();
    uploadForm.append('file', selectedFile.value);
    
    const uploadRes = await api.post('/sales/online/upload-receipt', uploadForm, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });

    const receiptPath = uploadRes.data.path;

    // 2. Prepare order / sale details object
    const saleObject = {
      grandTotal: cartStore.totalAmount,
      nit: billingForm.nit,
      razonSocial: billingForm.razonSocial,
      notes: billingForm.notes,
      paymentReceiptPath: receiptPath,
      details: cartStore.items.map(item => ({
        productId: item.id,
        price: item.price,
        quantity: item.quantity,
        total: item.price * item.quantity,
        saleUnitId: item.saleUnitId
      }))
    };

    // 3. Register the online sale
    const saleRes = await api.post('/sales/online', saleObject);
    
    submittedRef.value = saleRes.data.ref;
    orderSubmitted.value = true;
    cartStore.clearCart();

    // Clear uploader files
    if (uploaderRef.value) {
      uploaderRef.value.removeQueuedFiles();
    }

    $q.notify({
      color: 'positive',
      message: '¡Su preventa se ha registrado con éxito!',
      icon: 'check_circle'
    });

  } catch (error: any) {
    console.error('Checkout error:', error);
    $q.notify({
      color: 'negative',
      message: error.response?.data?.message || 'Error al procesar el pedido. Intente nuevamente.',
      icon: 'report_problem'
    });
  } finally {
    submitting.value = false;
    $q.loading.hide();
  }
};
</script>

<style scoped>
.max-width-lg {
  max-width: 1200px;
}

.max-width-md {
  max-width: 650px;
}

.max-width-sm {
  max-width: 550px;
}

.sticky-top {
  position: sticky;
  top: 90px;
}

.glass-card {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  border-radius: 16px;
}

.body--dark .glass-card {
  background: rgba(30, 30, 30, 0.75);
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.qr-container {
  display: inline-block;
  padding: 12px;
  border: 1px solid rgba(0,0,0,0.06);
}

.text-glow-success {
  text-shadow: 0 0 15px rgba(76, 175, 80, 0.4);
}

.touch-btn-48 {
  min-height: 48px !important;
}

.touch-btn-48-input :deep(.q-field__control) {
  min-height: 48px !important;
}

.q-uploader-responsive {
  max-height: 280px;
}

.animate-bounce {
  animation: bounce 2s infinite;
}

@keyframes bounce {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-8px);
  }
}
</style>
