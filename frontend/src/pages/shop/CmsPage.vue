<template>
  <q-page class="q-py-xl container q-mx-auto q-px-md max-width-md" :class="$q.dark.isActive ? 'bg-dark' : 'bg-grey-1'">
    <q-card flat class="glass-card q-pa-xl q-mb-lg">
      
      <!-- Loader State -->
      <div v-if="loading" class="row justify-center q-py-xl">
        <q-spinner-dots color="primary" size="50px" />
      </div>

      <!-- CMS Content Rendering -->
      <div v-else-if="pageData">
        <h2 class="text-weight-bold q-mt-none q-mb-lg text-primary page-title">{{ pageData.title }}</h2>
        <q-separator class="q-mb-xl" />
        
        <!-- Render HTML content safely -->
        <div class="cms-rendered-content" v-html="pageData.content"></div>
      </div>

      <!-- Error / Not Found State -->
      <div v-else class="text-center q-py-xl">
        <q-icon name="error_outline" size="70px" color="negative" class="q-mb-md" />
        <p class="text-h5 text-weight-bold q-mt-md">Página no encontrada</p>
        <p class="text-grey-6 text-subtitle1">La página solicitada no existe o no se encuentra disponible en este momento.</p>
        <q-btn 
          label="Volver al Inicio" 
          color="primary" 
          no-caps 
          unelevated 
          to="/" 
          class="q-mt-lg touch-btn-48 text-weight-bold" 
          size="lg"
        />
      </div>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import api from '@/api';

const route = useRoute();
const loading = ref(true);
const pageData = ref<any>(null);

const fetchPageContent = async (slug: string) => {
  loading.value = true;
  try {
    const res = await api.get(`/cms/pages/${slug}`);
    pageData.value = res.data;
  } catch (error) {
    console.error('Error fetching CMS page:', error);
    pageData.value = null;
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  if (route.params.slug) {
    fetchPageContent(route.params.slug as string);
  }
});

// React to route slug change (e.g. from Terms to Privacy Policy)
watch(() => route.params.slug, (newSlug) => {
  if (newSlug) {
    fetchPageContent(newSlug as string);
  }
});
</script>

<style scoped>
.max-width-md {
  max-width: 800px;
}

.glass-card {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  border-radius: 20px;
}

.body--dark .glass-card {
  background: rgba(30, 30, 30, 0.75);
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.page-title {
  font-size: 2.25rem;
  line-height: 1.2;
}

.touch-btn-48 {
  min-height: 48px !important;
  min-width: 160px;
}
</style>

<style>
/* Integrated styles specifically for the rendered CMS HTML content */
.cms-rendered-content {
  font-size: 1.1rem;
  line-height: 1.75;
}

.cms-rendered-content h2 {
  font-size: 1.65rem;
  font-weight: 700;
  margin-top: 2.5rem;
  margin-bottom: 1.2rem;
  color: var(--q-primary);
}

.cms-rendered-content h3 {
  font-size: 1.35rem;
  font-weight: 600;
  margin-top: 2rem;
  margin-bottom: 1rem;
}

.cms-rendered-content p {
  margin-bottom: 1.4rem;
}

.cms-rendered-content ul, .cms-rendered-content ol {
  margin-bottom: 1.5rem;
  padding-left: 1.5rem;
}

.cms-rendered-content li {
  margin-bottom: 0.6rem;
}

.cms-rendered-content b, .cms-rendered-content strong {
  font-weight: 700;
  color: var(--q-primary);
}

.body--dark .cms-rendered-content {
  color: #e0e0e0;
}
</style>
