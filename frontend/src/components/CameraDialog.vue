<template>
  <q-dialog v-model="internalModel" persistent maximized transition-show="slide-up" transition-hide="slide-down">
    <q-card class="column no-wrap bg-black text-white">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">Tomar Foto</div>
        <q-space />
        <q-btn icon="close" flat round dense v-close-popup color="white" />
      </q-card-section>

      <q-card-section class="col column flex-center q-pa-none relative-position overflow-hidden">
        <video
          ref="video"
          autoplay
          playsinline
          class="full-width full-height object-cover"
          style="object-fit: cover"
        ></video>
        
        <canvas ref="canvas" style="display: none"></canvas>

        <div v-if="loading" class="absolute-full flex flex-center bg-black">
          <q-spinner-dots color="primary" size="40px" />
        </div>
      </q-card-section>

      <q-card-actions align="center" class="q-pa-lg bg-grey-10">
        <div class="row q-gutter-x-lg items-center">
          <q-btn
            round
            size="20px"
            color="white"
            text-color="black"
            icon="camera"
            @click="capturePhoto"
            :disable="loading"
          />
        </div>
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue'
import { useQuasar } from 'quasar'

const props = defineProps({
  modelValue: Boolean
})

const emit = defineEmits(['update:modelValue', 'captured'])

const $q = useQuasar()
const video = ref<HTMLVideoElement | null>(null)
const canvas = ref<HTMLCanvasElement | null>(null)
const loading = ref(true)
const stream = ref<MediaStream | null>(null)

const internalModel = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const startCamera = async () => {
  loading.value = true
  try {
    const constraints = {
      video: {
        facingMode: 'environment',
        width: { ideal: 1280 },
        height: { ideal: 720 }
      }
    }
    stream.value = await navigator.mediaDevices.getUserMedia(constraints)
    if (video.value) {
      video.value.srcObject = stream.value
      video.value.onloadedmetadata = () => {
        loading.value = false
      }
    }
  } catch (err) {
    console.error('Error al acceder a la cámara:', err)
    $q.notify({
      color: 'negative',
      message: 'No se pudo acceder a la cámara'
    })
    internalModel.value = false
  }
}

const stopCamera = () => {
  if (stream.value) {
    stream.value.getTracks().forEach(track => track.stop())
    stream.value = null
  }
}

const capturePhoto = () => {
  if (video.value && canvas.value) {
    const context = canvas.value.getContext('2d')
    if (context) {
      canvas.value.width = video.value.videoWidth
      canvas.value.height = video.value.videoHeight
      context.drawImage(video.value, 0, 0, canvas.value.width, canvas.value.height)
      
      const dataUrl = canvas.value.toDataURL('image/jpeg', 0.8)
      emit('captured', dataUrl)
      internalModel.value = false
    }
  }
}

watch(() => props.modelValue, (newVal) => {
  if (newVal) {
    startCamera()
  } else {
    stopCamera()
  }
})

onUnmounted(() => {
  stopCamera()
})
</script>

<style scoped>
.object-cover {
  object-fit: cover;
}
</style>
