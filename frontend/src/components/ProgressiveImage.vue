<template>
  <div class="progressive-image-container" :style="{ aspectRatio: ratio }">
    <!-- Loader/Skeleton mientras carga -->
    <transition name="fade">
      <div v-if="!isLoaded" class="absolute-full flex flex-center bg-grey-2 loader-wrapper">
        <q-skeleton type="rect" class="full-width full-height" animation="pulse" />
        <q-spinner-dots color="primary" size="2em" class="absolute" />
      </div>
    </transition>

    <!-- Imagen Final -->
    <transition name="fade-slow">
      <img
        v-show="isLoaded"
        :src="currentSrc"
        :alt="alt"
        class="product-image"
        @load="onImageLoad"
      />
    </transition>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';

const props = defineProps({
  src: { type: String, required: true },
  alt: { type: String, default: 'Imagen' },
  ratio: { type: String, default: '1/1' }
});

const isLoaded = ref(false);
const currentSrc = ref('');

const loadImage = (url: string) => {
  if (!url) {
    isLoaded.value = false;
    currentSrc.value = '';
    return;
  }
  isLoaded.value = false;
  const img = new Image();
  img.src = url;
  img.onload = () => {
    currentSrc.value = url;
    isLoaded.value = true;
  };
};

const onImageLoad = () => {
  isLoaded.value = true;
};

onMounted(() => loadImage(props.src));
watch(() => props.src, (newUrl) => loadImage(newUrl));
</script>

<style scoped lang="scss">
.progressive-image-container {
  position: relative;
  overflow: hidden;
  width: 100%;
  border-radius: 8px;
  background: #f5f5f5;
  display: flex;
  align-items: center;
  justify-content: center;
}

.loader-wrapper {
  z-index: 1;
}

.product-image {
  width: 100%;
  height: 100%;
  object-fit: contain;
  display: block;
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}

.fade-slow-enter-active {
  transition: opacity 0.6s cubic-bezier(0.4, 0, 0.2, 1);
}
.fade-slow-enter-from {
  opacity: 0;
}
</style>
