<template>
  <q-page class="pos-page" :style="{ backgroundColor: 'var(--color-background)' }">
    <div class="row full-height no-wrap">
      <!-- Products Section -->
      <div class="col-grow column q-pa-sm">
        <!-- Search and Categories -->
        <div class="row q-col-gutter-sm q-mb-sm">
          <div class="col-12 col-md-6">
            <q-input
              v-model="search"
              placeholder="Buscar producto o Código (F2)..."
              outlined
              dense
              bg-color="transparent"
              class="pos-search-input"
              @keyup.enter="handleBarcodeScan"
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
              outlined
              dense
              bg-color="transparent"
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
                  <div class="absolute-bottom text-subtitle2 text-center q-pa-xs" :style="{ backgroundColor: 'rgba(var(--color-background-dark-rgb), 0.6)', color: 'var(--color-text-dark)' }">
                    {{ formatCurrency(product.price) }}
                  </div>
                </q-img>
                <q-card-section class="q-pa-sm text-center">
                  <div class="text-caption text-weight-bold ellipsis" :style="{ fontFamily: 'var(--font-family-body)' }">{{ product.name }}</div>
                  <div class="text-grey-7" style="font-size: 10px" :style="{ fontFamily: 'var(--font-family-body)' }">Stock: {{ getStock(product.id!) }}</div>
                </q-card-section>
              </q-card>
            </div>
          </div>
        </q-scroll-area>
      </div>

      <!-- Cart Section -->
      <div class="pos-cart column shadow-2 q-pa-sm" :style="{ backgroundColor: 'var(--color-background-elevated)', borderLeft: '1px solid var(--color-border)' }" style="width: 400px">
        <div class="text-h6 q-mb-md flex items-center" :style="{ fontFamily: 'var(--font-family-display)' }">
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
            class="q-btn--flat"
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
            <div :style="{ fontFamily: 'var(--font-family-body)' }">
              <q-icon name="add_shopping_cart" size="64px" class="q-mb-sm opacity-50" :style="{ color: 'var(--color-text-primary)', opacity: 0.4 }" />
              <div class="text-subtitle1" :style="{ color: 'var(--color-text-primary)', opacity: 0.6 }">El carrito está vacío</div>
              <div class="text-caption" :style="{ color: 'var(--color-text-primary)', opacity: 0.5 }">Selecciona productos para comenzar</div>
            </div>
          </div>
          <q-list v-else separator>
            <q-item v-for="(item, index) in cart" :key="item.productId" class="q-px-sm">
              <q-item-section>
                <q-item-label class="text-weight-medium" :style="{ fontFamily: 'var(--font-family-body)' }">{{ getProductName(item.productId) }}</q-item-label>
                <q-item-label caption class="text-tabular-nums" :style="{ fontFamily: 'var(--font-family-body)' }">{{ formatCurrency(item.price) }} x {{ item.quantity }}</q-item-label>
              </q-item-section>
              <q-item-section side>
                <div class="row items-center no-wrap q-gutter-x-xs">
                  <q-btn
                    size="sm"
                    round
                    flat
                    color="primary"
                    icon="remove"
                    @click="decrementQty(index)"
                    aria-label="Disminuir cantidad"
                  />
                  <div class="q-mx-sm text-weight-bold text-subtitle1 text-tabular-nums" :style="{ minWidth: '20px', textAlign: 'center', fontFamily: 'var(--font-family-body)' }">
                    {{ item.quantity }}
                  </div>
                  <q-btn
                    size="sm"
                    round
                    flat
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

        <q-separator :style="{ backgroundColor: 'var(--color-border)' }" />

        <div class="q-py-md">
          <div class="row justify-between text-subtitle1 q-mb-sm" :style="{ fontFamily: 'var(--font-family-body)' }">
            <span>Subtotal:</span>
            <span class="text-tabular-nums">{{ formatCurrency(subtotal) }}</span>
          </div>
          
          <div class="row q-col-gutter-sm q-mb-sm">
             <div class="col-4">
                <q-input v-model.number="formData.taxRate" label="Impuesto %" type="number" dense outlined input-class="text-right" @update:model-value="calculateTotals" />
             </div>
             <div class="col-4">
                <q-input v-model.number="formData.discount" label="Descuento $" type="number" dense outlined input-class="text-right" @update:model-value="calculateTotals" />
             </div>
             <div class="col-4">
                <q-input v-model.number="formData.shipping" label="Envío $" type="number" dense outlined input-class="text-right" @update:model-value="calculateTotals" />
             </div>
          </div>

          <q-separator class="q-my-sm" :style="{ backgroundColor: 'var(--color-border)' }"/>
          <div class="row justify-between text-h5 text-weight-bolder" :style="{ fontFamily: 'var(--font-family-display)', color: 'var(--color-primary)' }">
            <span>Total:</span>
            <span class="text-tabular-nums">{{ formatCurrency(total) }}</span>
          </div>
        </div>

        <q-btn
          label="PAGAR AHORA"
          color="primary"
          class="full-width text-weight-bold q-btn--elevated"
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
        <q-card-section :style="{ backgroundColor: 'var(--color-primary)', color: 'var(--color-text-dark)' }">
          <div class="text-h6" :style="{ fontFamily: 'var(--font-family-display)' }">Finalizar Venta</div>
        </q-card-section>

        <q-card-section class="q-pa-lg">
          <div class="text-h4 text-center q-mb-lg" :style="{ fontFamily: 'var(--font-family-display)', color: 'var(--color-primary)' }">
            Total: {{ formatCurrency(total) }}
          </div>
          
          <div class="row q-col-gutter-md">
            <div class="col-12">
              <q-input v-model="formData.notes" label="Notas de la Venta (Opcional)" type="textarea" autogrow outlined dense class="q-mb-md" />
            </div>
            <div class="col-12">
              <q-input
                v-model.number="paidAmount"
                label="Monto Recibido"
                type="number"
                outlined
                dense
                prefix="$"
                input-class="text-h6"
                @update:model-value="calculateChange"
              />
            </div>
            
            <div class="col-12 flex q-gutter-sm justify-center q-mb-md">
                <q-btn v-for="amt in quickAmounts" :key="amt" :label="formatCurrency(amt)" color="primary" outline @click="setQuickAmount(amt)" />
                <q-btn :label="`Exacto (${formatCurrency(total)})`" color="primary" outline @click="setQuickAmount(total)" />
            </div>

            <div class="col-12">
              <div class="row justify-between text-h6" :style="{ fontFamily: 'var(--font-family-body)' }">
                <span>Cambio:</span>
                <span :class="change < 0 ? 'text-negative' : 'text-positive'">{{ formatCurrency(change) }}</span>
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
import { useRouter } from 'vue-router'
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

import { useCurrency } from '@/composables/useCurrency';

const $q = useQuasar()
const router = useRouter()
const { formatCurrency } = useCurrency();


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
  date: new Date().toISOString().split('T')[0],
  taxRate: 0,
  discount: 0,
  shipping: 0,
  notes: ''
})

const quickAmounts = [10, 20, 50, 100]

const calculateTotals = () => {
    // Totals are computed now, so just trigger reactivity if needed
    paidAmount.value = total.value
    calculateChange()
}

const filteredProducts = computed(() => {
  return allProducts.value.filter((p: Product) => {
    const matchesSearch = p.name.toLowerCase().includes(search.value.toLowerCase()) || 
                           (p.code && p.code.toLowerCase().includes(search.value.toLowerCase()))
    const matchesCategory = !selectedCategory.value || p.categoryId === selectedCategory.value
    return matchesSearch && matchesCategory
  })
})

const subtotal = computed(() => cart.value.reduce((sum: number, item: SaleDetail) => sum + item.total, 0))
const total = computed(() => {
    const taxAmount = subtotal.value * ((formData.taxRate || 0) / 100)
    return subtotal.value + taxAmount + (formData.shipping || 0) - (formData.discount || 0)
})

const setQuickAmount = (amount: number) => {
    paidAmount.value = amount
    calculateChange()
}

const handleBarcodeScan = () => {
  if (!search.value) return
  const exactMatch = allProducts.value.find(p => p.code === search.value)
  if (exactMatch) {
    addToCart(exactMatch)
    search.value = ''
  } else {
    $q.notify({ color: 'warning', message: 'Producto no encontrado por código', timeout: 1000 })
  }
}

const getProductName = (id: number) => allProducts.value.find((p: Product) => p.id === id)?.name || ''
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
    allProducts.value.forEach((p: Product) => {
      stocks.value[p.id!] = Math.floor(Math.random() * 50) + 10 // Placeholder
    })
  } catch (error) {}
}

const addToCart = (product: Product) => {
  const existing = cart.value.find((item: SaleDetail) => item.productId === product.id)
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
  formData.taxRate = 0
  formData.discount = 0
  formData.shipping = 0
  formData.notes = ''
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
      ref: `POS-${Date.now()}`,
      isPos: true,
      grandTotal: total.value,
      paidAmount: total.value,
      paymentStatus: 'paid',
      status: 'completed',
      details: cart.value
    }

    const res = await saleService.create(sale)
    const saleId = res.data?.id || 1

    clearCart()
    showCheckoutDialog.value = false

    $q.dialog({
      title: 'Venta Exitosa',
      message: `Venta realizada con éxito.\nTotal: ${formatCurrency(total.value)}\nCambio: ${formatCurrency(change.value)}`,
      persistent: true,
      ok: { label: 'Imprimir Comprobante', color: 'primary', unelevated: true },
      cancel: { label: 'Cerrar', flat: true, color: 'grey' }
    }).onOk(() => {
      router.push(`/sales/print/${saleId}`)
    }).onCancel(() => {
      // just close
    })
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
  transition: transform 0.2s cubic-bezier(0.4, 0, 0.2, 1), box-shadow 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  border-radius: 12px; /* Consistent card border-radius */
  overflow: hidden;
  border: 1px solid var(--color-border);
  background-color: var(--color-background-elevated);

  &:hover {
    transform: translateY(-2px) scale(1.01);
    box-shadow: 0 6px 12px rgba(0,0,0,0.1);
    z-index: 1;
  }
  .body--dark & {
    border-color: var(--color-border-dark);
    background-color: var(--color-background-elevated-dark);
    &:hover {
      box-shadow: 0 8px 16px rgba(0,0,0,0.3);
    }
  }
}

.pos-search-input {
  .q-field__control {
    background-color: var(--color-background-elevated) !important;
    border-color: var(--color-border) !important;
    .body--dark & {
      background-color: var(--color-background-elevated-dark) !important;
      border-color: var(--color-border-dark) !important;
    }
  }
  .q-field__marginal {
    color: var(--color-text-primary) !important;
  }
}

.pos-cart {
  border-left: 1px solid var(--color-border);
}
</style>
