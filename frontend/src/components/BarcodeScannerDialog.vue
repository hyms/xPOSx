<template>
  <q-dialog v-model="internalModel" persistent maximized transition-show="slide-up" transition-hide="slide-down">
    <q-card class="column no-wrap bg-black text-white">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">{{ title }}</div>
        <q-space />
        <q-btn icon="close" flat round dense v-close-popup color="white" />
      </q-card-section>

      <q-card-section class="col column flex-center q-pa-none relative-position overflow-hidden">
        <qrcode-stream
          v-if="internalModel"
          :paused="paused"
          :track="paintBoundingBox"
          :formats="['qr_code', 'ean_13', 'ean_8', 'code_128', 'code_39', 'upc_a', 'upc_e']"
          @detect="onDetect"
          @error="onError"
          @camera-on="onCameraOn"
          class="full-height"
        >
          <div v-if="loading" class="absolute-full flex flex-center bg-black">
            <q-spinner-dots color="primary" size="40px" />
          </div>

          <div class="scanner-overlay absolute-full no-pointer-events">
            <div class="scanner-laser"></div>
          </div>
        </qrcode-stream>

        <div v-if="errorMessage" class="absolute-center text-center q-pa-md bg-red-9 rounded-borders">
          <q-icon name="error" size="48px" />
          <div class="text-subtitle1 q-mt-sm">{{ errorMessage }}</div>
          <q-btn label="Cerrar" color="white" flat class="q-mt-md" v-close-popup />
        </div>
      </q-card-section>

      <q-card-actions align="center" class="q-pa-md bg-grey-10">
        <div class="column items-center q-gutter-y-sm">
          <div class="text-caption text-grey-5">
            Coloque el código de barras dentro del visor
          </div>
          <div class="row q-gutter-x-sm">
            <q-toggle
              v-model="continuous"
              label="Escaneo continuo"
              color="primary"
              dark
              v-if="showContinuousOption"
            />
          </div>
        </div>
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { QrcodeStream } from 'vue-qrcode-reader'
import { useQuasar } from 'quasar'

const props = defineProps({
  modelValue: Boolean,
  title: {
    type: String,
    default: 'Escanear Código'
  },
  showContinuousOption: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue', 'detect'])

const $q = useQuasar()
const loading = ref(true)
const paused = ref(false)
const errorMessage = ref('')
const continuous = ref(props.showContinuousOption)

const internalModel = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const onCameraOn = () => {
  loading.value = false
}

const onError = (error: any) => {
  loading.value = false
  if (error.name === 'NotAllowedError') {
    errorMessage.value = 'Permiso denegado para acceder a la cámara'
  } else if (error.name === 'NotFoundError') {
    errorMessage.value = 'No se encontró ninguna cámara en este dispositivo'
  } else if (error.name === 'NotSupportedError') {
    errorMessage.value = 'Cámara no soportada en este contexto (requiere HTTPS)'
  } else if (error.name === 'NotReadableError') {
    errorMessage.value = 'La cámara está siendo usada por otra aplicación'
  } else {
    errorMessage.value = 'Error al iniciar la cámara: ' + error.message
  }
}

const onDetect = (detectedCodes: any[]) => {
  if (detectedCodes.length > 0) {
    // Tomamos el primer código detectado
    const result = detectedCodes[0]
    const code = result.rawValue
    
    if (!code) return

    $q.notify({
      message: `Código detectado: ${code}`,
      caption: `Formato: ${result.format}`,
      color: 'positive',
      icon: 'check',
      timeout: 600,
      position: 'top'
    })

    emit('detect', code)

    if (!continuous.value) {
      internalModel.value = false
    } else {
      // Pausar brevemente para evitar escaneos múltiples del mismo objeto
      paused.value = true
      setTimeout(() => {
        paused.value = false
      }, 1500)
    }
  }
}

const paintBoundingBox = (detectedCodes: any[], ctx: CanvasRenderingContext2D) => {
  for (const { boundingBox: { x, y, width, height } } of detectedCodes) {
    ctx.lineWidth = 2
    ctx.strokeStyle = '#22C55E'
    ctx.strokeRect(x, y, width, height)
  }
}

watch(() => props.modelValue, (newVal) => {
  if (newVal) {
    loading.value = true
    errorMessage.value = ''
    paused.value = false
  }
})
</script>

<style scoped lang="scss">
.scanner-overlay {
  border: 2px solid rgba(255, 255, 255, 0.3);
  box-shadow: inset 0 0 100px rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  
  &::before {
    content: '';
    width: 250px;
    height: 250px;
    border: 2px solid var(--q-primary);
    border-radius: 20px;
    box-shadow: 0 0 0 9999px rgba(0, 0, 0, 0.5);
  }
}

.scanner-laser {
  position: absolute;
  width: 250px;
  height: 2px;
  background-color: var(--q-primary);
  box-shadow: 0 0 8px var(--q-primary);
  animation: laser-scan 2s infinite ease-in-out;
}

@keyframes laser-scan {
  0% { top: calc(50% - 125px); }
  50% { top: calc(50% + 125px); }
  100% { top: calc(50% - 125px); }
}
</style>
