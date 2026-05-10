<template>
  <q-page padding>
    <q-card>
      <q-card-section>
        <div class="text-h6">Registrar Compra</div>
      </q-card-section>

      <q-form @submit.prevent="onSubmit">
        <q-card-section class="q-pa-md">
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-4">
              <q-input v-model="formData.ref" label="Referencia" outlined dense readonly />
            </div>
            <div class="col-12 col-md-4">
              <q-select
                v-model="formData.providerId"
                label="Proveedor"
                outlined
                dense
                :options="providerOptions"
                emit-value
                map-options
                lazy-rules
                :rules="[val => !!val || 'Requerido']"
              />
            </div>
            <div class="col-12 col-md-4">
              <q-select
                v-model="formData.warehouseId"
                label="Almacén de Destino"
                outlined
                dense
                :options="warehouseOptions"
                emit-value
                map-options
                lazy-rules
                :rules="[val => !!val || 'Requerido']"
              />
            </div>
          </div>

          <!-- Product Search -->
          <div class="row items-center q-col-gutter-md">
            <div class="col-12 col-md-10">
              <q-select
                v-model="selectedProduct"
                label="Buscar producto por código o nombre"
                outlined
                dense
                :options="productOptions"
                use-input
                @filter="filterProducts"
                option-value="id"
                option-label="name"
                return-object
              />
            </div>
            <div class="col-12 col-md-2">
              <q-btn color="primary" label="Añadir" @click="addProductToPurchase" :disable="!selectedProduct" class="full-width" />
            </div>
          </div>

          <!-- Purchase Details Table -->
          <q-table
            :rows="formData.details"
            :columns="detailsColumns"
            row-key="productId"
            flat
            bordered
          >
            <template v-slot:body-cell-cost="props">
              <q-td :props="props">
                <q-input v-model.number="props.row.cost" type="number" dense @update:model-value="updateTotals" />
              </q-td>
            </template>
            <template v-slot:body-cell-quantity="props">
              <q-td :props="props">
                <q-input v-model.number="props.row.quantity" type="number" dense @update:model-value="updateTotals" />
              </q-td>
            </template>
            <template v-slot:body-cell-total="props">
              <q-td :props="props">{{ formatCurrency(props.row.quantity * props.row.cost) }}</q-td>
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
                        <q-item-section side class="text-h6">{{ formatCurrency(formData.grandTotal) }}</q-item-section>
                    </q-item>
                    <q-item>
                        <q-item-section>Total:</q-item-section>
                        <q-item-section side class="text-h6">{{ formatCurrency(formData.grandTotal) }}</q-item-section>
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
                <q-input v-model="voucher.voucherType" label="Tipo (Factura A, Ticket B, etc.)" outlined dense />
              </div>
              <div class="col-12 col-md-3">
                <q-input v-model="voucher.voucherNumber" label="Número de Comprobante" outlined dense />
              </div>
              <div class="col-12 col-md-3">
                <q-input v-model="voucher.cae" label="CAE" outlined dense />
              </div>
              <div class="col-12 col-md-3">
                <q-input v-model="voucher.caeExpiration" label="Vencimiento CAE" type="date" stack-label outlined dense />
              </div>
            </div>
          </q-expansion-item>

        </q-card-section>

        <q-card-actions align="right">
          <q-btn flat label="Cancelar" to="/purchases" />
          <q-btn type="submit" color="primary" label="Guardar Compra" :loading="saving" :disable="saving || formData.details.length === 0" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { purchaseService } from '@/services/purchase.service'
import { providerService } from '@/services/provider.service'
import { warehouseService } from '@/services/warehouse.service'
import { productService } from '@/services/product.service'
import type { Purchase, PurchaseDetail, Product, Voucher } from '@/types'


import { useCurrency } from '@/composables/useCurrency';

const $q = useQuasar()
const router = useRouter()
const { formatCurrency, currencySymbol } = useCurrency();
const saving = ref(false)

const providerOptions = ref<any[]>([])
const warehouseOptions = ref<any[]>([])
const productOptions = ref<any[]>([])
let allProducts: Product[] = []
const selectedProduct = ref<Product | null>(null)


const formData = reactive<Partial<Purchase> & { details: PurchaseDetail[] }>({
  ref: `PR-${Date.now()}`,
  date: new Date().toISOString().substr(0, 10),
  providerId: undefined,
  warehouseId: undefined,
  grandTotal: 0,
  paidAmount: 0,
  status: 'received',
  paymentStatus: 'paid',
  details: []
})

const voucher = reactive<Partial<Voucher>>({
    voucherType: '',
    voucherNumber: '',
    cae: '',
    caeExpiration: ''
})

const subTotal = ref(0);

const detailsColumns = [
  { name: 'name', label: 'Producto', field: 'productName', align: 'left' as const },
  { name: 'cost', label: `Costo (${currencySymbol.value})`, field: 'cost', align: 'right' as const },
  { name: 'quantity', label: 'Cantidad', field: 'quantity', align: 'center' as const },
  { name: 'total', label: 'Total', field: 'total', align: 'right' as const },
  { name: 'actions', label: '', field: 'actions', align: 'center' as const }
]

const updateTotals = () => {
  subTotal.value = formData.details.reduce((acc, detail) => acc + (detail.quantity * detail.cost), 0)
  formData.grandTotal = subTotal.value - (formData.discount || 0) + (formData.shipping || 0)
}

const addProductToPurchase = () => {
  if (!selectedProduct.value) return
  
  const existingDetail = formData.details.find((d: PurchaseDetail) => d.productId === selectedProduct.value!.id)
  if (existingDetail) {
      existingDetail.quantity++
  } else {
    formData.details.push({
      productId: selectedProduct.value.id!,
      cost: selectedProduct.value.cost,
      quantity: 1,
      total: selectedProduct.value.cost,
      productName: selectedProduct.value.name // Add product name for display
    })
  }
  updateTotals()
  selectedProduct.value = null
}

const removeProduct = (row: PurchaseDetail) => {
    const index = formData.details.findIndex((d: PurchaseDetail) => d.productId === row.productId)
    if(index > -1) {
        formData.details.splice(index, 1)
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
  saving.value = true
  try {
    const payload = { ...formData }
    if(voucher.voucherType && voucher.voucherNumber && voucher.cae){
        (payload as any).voucher = { ...voucher, issuedAt: new Date().toISOString().substr(0, 10) }
    }
    const res = await purchaseService.create(payload as Purchase)
    const purchaseId = res.data?.id || 1
    
    $q.dialog({
      title: 'Compra Exitosa',
      message: `La compra se ha registrado correctamente. ¿Desea imprimir el comprobante?`,
      persistent: true,
      ok: { label: 'Imprimir', color: 'primary', unelevated: true, icon: 'print' },
      cancel: { label: 'No, gracias', flat: true, color: 'grey' }
    }).onOk(() => {
      router.push(`/purchases/print/${purchaseId}`)
    }).onCancel(() => {
      router.push('/purchases')
    })
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al registrar la compra' })
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  const [providersRes, warehousesRes, productsRes] = await Promise.all([
    providerService.getAll(),
    warehouseService.getAll(),
    productService.getAll()
  ])
  providerOptions.value = providersRes.data.map(p => ({ label: p.name, value: p.id }))
  warehouseOptions.value = warehousesRes.data.map(w => ({ label: w.name, value: w.id }))
  allProducts = productsRes.data
})
</script>
