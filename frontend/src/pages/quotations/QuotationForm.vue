<template>
  <q-page padding>
    <q-card>
      <q-card-section>
        <div class="text-h6">{{ isEdit ? 'Ver Cotización' : 'Crear Nueva Cotización' }}</div>
      </q-card-section>

      <q-card-section class="q-pa-md">
        <q-form @submit="submitForm" class="q-gutter-md">
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-4">
              <q-input v-model="formData.date" label="Fecha" type="date" stack-label outlined dense :rules="[val => !!val || 'Requerido']" :readonly="isEdit" />
            </div>
            <div class="col-12 col-md-4">
              <q-select
                v-model="formData.clientId"
                :options="clientOptions"
                label="Cliente"
                emit-value
                map-options
                outlined
                dense
                :rules="[val => !!val || 'Requerido']"
                :readonly="isEdit"
              />
            </div>
            <div class="col-12 col-md-4">
              <q-select
                v-model="formData.warehouseId"
                :options="warehouseOptions"
                label="Almacén"
                emit-value
                map-options
                outlined
                dense
                :rules="[val => !!val || 'Requerido']"
                :readonly="isEdit"
              />
            </div>
          </div>

          <!-- Product Search -->
          <div class="row q-col-gutter-md items-center">
            <div class="col-12">
              <q-select
                v-model="searchProduct"
                use-input
                input-debounce="300"
                label="Buscar Producto"
                :options="productOptions"
                @filter="filterProducts"
                @update:model-value="addProduct"
                outlined
                dense
                :readonly="isEdit"
              >
                <template v-slot:no-option>
                  <q-item><q-item-section class="text-grey">No se encontraron productos</q-item-section></template>
                </template>
              </q-select>
            </div>
          </div>

          <!-- Details Table -->
          <q-table
            flat bordered
            :rows="formData.details"
            :columns="columns"
            row-key="productId"
            hide-pagination
          >
            <template v-slot:body-cell-quantity="props">
              <q-td :props="props">
                <q-input v-model.number="props.row.quantity" type="number" dense @update:model-value="updateRow(props.row)" :readonly="isEdit" />
              </q-td>
            </template>
            <template v-slot:body-cell-actions="props">
              <q-td :props="props">
                <q-btn flat round color="negative" icon="delete" @click="removeProduct(props.row)" :disable="isEdit" />
              </q-td>
            </template>
          </q-table>

          <div class="row justify-end q-mt-md">
            <div class="col-12 col-md-4">
              <q-field label="Total Cotización" stack-label borderless class="text-h6">
                {{ formatCurrency(formData.grandTotal) }}
              </q-field>
            </div>
          </div>

          <div class="row justify-end q-gutter-sm">
            <q-btn label="Cancelar" color="primary" flat to="/quotations" />
            <q-btn label="Guardar Cotización" color="primary" type="submit" :loading="saving" :disable="isEdit" />
          </div>
        </q-form>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { quotationService } from '@/services/quotation.service'
import { clientService } from '@/services/client.service'
import { warehouseService } from '@/services/warehouse.service'
import { productService } from '@/services/product.service'
import type { Quotation, Product, QuotationDetail } from '@/types'

import { useCurrency } from '@/composables/useCurrency';

const router = useRouter()
const $q = useQuasar()
const saving = ref(false)
const { formatCurrency } = useCurrency();


const clientOptions = ref<any[]>([])
const warehouseOptions = ref<any[]>([])
const productOptions = ref<any[]>([])
const searchProduct = ref(null)
const allProducts = ref<Product[]>([])
const isEdit = ref(false)

const formData = reactive<Quotation>({
  date: new Date().toISOString().substr(0, 10),
  clientId: 0,
  warehouseId: 0,
  grandTotal: 0,
  status: 'pending',
  details: []
})

const columns = [
  { name: 'name', label: 'Producto', field: 'name', align: 'left' as const },
  { name: 'price', label: 'Precio', field: 'price', align: 'right' as const },
  { name: 'quantity', label: 'Cantidad', field: 'quantity', align: 'center' as const },
  { name: 'total', label: 'Subtotal', field: 'total', align: 'right' as const },
  { name: 'actions', label: '', field: 'actions' }
]

const filterProducts = (val: string, update: any) => {
  update(() => {
    const needle = val.toLowerCase()
    productOptions.value = allProducts.value
      .filter((v: Product) => v.name.toLowerCase().indexOf(needle) > -1 || v.code.toLowerCase().indexOf(needle) > -1)
      .map((p: Product) => ({ label: `${p.code} - ${p.name}`, value: p }))
  })
}

const updateRow = (row: any) => {
  row.total = row.price * row.quantity
  calculateTotal()
}

const addProduct = (val: any) => {
  if (!val) return
  const product = val.value
  const existing = formData.details.find((d: QuotationDetail) => d.productId === product.id)
  if (existing) {
    existing.quantity++
    updateRow(existing)
  } else {
    formData.details.push({
      productId: product.id,
      name: product.name,
      price: product.price,
      quantity: 1,
      total: product.price
    } as any)
  }
  searchProduct.value = null
  calculateTotal()
}

const removeProduct = (row: any) => {
  formData.details = formData.details.filter((d: QuotationDetail) => d.productId !== row.productId)
  calculateTotal()
}

const calculateTotal = () => {
  formData.grandTotal = formData.details.reduce((acc: number, curr: QuotationDetail) => acc + curr.total, 0)
}

const submitForm = async () => {
  if (formData.details.length === 0) {
    $q.notify({ color: 'warning', message: 'Debe añadir al menos un producto' })
    return
  }
  saving.value = true
  try {
    const res = await quotationService.create(formData)
    const quotationId = res.data?.id || 1

    $q.dialog({
      title: 'Cotización Exitosa',
      message: `La cotización se ha registrado correctamente. ¿Desea imprimir el comprobante?`,
      persistent: true,
      ok: { label: 'Imprimir', color: 'primary', unelevated: true, icon: 'print' },
      cancel: { label: 'No, gracias', flat: true, color: 'grey' }
    }).onOk(() => {
      router.push(`/quotations/print/${quotationId}`)
    }).onCancel(() => {
      router.push('/quotations')
    })
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al crear cotización' })
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  const [clients, warehouses, products] = await Promise.all([
    clientService.getAll(),
    warehouseService.getAll(),
    productService.getAll()
  ])
  clientOptions.value = clients.data.map(c => ({ label: c.name, value: c.id }))
  warehouseOptions.value = warehouses.data.map(w => ({ label: w.name, value: w.id }))
  allProducts.value = products.data

  // Check if we are in edit mode
  const quotationId = Number(router.currentRoute.value.params.id)
  if (quotationId) {
    isEdit.value = true
    try {
      const res = await quotationService.getById(quotationId)
      const quotationData = res.data

      if (quotationData) {
        // Populate formData
        Object.assign(formData, { 
            ...quotationData, 
            details: quotationData.details || [] 
        })
      }
    } catch (error) {
      $q.notify({ color: 'negative', message: 'Error al cargar la cotización para edición' })
      router.push('/quotations')
    }
  }
})
</script>
