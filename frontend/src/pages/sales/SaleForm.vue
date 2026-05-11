<template>
  <q-page padding>
    <q-card>
      <q-card-section>
        <div class="text-h6" :style="{ fontFamily: 'var(--font-family-display)' }">{{ isEdit ? 'Ver Venta' : 'Crear Venta' }}</div>
      </q-card-section>

      <q-form @submit.prevent="onSubmit">
        <q-card-section class="q-pa-md">
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-4">
              <q-input v-model="sale.ref" label="Referencia" outlined dense readonly />
            </div>
            <div class="col-12 col-md-4">
              <q-select v-model="sale.clientId" label="Cliente" outlined dense :options="clientOptions" emit-value map-options lazy-rules :rules="[val => !!val || 'Requerido']" :readonly="isEdit" />
            </div>
            <div class="col-12 col-md-4">
              <q-select v-model="sale.warehouseId" label="Almacén" outlined dense :options="warehouseOptions" emit-value map-options lazy-rules :rules="[val => !!val || 'Requerido']" :readonly="isEdit" />
            </div>
          </div>

          <!-- Product Search -->
          <div class="row items-center q-col-gutter-md">
            <div class="col-12 col-md-10">
<q-select
                v-model="selectedProduct"
                label="Buscar producto por código o nombre"
                :options="productOptions"
                use-input
                @filter="filterProducts"
                option-value="id"
                option-label="name"
                outlined
                dense
                :readonly="isEdit"
              />
            </div>
            <div class="col-12 col-md-2">
              <q-btn color="primary" label="Añadir" @click="addProductToSale" :disable="!selectedProduct || isEdit" class="full-width" />
            </div>
          </div>

          <!-- Sale Details Table -->
          <div class="app-table-container">
            <q-table
              :rows="sale.details"
              :columns="detailsColumns"
              row-key="productId"
              flat
              bordered
            >
            <template v-slot:body-cell-quantity="props">
              <q-td :props="props">
                <q-input v-model.number="props.row.quantity" type="number" dense @update:model-value="updateTotals" :readonly="isEdit" />
              </q-td>
            </template>
            <template v-slot:body-cell-total="props">
              <q-td :props="props">{{ formatCurrency(props.row.quantity * props.row.price) }}</q-td>
            </template>
            <template v-slot:body-cell-actions="props">
                <q-td :props="props">
                    <q-btn flat round color="negative" icon="delete" @click="removeProduct(props.row)" :disable="isEdit" />
                </q-td>
            </template>
          </q-table>
          </div>
          
          <!-- Totals -->
          <div class="row justify-end q-mt-md">
            <div class="col-12 col-md-4">
                <q-list bordered class="totals-list">
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
                <q-input v-model="voucher.voucherType" label="Tipo (Factura A, Ticket B, etc.)" outlined dense :readonly="isEdit" />
              </div>
              <div class="col-12 col-md-3">
                <q-input v-model="voucher.voucherNumber" label="Número de Comprobante" outlined dense :readonly="isEdit" />
              </div>
              <div class="col-12 col-md-3">
                <q-input v-model="voucher.cae" label="CAE" outlined dense :readonly="isEdit" />
              </div>
              <div class="col-12 col-md-3">
                <q-input v-model="voucher.caeExpiration" label="Vencimiento CAE" type="date" stack-label outlined dense :readonly="isEdit" />
              </div>
            </div>
          </q-expansion-item>

        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" to="/sales" />
          <q-btn type="submit" color="primary" label="Guardar Venta" :loading="submitting" :disable="submitting || sale.details.length === 0 || isEdit" />
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

import { useCurrency } from '@/composables/useCurrency';

const $q = useQuasar()
const router = useRouter()
const submitting = ref(false)
const { formatCurrency } = useCurrency();


const clientOptions = ref<any[]>([])
const warehouseOptions = ref<any[]>([])
const productOptions = ref<any[]>([])
let allProducts: Product[] = []
const selectedProduct = ref<Product | null>(null)
const isEdit = ref(false) // Add this

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
    id: undefined,
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

const subTotal = computed(() => sale.details.reduce((acc: number, item: SaleDetail) => acc + (item.quantity * item.price), 0))

const updateTotals = () => {
    sale.grandTotal = subTotal.value
}

const addProductToSale = () => {
  if (!selectedProduct.value) return
  
  const existingDetail = sale.details.find((d: SaleDetail) => d.productId === selectedProduct.value!.id)
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
    const index = sale.details.findIndex((d: SaleDetail) => d.productId === row.productId)
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
        (payload as any).voucher = { ...voucher, issuedAt: voucher.issuedAt || new Date().toISOString().substr(0, 10) }
    }
    const res = await saleService.create(payload as Sale)
    const saleId = res.data?.id || 1

    $q.dialog({
      title: 'Venta Exitosa',
      message: `La venta se ha registrado correctamente. ¿Desea imprimir el comprobante?`,
      persistent: true,
      ok: { label: 'Imprimir', color: 'primary', unelevated: true, icon: 'print' },
      cancel: { label: 'No, gracias', flat: true, color: 'grey' }
    }).onOk(() => {
      router.push(`/sales/print/${saleId}`)
    }).onCancel(() => {
      router.push('/sales')
    })
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al crear la venta' })
  } finally {
    submitting.value = false
  }
}

onMounted(async () => {
  const [clientsRes, warehousesRes, productsRes] = await Promise.all([
    clientService.getAll(),
    warehouseService.getAll(),
    productService.getAll()
  ])
  clientOptions.value = clientsRes.data.map(c => ({ label: c.name, value: c.id }))
  warehouseOptions.value = warehousesRes.data.map(w => ({ label: w.name, value: w.id }))
  allProducts = productsRes.data

  // Check if we are in edit mode
  const saleId = Number(router.currentRoute.value.params.id)
  if (saleId) {
    isEdit.value = true
    try {
      const res = await saleService.getById(saleId)
      const saleData = res.data

      if (saleData) {
        // Populate formData
        Object.assign(sale, { 
            ...saleData, 
            details: saleData.details || [] 
        })
        // Populate voucher
        if (saleData.voucher) {
          Object.assign(voucher, saleData.voucher)
        }
        updateTotals()
      }
    } catch (error) {
      $q.notify({ color: 'negative', message: 'Error al cargar la venta para edición' })
      router.push('/sales')
    }
  }
})</script>

<style lang="scss">
.totals-list {
  border-radius: 12px;
  background-color: var(--color-background-elevated);
  border: 1px solid var(--color-border);
  padding: 8px 16px;

  .body--dark & {
    background-color: var(--color-background-elevated);
    border-color: var(--color-border-dark);
  }

  .q-item {
    padding: 8px 0;
    font-family: var(--font-family-body);
    color: var(--color-text-primary);
  }

  .text-h6 {
    font-family: var(--font-family-display);
    font-weight: 700;
    color: var(--color-primary); /* Highlight total */
  }
}
</style>
