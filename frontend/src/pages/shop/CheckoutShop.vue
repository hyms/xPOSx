<template>
  <q-page class="q-py-xl container q-mx-auto q-px-md max-width-lg">
    <!-- Header -->
    <div class="text-center q-mb-xl">
      <h3 class="text-weight-bold q-my-none text-primary">Finalizar Preventa</h3>
      <p class="text-subtitle1 text-grey-6 q-mt-xs">Completa los datos de pago y facturación para reservar tu pedido</p>
    </div>

    <!-- Empty Cart State -->
    <div v-if="cartStore.items.length === 0 && !orderSubmitted" class="text-center q-py-xl glass-card">
      <q-icon name="shopping_cart_checkout" size="70px" color="grey-5" />
      <p class="text-h6 text-grey-6 q-mt-md">Tu carrito de compras está vacío.</p>
      <q-btn label="Ir al Catálogo" color="primary" unelevated no-caps size="md" class="q-mt-sm" to="/catalog" />
    </div>

    <!-- Order Submitted Success State -->
    <div v-else-if="orderSubmitted" class="text-center q-py-xl glass-card q-px-lg max-width-sm q-mx-auto">
      <q-icon name="check_circle" size="80px" color="positive" class="text-glow-success q-mb-md" />
      <h4 class="text-weight-bold q-my-none">¡Pedido Registrado con Éxito!</h4>
      <p class="text-subtitle1 text-grey-6 q-mt-md">
        Su pedido <b>{{ submittedRef }}</b> se ha registrado y está en estado <b>Pendiente de Verificación</b>. 
        En breve, nuestro administrador validará el depósito y procesará el despacho de sus productos.
      </p>
      <q-btn label="Seguir Comprando" color="primary" no-caps unelevated size="md" class="q-mt-lg" to="/catalog" />
    </div>

    <!-- Active Checkout Flow -->
    <div v-else class="row q-col-gutter-lg">
      <!-- Left Column: Cart Summary and Form -->
      <div class="col-12 col-md-7">
        <!-- Cart Items List -->
        <q-card flat class="glass-card q-pa-md q-mb-lg">
          <span class="text-weight-bold text-h6 block q-mb-md">Detalle de Productos</span>
          
          <q-list separator class="q-pl-none">
            <q-item v-for="item in cartStore.items" :key="item.id" class="q-px-none q-py-md">
              <q-item-section avatar>
                <q-img :src="item.image || '/icons/placeholder_product.png'" width="60px" height="60px" fit="cover" class="rounded-borders" />
              </q-item-section>
              
              <q-item-section>
                <q-item-label class="text-weight-bold text-subtitle1">{{ item.name }}</q-item-label>
                <q-item-label caption class="text-primary text-weight-medium">{{ formatPrice(item.price) }} x unidad</q-item-label>
              </q-item-section>

              <q-item-section side class="row no-wrap items-center">
                <!-- Quantity adjust -->
                <div class="row items-center q-gutter-xs">
                  <q-btn flat round dense icon="remove" size="xs" color="primary" @click="cartStore.updateQuantity(item.id, item.quantity - 1)" />
                  <span class="text-subtitle1 text-weight-bold q-px-sm">{{ item.quantity }}</span>
                  <q-btn flat round dense icon="add" size="xs" color="primary" @click="cartStore.updateQuantity(item.id, item.quantity + 1)" />
                  <q-btn flat round dense icon="delete" size="xs" color="red" class="q-ml-md" @click="cartStore.removeFromCart(item.id)" />
                </div>
              </q-item-section>
            </q-item>
          </q-list>

          <q-separator class="q-my-md" />

          <!-- Totals -->
          <div class="row justify-between items-center q-py-sm">
            <span class="text-subtitle1 text-grey-7">Subtotal:</span>
            <span class="text-subtitle1 text-weight-medium">{{ formatPrice(cartStore.totalAmount) }}</span>
          </div>
          <div class="row justify-between items-center q-py-sm text-h6 text-weight-bold">
            <span>Total a transferir:</span>
            <span class="text-primary">{{ formatPrice(cartStore.totalAmount) }}</span>
          </div>
        </q-card>

        <!-- Billing Info & Receipt Form -->
        <q-card flat class="glass-card q-pa-md">
          <span class="text-weight-bold text-h6 block q-mb-md">Datos de Facturación y Contacto</span>
          
          <q-form @submit="submitOrder" class="q-gutter-md">
            <div class="row q-col-gutter-sm">
              <div class="col-12 col-sm-6">
                <q-input 
                  v-model="billingForm.nit" 
                  outlined 
                  dense 
                  label="NIT / C.I." 
                  placeholder="Ej: 1234567" 
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
              label="Notas Adicionales" 
              placeholder="Indicaciones sobre entrega, horario, etc..." 
            />

            <!-- Image/Receipt Capture with Compression -->
            <div class="q-mt-lg">
              <span class="text-weight-bold text-subtitle1 block q-mb-sm">Comprobante de Pago (QR)</span>
              
              <div class="border-dashed rounded-borders q-pa-lg text-center relative-position cursor-pointer hover-bg" @click="triggerFileInput">
                <input 
                  type="file" 
                  ref="fileInput" 
                  accept="image/*" 
                  style="display: none;" 
                  @change="onFileSelected"
                />
                
                <div v-if="!receiptImagePreview" class="q-py-md">
                  <q-icon name="cloud_upload" size="50px" color="primary" />
                  <p class="text-weight-medium q-mt-sm">Haz clic para subir o capturar el comprobante</p>
                  <p class="text-caption text-grey-6">Se comprimirá automáticamente de forma reactiva antes de enviar</p>
                </div>

                <div v-else class="relative-position">
                  <q-img :src="receiptImagePreview" max-height="250px" fit="contain" class="rounded-borders" />
                  
                  <!-- Floating Remove Button -->
                  <q-btn 
                    fab-mini 
                    color="red" 
                    icon="close" 
                    size="sm" 
                    class="absolute top-right q-ma-xs" 
                    @click.stop="removeReceipt" 
                  />

                  <div class="q-mt-sm text-caption text-grey-7">
                    Imagen comprimida: <b>{{ originalSize }}</b> → <b class="text-primary">{{ compressedSize }}</b> ({{ compressionRatio }}% menor)
                  </div>
                </div>
              </div>
              <div v-if="receiptRequiredError" class="text-negative text-caption q-mt-xs q-pl-sm">
                Debe subir el comprobante de pago para procesar el pedido.
              </div>
            </div>

            <!-- Submit Button -->
            <q-btn 
              type="submit" 
              color="primary" 
              unelevated 
              no-caps 
              label="Confirmar y Enviar Venta" 
              class="full-width text-weight-bold q-py-sm q-mt-md" 
              :loading="submitting"
            />
          </q-form>
        </q-card>
      </div>

      <!-- Right Column: QR and Instructions -->
      <div class="col-12 col-md-5">
        <q-card flat class="glass-card q-pa-md sticky-top text-center">
          <span class="text-weight-bold text-h6 block q-mb-md">Instrucciones de Pago</span>
          
          <div class="q-py-md">
            <!-- QR Display -->
            <div class="qr-container bg-white q-pa-md inline-block shadow-1 rounded-borders q-mb-md">
              <q-img 
                :src="getQrCodeUrl" 
                width="200px" 
                height="200px" 
                fit="contain"
              >
                <template #error>
                  <!-- Beautiful simulated default QR if not configured -->
                  <div class="absolute-full flex flex-center bg-grey-3 text-grey-8 text-caption">
                    <q-icon name="qr_code" size="80px" color="primary" class="q-mb-xs" />
                    <span>QR de Pago General</span>
                  </div>
                </template>
              </q-img>
            </div>

            <p class="text-subtitle1 text-weight-bold text-primary">Escanea este QR con tu App Bancaria</p>
            <p class="text-body2 text-grey-7 q-px-md">
              1. Abre tu aplicación financiera bancaria.<br>
              2. Escanea el código de arriba y transfiere el monto exacto: <b>{{ formatPrice(cartStore.totalAmount) }}</b>.<br>
              3. Captura la pantalla del comprobante y súbela en el formulario de la izquierda.<br>
              4. Confirma tus datos de facturación para completar el pedido.
            </p>
          </div>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue';
import { useQuasar } from 'quasar';
import api from '@/api';
import { useCartStore } from '@/stores/cart';
import { useSettingsStore } from '@/stores/settings';
import { useCurrency } from '@/composables/useCurrency';

const $q = useQuasar();
const cartStore = useCartStore();
const settingsStore = useSettingsStore();
const { formatCurrency } = useCurrency();

const fileInput = ref<HTMLInputElement | null>(null);
const submitting = ref(false);
const orderSubmitted = ref(false);
const submittedRef = ref('');
const receiptRequiredError = ref(false);

// Sizes for showing compression ratio
const originalSize = ref('');
const compressedSize = ref('');
const compressionRatio = ref(0);

const billingForm = reactive({
  nit: '',
  razonSocial: '',
  notes: ''
});

const receiptFile = ref<File | null>(null);
const receiptImagePreview = ref<string | null>(null);

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

const triggerFileInput = () => {
  fileInput.value?.click();
};

const removeReceipt = () => {
  receiptFile.value = null;
  receiptImagePreview.value = null;
  originalSize.value = '';
  compressedSize.value = '';
  compressionRatio.value = 0;
};

// Compression logic as requested
const compressImage = (file: File): Promise<File> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = (event) => {
      const img = new Image();
      img.src = event.target?.result as string;
      img.onload = () => {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        if (!ctx) return resolve(file);

        const maxW = 1000;
        const maxH = 1000;
        let width = img.width;
        let height = img.height;

        if (width > maxW || height > maxH) {
          if (width > height) {
            height = Math.round((height * maxW) / width);
            width = maxW;
          } else {
            width = Math.round((width * maxH) / height);
            height = maxH;
          }
        }

        canvas.width = width;
        canvas.height = height;
        ctx.drawImage(img, 0, 0, width, height);

        canvas.toBlob((blob) => {
          if (blob) {
            const compressedFile = new File([blob], file.name.replace(/\.[^/.]+$/, "") + ".jpg", {
              type: 'image/jpeg',
              lastModified: Date.now()
            });
            resolve(compressedFile);
          } else {
            resolve(file);
          }
        }, 'image/jpeg', 0.7); // 70% quality compression
      };
      img.onerror = () => reject(new Error('Error al decodificar la imagen'));
    };
    reader.onerror = () => reject(new Error('Error al leer el archivo'));
  });
};

const formatKB = (bytes: number) => {
  return (bytes / 1024).toFixed(1) + ' KB';
};

const onFileSelected = async (e: Event) => {
  const target = e.target as HTMLInputElement;
  if (!target.files || target.files.length === 0) return;

  const originalFile = target.files[0];
  originalSize.value = formatKB(originalFile.size);

  $q.loading.show({ message: 'Comprimiendo comprobante de forma reactiva...' });
  
  try {
    const compressed = await compressImage(originalFile);
    receiptFile.value = compressed;
    compressedSize.value = formatKB(compressed.size);
    compressionRatio.value = Math.round(((originalFile.size - compressed.size) / originalFile.size) * 100);

    const reader = new FileReader();
    reader.onload = (event) => {
      receiptImagePreview.value = event.target?.result as string;
    };
    reader.readAsDataURL(compressed);
    receiptRequiredError.value = false;
  } catch (error) {
    console.error('Compression error:', error);
    $q.notify({
      color: 'negative',
      message: 'No se pudo comprimir la imagen.',
      icon: 'report_problem'
    });
  } finally {
    $q.loading.hide();
  }
};

const submitOrder = async () => {
  if (!receiptFile.value) {
    receiptRequiredError.value = true;
    return;
  }

  submitting.value = true;
  $q.loading.show({ message: 'Registrando pedido y cargando comprobante...' });

  try {
    // 1. Upload receipt
    const uploadForm = new FormData();
    uploadForm.append('file', receiptFile.value);
    
    const uploadRes = await api.post('/sales/online/upload-receipt', uploadForm, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });

    const receiptPath = uploadRes.data.path;

    // 2. Prepare sale object
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

    // 3. Post to online sales API
    const saleRes = await api.post('/sales/online', saleObject);
    
    submittedRef.value = saleRes.data.ref;
    orderSubmitted.value = true;
    cartStore.clearCart();

    $q.notify({
      color: 'positive',
      message: '¡Pedido enviado exitosamente!',
      icon: 'check_circle_outline'
    });
  } catch (error: any) {
    console.error('Error submitting order:', error);
    $q.notify({
      color: 'negative',
      message: error.response?.data?.message || 'Error interno al registrar el pedido.',
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

.border-dashed {
  border: 2px dashed rgba(var(--q-primary), 0.3);
  background: rgba(var(--q-primary), 0.01);
  transition: all 0.2s ease;
}

.border-dashed:hover {
  background: rgba(var(--q-primary), 0.04);
  border-color: var(--q-primary);
}

.qr-container {
  display: inline-block;
  padding: 12px;
  border: 1px solid rgba(0,0,0,0.06);
}

.text-glow-success {
  text-shadow: 0 0 15px rgba(76, 175, 80, 0.4);
}

.top-right {
  top: 0;
  right: 0;
}
</style>
