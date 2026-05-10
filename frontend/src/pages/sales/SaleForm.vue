<template>
  <q-page padding>
    <q-card>
      <q-card-section>
        <div class="text-h6">Crear Venta</div>
      </q-card-section>

      <q-form @submit.prevent="onSubmit">
        <q-card-section class="q-gutter-md">
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-4">
              <q-input v-model="sale.ref" label="Referencia" filled readonly />
            </div>
            <div class="col-12 col-md-4">
              <q-select v-model="sale.clientId" label="Cliente" filled :options="clientOptions" emit-value map-options lazy-rules :rules="[val => !!val || 'Requerido']" />
            </div>
            <div class="col-12 col-md-4">
              <q-select v-model="sale.warehouseId" label="Almacén" filled :options="warehouseOptions" emit-value map-options lazy-rules :rules="[val => !!val || 'Requerido']" />
            </div>
          </div>

          <!-- Product Search -->
          <div class="row items-center q-col-gutter-md">
            <div class="col-12 col-md-10">
              <q-select
                v-model="selectedProduct"
                label="Buscar producto por código o nombre"
                filled
                :options="productOptions"
                use-input
                @filter="filterProducts"
                option-value="id"
                option-label="name"
                return-object
              />
            </div>
            <div class="col-12 col-md-2">
              <q-btn color="primary" label="Añadir" @click="addProductToSale" :disable="!selectedProduct" class="full-width" />
            </div>
          </div>

          <!-- Sale Details Table -->
          <q-table
            :rows="sale.details"
            :columns="detailsColumns"
            row-key="productId"
            flat
            bordered
          >
            <template v-slot:body-cell-quantity="props">
              <q-td :props="props">
                <q-input v-model.number="props.row.quantity" type="number" dense @update:model-value="updateTotals" />
              </q-td>
            </template>
            <template v-slot:body-cell-total="props">
              <q-td :props="props">{{ formatCurrency(props.row.quantity * props.row.price) }}</q-td>
            </template>
            <template v-slot:body-cell-actions="props">
                <q-td :props="props">
                    <q-btn flat round color="negative" icon="delete" @click="removeProduct(props.row)" />
                </q-td>
            </template>
          </q-table>
          
          <!-- Totals -->
          <div class="row justify-end q-mt-md">
            <div class="col-12 col-md-4">
                <q-list bordered>
                    <q-item>
                        <q-item-section>Subtotal:</q-item-section>
                        <q-item-section side>{{ formatCurrency(subTotal) }}</q-item-section>
                    </q-item>
                    <q-item>
                        <q-item-section>Total:</q-item-section>
                        <q-item-section side class="text-h6">{{ formatCurrency(sale.grandTotal) }}</q-item-section>
                    </q-item>
                </q-list>
            </div>
          </div>

          <!-- Voucher Section -->
          <q-expansion-item
            icon="receipt"
            label="Información del Comprobante (Opcional)"
            class="q-mt-md"
          >
            <div class="row q-col-gutter-md q-pt-md">
              <div class="col-12 col-md-3">
                <q-input v-model="voucher.voucherType" label="Tipo (Factura A, Ticket B, etc.)" filled />
              </div>
              <div class="col-12 col-md-3">
                <q-input v-model="voucher.voucherNumber" label="Número de Comprobante" filled />
              </div>
              <div class="col-12 col-md-3">
                <q-input v-model="voucher.cae" label="CAE" filled />
              </div>
              <div class="col-12 col-md-3">
                <q-input v-model="voucher.caeExpiration" label="Vencimiento CAE" type="date" stack-label filled />
              </div>
            </div>
          </q-expansion-item>

        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" to="/sales" />
          <q-btn type="submit" color="primary" label="Guardar Venta" :loading="submitting" :disable="submitting || sale.details.length === 0" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { saleService } from '@/services/sale.service'
import { clientService } from '@/services/client.service'
import { warehouseService } from '@/services/warehouse.service'
import { productService } from '@/services/product.service'
import type { Sale, SaleDetail, Product, Voucher } from '@/types'

const $q = useQuasar()
const router = useRouter()
const submitting = ref(false)

const clientOptions = ref<any[]>([])
const warehouseOptions = ref<any[]>([])
const productOptions = ref<any[]>([])
let allProducts: Product[] = []
const selectedProduct = ref<Product | null>(null)

const sale = reactive<Partial<Sale> & { details: SaleDetail[] }>({
  ref: `SL-${Date.now()}`,
  date: new Date().toISOString().substr(0, 10),
  clientId: undefined,
  warehouseId: undefined,
  grandTotal: 0,
  paidAmount: 0,
  status: 'completed',
  paymentStatus: 'paid',
  details: []
})

const voucher = reactive<Partial<Voucher>>({
    voucherType: '',
    voucherNumber: '',
    cae: '',
    caeExpiration: ''
})

const detailsColumns = [
    { name: 'name', label: 'Producto', field: (row: any) => allProducts.find(p => p.id === row.productId)?.name || '', align: 'left' as const },
    { name: 'quantity', label: 'Cantidad', field: 'quantity', align: 'center' as const },
    { name: 'price', label: 'Precio', field: 'price', format: (val: number) => formatCurrency(val), align: 'right' as const },
    { name: 'total', label: 'Total', field: 'total', align: 'right' as const },
    { name: 'actions', label: '', field: 'actions' }
]

const subTotal = computed(() => sale.details.reduce((acc, item) => acc + (item.quantity * item.price), 0))

const updateTotals = () => {
    sale.grandTotal = subTotal.value
}

const addProductToSale = () => {
  if (!selectedProduct.value) return
  
  const existingDetail = sale.details.find(d => d.productId === selectedProduct.value!.id)
  if (existingDetail) {
      existingDetail.quantity++
  } else {
    sale.details.push({
      productId: selectedProduct.value.id!,
      quantity: 1,
      price: selectedProduct.value.price,
      total: selectedProduct.value.price
    })
  }
  updateTotals()
  selectedProduct.value = null
}

const removeProduct = (row: SaleDetail) => {
    const index = sale.details.findIndex(d => d.productId === row.productId)
    if(index > -1) {
        sale.details.splice(index, 1)
    }
    updateTotals()
}

const filterProducts = (val: string, update: (cb: () => void) => void) => {
    if (val === '') {
        update(() => { productOptions.value = [] })
        return
    }
    update(() => {
        const needle = val.toLowerCase()
        productOptions.value = allProducts.filter(v => v.name.toLowerCase().indexOf(needle) > -1 || v.code.toLowerCase().indexOf(needle) > -1)
    })
}

const onSubmit = async () => {
  submitting.value = true
  try {
    const payload = { ...sale }
    if(voucher.voucherType && voucher.voucherNumber && voucher.cae){
        (payload as any).voucher = { ...voucher, issuedAt: new Date() }
    }
    await saleService.create(payload as Sale)
    $q.notify({ color: 'positive', message: 'Venta creada con éxito' })
    router.push('/sales')
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al crear la venta' })
  } finally {
    submitting.value = false
  }
}

const formatCurrency = (val?: number) => new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(val || 0)

onMounted(async () => {
  const [clientsRes, warehousesRes, productsRes] = await Promise.all([
    clientService.getAll(),
    warehouseService.getAll(),
    productService.getAll()
  ])
  clientOptions.value = clientsRes.data.map(c => ({ label: c.name, value: c.id }))
  warehouseOptions.value = warehousesRes.data.map(w => ({ label: w.name, value: w.id }))
  allProducts = productsRes.data
})
</script>
