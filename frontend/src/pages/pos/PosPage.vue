<template>
  <q-page class="pos-page bg-grey-2">
    <div class="row full-height no-wrap">
      <!-- Products Section -->
      <div class="col-grow column q-pa-sm">
        <!-- Search and Categories -->
        <div class="row q-col-gutter-sm q-mb-sm">
          <div class="col-12 col-md-6">
            <q-input
              v-model="search"
              placeholder="Buscar producto (F2)..."
              filled
              dense
              bg-color="white"
              class="pos-search-input"
            >
              <template v-slot:prepend>
                <q-icon name="search" />
              </template>
            </q-input>
          </div>

          <div class="col-12 col-md-6">
            <q-select
              v-model="selectedCategory"
              :options="categories"
              option-label="name"
              option-value="id"
              label="Categoría"
              filled
              dense
              bg-color="white"
              clearable
              emit-value
              map-options
            />
          </div>
        </div>

        <!-- Products Grid -->
        <q-scroll-area class="col">
          <div class="row q-col-gutter-sm">
            <div
              v-for="product in filteredProducts"
              :key="product.id"
              class="col-6 col-sm-4 col-md-3"
            >
              <q-card class="product-card cursor-pointer" @click="addToCart(product)">
                <q-img
                  :src="product.image || 'https://cdn.quasar.dev/img/parallax2.jpg'"
                  :ratio="1"
                  class="bg-grey-3"
                >
                  <div class="absolute-bottom text-subtitle2 text-center bg-black-50 q-pa-xs">
                    ${{ product.price.toFixed(2) }}
                  </div>
                </q-img>
                <q-card-section class="q-pa-sm text-center">
                  <div class="text-caption text-weight-bold ellipsis">{{ product.name }}</div>
                  <div class="text-grey-7" style="font-size: 10px">Stock: {{ getStock(product.id!) }}</div>
                </q-card-section>
              </q-card>
            </div>
          </div>
        </q-scroll-area>
      </div>

      <!-- Cart Section -->
      <div class="pos-cart column bg-white shadow-2 q-pa-sm" style="width: 400px">
        <div class="text-h6 q-mb-md flex items-center">
          <q-icon name="shopping_cart" color="primary" class="q-mr-sm" />
          Carrito
          <q-space />
          <q-btn
            flat
            round
            icon="delete_sweep"
            color="negative"
            @click="clearCart"
            aria-label="Vaciar carrito"
          >
            <q-tooltip>Vaciar carrito</q-tooltip>
          </q-btn>
        </div>

        <q-select
          v-model="formData.clientId"
          :options="clients"
          option-label="name"
          option-value="id"
          label="Cliente"
          dense
          outlined
          class="q-mb-md"
          emit-value
          map-options
        />

        <q-select
          v-model="formData.warehouseId"
          :options="warehouses"
          option-label="name"
          option-value="id"
          label="Almacén"
          dense
          outlined
          class="q-mb-md"
          emit-value
          map-options
          @update:model-value="fetchStocks"
        />

        <q-scroll-area class="col q-mb-md">
          <div v-if="cart.length === 0" class="full-height flex flex-center text-grey-6 q-pa-md text-center">
            <div>
              <q-icon name="add_shopping_cart" size="64px" class="q-mb-sm opacity-50" />
              <div class="text-subtitle1">El carrito está vacío</div>
              <div class="text-caption">Selecciona productos para comenzar</div>
            </div>
          </div>
          <q-list v-else separator>
            <q-item v-for="(item, index) in cart" :key="item.productId" class="q-px-sm">
              <q-item-section>
                <q-item-label class="text-weight-medium">{{ getProductName(item.productId) }}</q-item-label>
                <q-item-label caption class="text-tabular-nums">${{ item.price.toFixed(2) }} x {{ item.quantity }}</q-item-label>
              </q-item-section>
              <q-item-section side>
                <div class="row items-center no-wrap q-gutter-x-xs">
                  <q-btn
                    size="sm"
                    round
                    unelevated
                    color="primary"
                    icon="remove"
                    @click="decrementQty(index)"
                    aria-label="Disminuir cantidad"
                  />
                  <div class="q-mx-sm text-weight-bold text-subtitle1 text-tabular-nums" style="min-width: 20px; text-align: center;">
                    {{ item.quantity }}
                  </div>
                  <q-btn
                    size="sm"
                    round
                    unelevated
                    color="primary"
                    icon="add"
                    @click="incrementQty(index)"
                    aria-label="Aumentar cantidad"
                  />
                  <q-btn
                    size="sm"
                    flat
                    round
                    color="negative"
                    icon="close"
                    class="q-ml-xs"
                    @click="removeFromCart(index)"
                    aria-label="Quitar del carrito"
                  />
                </div>
              </q-item-section>
            </q-item>
          </q-list>
        </q-scroll-area>

        <q-separator />

        <div class="q-py-md">
          <div class="row justify-between text-subtitle1">
            <span>Subtotal:</span>
            <span class="text-tabular-nums">${{ subtotal.toFixed(2) }}</span>
          </div>
          <div class="row justify-between text-subtitle1">
            <span>Impuesto (0%):</span>
            <span class="text-tabular-nums">$0.00</span>
          </div>
          <q-separator class="q-my-sm" />
          <div class="row justify-between text-h5 text-weight-bolder text-primary">
            <span>Total:</span>
            <span class="text-tabular-nums">${{ total.toFixed(2) }}</span>
          </div>
        </div>

        <q-btn
          label="PAGAR AHORA"
          color="accent"
          class="full-width text-weight-bold"
          size="lg"
          unelevated
          @click="showCheckoutDialog = true"
          :disable="cart.length === 0 || !formData.clientId || !formData.warehouseId"
        />
      </div>

    </div>

    <!-- Checkout Dialog -->
    <q-dialog v-model="showCheckoutDialog" persistent>
      <q-card style="width: 500px">
        <q-card-section class="bg-primary text-white">
          <div class="text-h6">Finalizar Venta</div>
        </q-card-section>

        <q-card-section class="q-pa-lg">
          <div class="text-h4 text-center q-mb-lg text-primary text-weight-bold">
            Total: ${{ total.toFixed(2) }}
          </div>
          
          <div class="row q-col-gutter-md">
            <div class="col-12">
              <q-input
                v-model.number="paidAmount"
                label="Monto Recibido"
                type="number"
                filled
                prefix="$"
                input-class="text-h6"
                @update:model-value="calculateChange"
              />
            </div>
            <div class="col-12">
              <div class="row justify-between text-h6">
                <span>Cambio:</span>
                <span :class="change < 0 ? 'text-negative' : 'text-positive'">${{ change.toFixed(2) }}</span>
              </div>
            </div>
          </div>
        </q-card-section>

        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat label="Regresar" color="primary" v-close-popup />
          <q-btn
            label="CONFIRMAR VENTA"
            color="primary"
            unelevated
            @click="submitSale"
            :loading="saving"
            :disable="paidAmount < total"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'

import { useQuasar } from 'quasar'

import { productService } from '@/services/product.service';
import type { Product } from '@/types'
import { categoryService } from '@/services/category.service';
import type { Category } from '@/types'
import { warehouseService } from '@/services/warehouse.service';
import type { Warehouse } from '@/types'
import { clientService } from '@/services/client.service';
import type { Client } from '@/types'
import { saleService } from '@/services/sale.service';
import type { Sale, SaleDetail } from '@/types'

const $q = useQuasar()


const search = ref('')
const selectedCategory = ref(null)
const categories = ref<Category[]>([])
const allProducts = ref<Product[]>([])
const warehouses = ref<Warehouse[]>([])
const clients = ref<Client[]>([])
const stocks = ref<Record<number, number>>({})

const cart = ref<SaleDetail[]>([])
const showCheckoutDialog = ref(false)
const paidAmount = ref(0)
const change = ref(0)
const saving = ref(false)

const formData = reactive({
  clientId: 0,
  warehouseId: 0,
  date: new Date().toISOString().split('T')[0]
})

const filteredProducts = computed(() => {
  return allProducts.value.filter(p => {
    const matchesSearch = p.name.toLowerCase().includes(search.value.toLowerCase()) || 
                         (p.code && p.code.toLowerCase().includes(search.value.toLowerCase()))
    const matchesCategory = !selectedCategory.value || p.categoryId === selectedCategory.value
    return matchesSearch && matchesCategory
  })
})

const subtotal = computed(() => cart.value.reduce((sum, item) => sum + item.total, 0))
const total = computed(() => subtotal.value)

const getProductName = (id: number) => allProducts.value.find(p => p.id === id)?.name || ''
const getStock = (productId: number) => stocks.value[productId] || 0

const fetchData = async () => {
  try {
    const [pRes, cRes, wRes, clRes] = await Promise.all([
      productService.getAll(),
      categoryService.getAll(),
      warehouseService.getAll(),
      clientService.getAll()
    ])
    allProducts.value = pRes.data
    categories.value = cRes.data
    warehouses.value = wRes.data
    clients.value = clRes.data

    if (warehouses.value.length > 0) {
      formData.warehouseId = warehouses.value[0].id!
      fetchStocks()
    }
    if (clients.value.length > 0) {
      formData.clientId = clients.value[0].id!
    }
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar datos' })
  }
}

const fetchStocks = async () => {
  if (!formData.warehouseId) return
  try {
    // We would need an endpoint to get all stocks for a warehouse
    // For now we'll assume products have their global stock or we fetch individually
    // Ideally: const response = await warehouseService.getStocks(formData.warehouseId)
    // Placeholder logic:
    allProducts.value.forEach(p => {
      stocks.value[p.id!] = Math.floor(Math.random() * 50) + 10 // Placeholder
    })
  } catch (error) {}
}

const addToCart = (product: Product) => {
  const existing = cart.value.find(item => item.productId === product.id)
  if (existing) {
    existing.quantity++
    existing.total = existing.quantity * existing.price
  } else {
    cart.value.push({
      productId: product.id!,
      quantity: 1,
      price: product.price,
      total: product.price
    })
  }
  paidAmount.value = total.value
  calculateChange()
}

const incrementQty = (index: number) => {
  cart.value[index].quantity++
  cart.value[index].total = cart.value[index].quantity * cart.value[index].price
  paidAmount.value = total.value
}

const decrementQty = (index: number) => {
  if (cart.value[index].quantity > 1) {
    cart.value[index].quantity--
    cart.value[index].total = cart.value[index].quantity * cart.value[index].price
    paidAmount.value = total.value
  }
}

const removeFromCart = (index: number) => {
  cart.value.splice(index, 1)
  paidAmount.value = total.value
}

const clearCart = () => {
  cart.value = []
  paidAmount.value = 0
}

// Keyboard shortcuts
const handleKeydown = (e: KeyboardEvent) => {
  if (e.key === 'F2') {
    e.preventDefault()
    // Focus search input (we need a ref for this)
    const searchInput = document.querySelector('.pos-search-input input') as HTMLInputElement
    if (searchInput) searchInput.focus()
  } else if (e.key === 'F4') {
    e.preventDefault()
    if (cart.value.length > 0 && formData.clientId && formData.warehouseId) {
      showCheckoutDialog.value = true
    }
  } else if (e.key === 'Escape') {
    if (showCheckoutDialog.value) {
      showCheckoutDialog.value = false
    } else if (search.value) {
      search.value = ''
    }
  }
}

const calculateChange = () => {
  change.value = paidAmount.value - total.value
}

const submitSale = async () => {
  saving.value = true
  try {
    const sale: Sale = {
      ...formData,
      isPos: true,
      grandTotal: total.value,
      paidAmount: total.value,
      paymentStatus: 'paid',
      status: 'completed',
      details: cart.value
    }

    await saleService.create(sale)
    $q.notify({
      color: 'positive',
      message: 'Venta realizada con éxito',
      icon: 'check',
      timeout: 1000
    })
    
    clearCart()
    showCheckoutDialog.value = false
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al procesar la venta' })
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  fetchData()
  window.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown)
})
</script>


<style lang="scss" scoped>
.pos-page {
  height: calc(100vh - 50px);
}
.product-card {
  transition: transform 0.2s;
  &:hover {
    transform: scale(1.02);
    z-index: 1;
  }
}
.bg-black-50 {
  background: rgba(0, 0, 0, 0.5);
  color: white;
}
.pos-cart {
  border-left: 1px solid #ddd;
}
</style>
