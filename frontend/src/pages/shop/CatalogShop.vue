<template>
  <q-page class="q-py-xl container q-mx-auto q-px-md max-width-lg">
    <!-- Header Page Section -->
    <div class="row items-center justify-between q-mb-xl">
      <div>
        <h3 class="text-weight-bold q-my-none text-primary">Catálogo de Productos</h3>
        <p class="text-subtitle1 text-grey-6 q-mt-xs">Explora nuestra colección y compra con preventa QR</p>
      </div>

      <!-- Quick Cart Info -->
      <q-chip outline color="primary" icon="shopping_cart" class="text-weight-bold q-py-md cursor-pointer" to="/checkout">
        Carrito: {{ cartStore.totalItems }} items / {{ formatPrice(cartStore.totalAmount) }}
      </q-chip>
    </div>

    <div class="row q-col-gutter-lg">
      <!-- Lateral Filters Column (gt-xs) -->
      <div class="col-12 col-md-3">
        <q-card flat class="glass-card q-pa-md sticky-top">
          <!-- Search input -->
          <div class="q-mb-lg">
            <span class="text-weight-bold text-subtitle1 block q-mb-xs">Buscar</span>
            <q-input 
              v-model="search" 
              outlined 
              dense 
              placeholder="Buscar producto o código..." 
              clearable
              bg-color="transparent"
            >
              <template #prepend>
                <q-icon name="search" color="primary" />
              </template>
            </q-input>
          </div>

          <!-- Category filter -->
          <div>
            <div class="row items-center justify-between q-mb-sm">
              <span class="text-weight-bold text-subtitle1">Categorías</span>
              <q-btn flat dense no-caps label="Limpiar" color="primary" size="sm" v-if="selectedCategory" @click="selectedCategory = null" />
            </div>

            <q-list dense class="q-pl-none">
              <q-item 
                clickable 
                v-for="cat in categories" 
                :key="cat.id"
                :active="selectedCategory === cat.id"
                active-class="bg-primary-light text-primary text-weight-bold"
                class="category-list-item rounded-borders"
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

      <!-- Products Grid Column -->
      <div class="col-12 col-md-9">
        <!-- Mobile Filters Accordion (lt-md) -->
        <q-expansion-item
          icon="tune"
          label="Filtros y Búsqueda"
          header-class="bg-primary text-white text-weight-bold rounded-borders q-mb-md lt-md"
          class="lt-md q-mb-md overflow-hidden"
          style="border-radius: 8px;"
        >
          <q-card class="glass-card q-pa-md">
            <q-input 
              v-model="search" 
              outlined 
              dense 
              placeholder="Buscar producto o código..." 
              clearable
              class="q-mb-md"
            >
              <template #prepend>
                <q-icon name="search" />
              </template>
            </q-input>

            <q-select
              v-model="selectedCategory"
              :options="categories"
              option-value="id"
              option-label="name"
              emit-value
              map-options
              outlined
              dense
              label="Filtrar por Categoría"
              clearable
            />
          </q-card>
        </q-expansion-item>

        <!-- Loader -->
        <div v-if="loading" class="row justify-center q-py-xl">
          <q-spinner-dots color="primary" size="50px" />
        </div>

        <!-- No products found -->
        <div v-else-if="filteredProducts.length === 0" class="text-center q-py-xl glass-card">
          <q-icon name="search_off" size="60px" color="grey-5" />
          <p class="text-h6 text-grey-6 q-mt-md">No se encontraron productos para los criterios seleccionados.</p>
          <q-btn label="Limpiar Filtros" color="primary" no-caps unelevated class="q-mt-sm" @click="clearFilters" />
        </div>

        <!-- Products Grid -->
        <div v-else class="row q-col-gutter-lg">
          <div 
            v-for="product in filteredProducts" 
            :key="product.id" 
            class="col-12 col-sm-6 col-md-4"
          >
            <q-card class="product-card glass-card hover-lift flex column justify-between">
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
      !selectedCategory.value || 
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
}

.category-list-item {
  transition: all 0.2s ease;
  padding: 8px 12px;
  margin-bottom: 4px;
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
</style>
