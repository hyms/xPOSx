<template>
  <q-page class="q-pb-xl">
    <!-- Hero / Promotional Banner Carousel -->
    <div class="hero-banner relative-position q-mb-xl overflow-hidden">
      <div class="absolute-full gradient-overlay"></div>
      <q-carousel
        animated
        v-model="slide"
        navigation
        infinite
        :autoplay="autoplay"
        arrows
        transition-prev="slide-right"
        transition-next="slide-left"
        @mouseenter="autoplay = false"
        @mouseleave="autoplay = true"
        class="bg-transparent hero-carousel"
        height="500px"
      >
        <q-carousel-slide :name="1" class="column no-wrap flex-center text-white q-px-lg">
          <q-icon name="shopping_bag" size="80px" class="q-mb-md text-glow" />
          <div class="text-h2 text-weight-bolder text-center leading-tight">
            Preventas Exclusivas QR
          </div>
          <div class="text-h5 text-center q-mt-md q-mx-auto max-width-md opacity-90">
            Escanea, transfiere y asegura tus productos favoritos con total facilidad y seguridad.
          </div>
          <q-btn label="Explorar Catálogo" to="/catalog" color="primary" size="lg" no-caps class="q-mt-xl shadow-glow text-weight-bold" />
        </q-carousel-slide>

        <q-carousel-slide :name="2" class="column no-wrap flex-center text-white q-px-lg">
          <q-icon name="qr_code_2" size="80px" class="q-mb-md text-glow" />
          <div class="text-h2 text-weight-bolder text-center leading-tight">
            Paga Rápido y Simple
          </div>
          <div class="text-h5 text-center q-mt-md q-mx-auto max-width-md opacity-90">
            Sube tu comprobante de pago comprimido al instante desde tu dispositivo.
          </div>
          <q-btn label="Ver Productos" to="/catalog" color="primary" size="lg" no-caps class="q-mt-xl shadow-glow text-weight-bold" />
        </q-carousel-slide>
      </q-carousel>
    </div>

    <div class="container q-mx-auto q-px-md max-width-lg">
      <!-- Section: Categories Quick Access -->
      <div class="text-center q-mb-xl">
        <h3 class="text-weight-bold q-my-none text-primary">Categorías Destacadas</h3>
        <p class="text-subtitle1 text-grey-6 q-mt-sm">Encuentra exactamente lo que necesitas en un solo clic</p>
        
        <div class="row q-col-gutter-md justify-center q-mt-md">
          <div 
            v-for="cat in categories.slice(0, 4)" 
            :key="cat.id" 
            class="col-6 col-sm-3 col-md-2 cursor-pointer"
            @click="goToCatalogWithCategory(cat.id)"
          >
            <q-card flat class="category-card text-center q-py-lg glass-card hover-lift">
              <q-avatar size="50px" color="primary-light" text-color="primary" class="q-mb-sm">
                <q-icon name="category" />
              </q-avatar>
              <div class="text-weight-bold q-px-xs text-ellipsis">{{ cat.name }}</div>
            </q-card>
          </div>
        </div>
      </div>

      <!-- Section: Featured Products Grid -->
      <div class="q-mb-xl">
        <div class="row items-center justify-between q-mb-md">
          <div>
            <h3 class="text-weight-bold q-my-none text-primary">Productos Destacados</h3>
            <p class="text-subtitle1 text-grey-6 q-mt-xs">Nuestra selección exclusiva para pre-venta</p>
          </div>
          <q-btn flat no-caps color="primary" label="Ver todo el catálogo" icon-right="arrow_forward" to="/catalog" class="text-weight-bold" />
        </div>

        <div v-if="loading" class="row justify-center q-py-xl">
          <q-spinner-dots color="primary" size="40px" />
        </div>

        <div v-else-if="products.length === 0" class="text-center q-py-xl">
          <q-icon name="inventory_2" size="60px" color="grey-5" />
          <p class="text-h6 text-grey-6 q-mt-md">No hay productos destacados disponibles en este momento.</p>
        </div>

        <div v-else class="row q-col-gutter-lg">
          <div 
            v-for="product in products.slice(0, 8)" 
            :key="product.id" 
            class="col-12 col-sm-6 col-md-3"
          >
            <q-card class="product-card glass-card hover-lift relative-position flex column justify-between">
              <!-- Image container -->
              <div class="product-image-container relative-position overflow-hidden">
                <q-img 
                  :src="product.image || '/icons/placeholder_product.png'" 
                  height="200px" 
                  fit="cover" 
                  class="product-img"
                >
                  <template #error>
                    <div class="absolute-full flex flex-center bg-grey-3 text-grey-7">
                      <q-icon name="image" size="48px" />
                    </div>
                  </template>
                </q-img>
                <q-chip 
                  color="accent" 
                  text-color="white" 
                  dense 
                  label="Destacado" 
                  class="absolute top-right q-ma-sm text-weight-bold" 
                />
              </div>

              <!-- Content -->
              <q-card-section class="q-pa-md flex-grow">
                <div class="text-subtitle2 text-primary text-weight-bold text-uppercase tracking-wider">
                  {{ product.categoryName || 'General' }}
                </div>
                <div class="text-h6 text-weight-bold q-mt-xs text-ellipsis-2 text-height-tight">
                  {{ product.name }}
                </div>
                <div class="row items-center justify-between q-mt-md">
                  <span class="text-h5 text-weight-bolder text-primary">
                    {{ formatPrice(product.price) }}
                  </span>
                  <span class="text-caption text-grey-6" v-if="product.code">
                    Cód: {{ product.code }}
                  </span>
                </div>
              </q-card-section>

              <!-- Actions -->
              <q-card-actions class="q-px-md q-pb-md q-pt-none">
                <q-btn 
                  color="primary" 
                  unelevated 
                  no-caps 
                  icon="add_shopping_cart" 
                  label="Agregar al carrito" 
                  class="full-width text-weight-bold" 
                  @click="addToCart(product)"
                />
              </q-card-actions>
            </q-card>
          </div>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import { productService } from '@/services/product.service';
import { categoryService } from '@/services/category.service';
import { useCartStore } from '@/stores/cart';
import { useCurrency } from '@/composables/useCurrency';

const slide = ref(1);
const autoplay = ref(true);
const loading = ref(true);
const products = ref<any[]>([]);
const categories = ref<any[]>([]);

const router = useRouter();
const $q = useQuasar();
const cartStore = useCartStore();
const { formatCurrency } = useCurrency();

onMounted(async () => {
  try {
    const [prodResponse, catResponse] = await Promise.all([
      productService.getAll(),
      categoryService.getAll()
    ]);
    products.value = prodResponse.data;
    categories.value = catResponse.data;
  } catch (err) {
    console.error('Error fetching shop data:', err);
    $q.notify({
      color: 'negative',
      message: 'No se pudieron cargar los productos destacados.',
      icon: 'report_problem'
    });
  } finally {
    loading.value = false;
  }
});

const formatPrice = (price: number) => {
  return formatCurrency ? formatCurrency(price) : `${price} BOB`;
};

const addToCart = (product: any) => {
  cartStore.addToCart(product);
  $q.notify({
    color: 'positive',
    message: `${product.name} agregado al carrito`,
    icon: 'check_circle',
    timeout: 1500,
    position: 'top-right'
  });
};

const goToCatalogWithCategory = (catId: number) => {
  router.push({ path: '/catalog', query: { categoryId: catId.toString() } });
};
</script>

<style scoped>
.hero-banner {
  background: linear-gradient(135deg, #100b32 0%, #3b1154 50%, #150d3a 100%);
  min-height: 450px;
  border-radius: 0 0 40px 40px;
}

.gradient-overlay {
  background: radial-gradient(circle, transparent 20%, rgba(0,0,0,0.4) 100%);
}

.hero-carousel {
  border-radius: 0 0 40px 40px;
}

.text-glow {
  text-shadow: 0 0 20px rgba(186, 104, 200, 0.6);
}

.shadow-glow {
  box-shadow: 0 0 15px rgba(186, 104, 200, 0.4);
}

.max-width-md {
  max-width: 700px;
}

.max-width-lg {
  max-width: 1200px;
}

.leading-tight {
  line-height: 1.15;
}

.hover-lift {
  transition: transform 0.25s cubic-bezier(0.2, 0.8, 0.2, 1), box-shadow 0.25s cubic-bezier(0.2, 0.8, 0.2, 1);
}

.hover-lift:hover {
  transform: translateY(-8px);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.12);
}

.glass-card {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(10px);
  -webkit-backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.4);
  border-radius: 16px;
  overflow: hidden;
}

.body--dark .glass-card {
  background: rgba(30, 30, 30, 0.75);
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.product-card {
  height: 100%;
}

.product-image-container {
  border-top-left-radius: 16px;
  border-top-right-radius: 16px;
}

.product-img {
  transition: transform 0.5s ease;
}

.product-card:hover .product-img {
  transform: scale(1.05);
}

.top-right {
  top: 0;
  right: 0;
}

.text-ellipsis-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
  min-height: 2.8rem;
}

.text-height-tight {
  line-height: 1.25;
}

.text-ellipsis {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.category-card {
  border-radius: 16px;
  padding: 16px;
}

.primary-light {
  background-color: rgba(var(--q-primary), 0.1);
}
</style>
