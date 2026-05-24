<template>
  <q-page class="q-py-xl container q-mx-auto q-px-md max-width-lg">
    
    <!-- Page Title & Header -->
    <div class="row items-center justify-between q-mb-xl">
      <div>
        <h3 class="text-weight-bold q-my-none text-primary">Catálogo de Productos</h3>
        <p class="text-subtitle1 text-grey-6 q-mt-xs">Explora nuestra colección y compra con preventa QR</p>
      </div>

      <!-- Quick Cart Chip Info -->
      <q-chip 
        outline 
        color="primary" 
        icon="shopping_cart" 
        class="text-weight-bold q-py-md cursor-pointer touch-btn-48" 
        to="/carrito"
      >
        Carrito: {{ cartStore.totalItems }} items / {{ formatPrice(cartStore.totalAmount) }}
      </q-chip>
    </div>

    <!-- Main Content Layout -->
    <div class="row q-col-gutter-lg">
      
      <!-- Desktop Filters Column (Visible on Desktop/Tablet landscape gt-sm) -->
      <div class="col-12 col-md-3 gt-sm">
        <q-card flat class="glass-card q-pa-md sticky-top">
          <div class="row items-center justify-between q-mb-md">
            <span class="text-weight-bold text-h6 text-primary">Filtros</span>
            <q-btn 
              v-if="search || selectedCategory" 
              flat 
              dense 
              no-caps 
              label="Limpiar todo" 
              color="negative" 
              @click="clearFilters" 
              class="text-weight-bold touch-btn-48" 
            />
          </div>

          <!-- Text Search -->
          <div class="q-mb-lg">
            <span class="text-weight-bold text-subtitle2 block q-mb-xs text-grey-7">Buscar</span>
            <q-input 
              v-model="search" 
              outlined 
              dense 
              placeholder="Nombre o código..." 
              clearable
              bg-color="transparent"
              class="search-input"
            >
              <template #prepend>
                <q-icon name="search" color="primary" />
              </template>
            </q-input>
          </div>

          <!-- Category Selection -->
          <div>
            <span class="text-weight-bold text-subtitle2 block q-mb-xs text-grey-7">Categorías</span>
            <q-list dense class="q-pl-none">
              <!-- "All" category option -->
              <q-item 
                clickable 
                :active="selectedCategory === null"
                active-class="bg-primary-light text-primary text-weight-bold"
                class="category-list-item rounded-borders q-my-xs touch-btn-48"
                @click="selectedCategory = null"
              >
                <q-item-section avatar class="min-width-auto q-pr-sm">
                  <q-icon name="chevron_right" size="18px" />
                </q-item-section>
                <q-item-section>Todas las categorías</q-item-section>
              </q-item>

              <!-- Category items -->
              <q-item 
                clickable 
                v-for="cat in categories" 
                :key="cat.id"
                :active="selectedCategory === cat.id"
                active-class="bg-primary-light text-primary text-weight-bold"
                class="category-list-item rounded-borders q-my-xs touch-btn-48"
                @click="selectedCategory = cat.id"
              >
                <q-item-section avatar class="min-width-auto q-pr-sm">
                  <q-icon name="chevron_right" size="18px" />
                </q-item-section>
                <q-item-section>{{ cat.name }}</q-item-section>
              </q-item>
            </q-list>
          </div>
        </q-card>
      </div>

      <!-- Products Grid Column (Adaptive Layout) -->
      <div class="col-12 col-md-9">
        
        <!-- Loading Spinner -->
        <div v-if="loading" class="row justify-center q-py-xl">
          <q-spinner-dots color="primary" size="50px" />
        </div>

        <!-- No Products Found State -->
        <div v-else-if="filteredProducts.length === 0" class="text-center q-py-xl glass-card q-px-md">
          <q-icon name="search_off" size="60px" color="grey-5" />
          <p class="text-h6 text-grey-6 q-mt-md">No se encontraron productos para los criterios seleccionados.</p>
          <q-btn 
            label="Limpiar Filtros" 
            color="primary" 
            no-caps 
            unelevated 
            class="q-mt-sm touch-btn-48" 
            @click="clearFilters" 
          />
        </div>

        <!-- Responsive Products Grid -->
        <div v-else class="row q-col-gutter-lg">
          <div 
            v-for="product in filteredProducts" 
            :key="product.id" 
            class="col-12 col-sm-6 col-md-4"
          >
            <q-card class="product-card glass-card hover-lift flex column justify-between">
              
              <!-- Product Image -->
              <div class="product-image-container relative-position overflow-hidden">
                <q-img 
                  :src="product.image || '/icons/placeholder_product.png'" 
                  height="180px" 
                  fit="cover" 
                  class="product-img"
                >
                  <template #error>
                    <div class="absolute-full flex flex-center bg-grey-3 text-grey-7">
                      <q-icon name="image" size="48px" />
                    </div>
                  </template>
                </q-img>
              </div>

              <!-- Product Info -->
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

              <!-- Add to Cart (min 48px tactile height) -->
              <q-card-actions class="q-px-md q-pb-md q-pt-none">
                <q-btn 
                  color="primary" 
                  unelevated 
                  no-caps 
                  icon="add_shopping_cart" 
                  label="Agregar al carrito" 
                  class="full-width text-weight-bold touch-btn-48" 
                  @click="addToCart(product)"
                />
              </q-card-actions>
            </q-card>
          </div>
        </div>
      </div>
    </div>

    <!-- Floating Action Button for Mobile/Tablet Filters (Visible on lt-md) -->
    <q-page-sticky position="bottom-right" :offset="[20, 20]" class="lt-md" style="z-index: 1000;">
      <q-btn 
        fab 
        icon="tune" 
        color="primary" 
        class="shadow-glow" 
        @click="showMobileFilters = true"
        style="width: 56px; height: 56px;"
      >
        <q-badge v-if="search || selectedCategory" color="red" floating rounded />
      </q-btn>
    </q-page-sticky>

    <!-- Mobile Bottom-Sheet Filters Dialog (lt-md) -->
    <q-dialog v-model="showMobileFilters" position="bottom" class="lt-md">
      <q-card class="mobile-filters-card q-pa-lg">
        <div class="row items-center justify-between q-mb-md">
          <span class="text-weight-bold text-h6 text-primary">Filtrar Productos</span>
          <q-btn flat round dense icon="close" v-close-popup class="touch-btn-48" />
        </div>

        <q-separator class="q-mb-lg" />

        <!-- Search Input -->
        <div class="q-mb-lg">
          <span class="text-weight-bold text-subtitle2 block q-mb-xs text-grey-7">Buscar</span>
          <q-input 
            v-model="search" 
            outlined 
            dense 
            placeholder="Buscar por nombre o código..." 
            clearable
          >
            <template #prepend>
              <q-icon name="search" color="primary" />
            </template>
          </q-input>
        </div>

        <!-- Categories List (Select option for cleaner space in mobile sheet) -->
        <div class="q-mb-xl">
          <span class="text-weight-bold text-subtitle2 block q-mb-xs text-grey-7">Categorías</span>
          <q-select
            v-model="selectedCategory"
            :options="categories"
            option-value="id"
            option-label="name"
            emit-value
            map-options
            outlined
            dense
            label="Selecciona una categoría"
            clearable
            class="touch-btn-48"
          />
        </div>

        <!-- Sheet Footer Actions -->
        <div class="row q-col-gutter-sm">
          <div class="col-6">
            <q-btn 
              flat 
              no-caps 
              label="Limpiar Filtros" 
              color="negative" 
              class="full-width text-weight-bold touch-btn-48" 
              @click="clearFilters" 
              v-close-popup
            />
          </div>
          <div class="col-6">
            <q-btn 
              unelevated 
              no-caps 
              label="Aplicar Filtros" 
              color="primary" 
              class="full-width text-weight-bold touch-btn-48" 
              v-close-popup
            />
          </div>
        </div>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import { useQuasar } from 'quasar';
import { productService } from '@/services/product.service';
import { categoryService } from '@/services/category.service';
import { useCartStore } from '@/stores/cart';
import { useCurrency } from '@/composables/useCurrency';

const route = useRoute();
const $q = useQuasar();
const cartStore = useCartStore();
const { formatCurrency } = useCurrency();

const loading = ref(true);
const search = ref('');
const selectedCategory = ref<number | null>(null);
const showMobileFilters = ref(false);

const products = ref<any[]>([]);
const categories = ref<any[]>([]);

onMounted(async () => {
  // Read category filter from query params
  if (route.query.categoryId) {
    selectedCategory.value = parseInt(route.query.categoryId as string, 10);
  }

  try {
    const [prodResponse, catResponse] = await Promise.all([
      productService.getAll(),
      categoryService.getAll()
    ]);
    products.value = prodResponse.data;
    categories.value = catResponse.data;
  } catch (err) {
    console.error('Error fetching catalog data:', err);
    $q.notify({
      color: 'negative',
      message: 'No se pudieron cargar los datos del catálogo.',
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

const clearFilters = () => {
  search.value = '';
  selectedCategory.value = null;
};

const filteredProducts = computed(() => {
  return products.value.filter(p => {
    const matchesSearch = 
      !search.value || 
      p.name.toLowerCase().includes(search.value.toLowerCase()) ||
      (p.code && p.code.toLowerCase().includes(search.value.toLowerCase()));
    
    const matchesCategory = 
      selectedCategory.value === null || 
      p.categoryId === selectedCategory.value;

    return matchesSearch && matchesCategory;
  });
});
</script>

<style scoped>
.max-width-lg {
  max-width: 1200px;
}

.sticky-top {
  position: sticky;
  top: 90px;
  z-index: 10;
}

.category-list-item {
  transition: all 0.2s ease;
  padding: 8px 12px;
}

.category-list-item:hover {
  background: rgba(var(--q-primary), 0.05);
}

.bg-primary-light {
  background-color: rgba(var(--q-primary), 0.08) !important;
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

.hover-lift {
  transition: transform 0.25s cubic-bezier(0.2, 0.8, 0.2, 1), box-shadow 0.25s cubic-bezier(0.2, 0.8, 0.2, 1);
}

.hover-lift:hover {
  transform: translateY(-8px);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.12);
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

.min-width-auto {
  min-width: auto;
}

.touch-btn-48 {
  min-height: 48px !important;
}

.shadow-glow {
  box-shadow: 0 4px 14px 0 rgba(var(--q-primary), 0.4);
}

.mobile-filters-card {
  border-top-left-radius: 24px;
  border-top-right-radius: 24px;
}
</style>
