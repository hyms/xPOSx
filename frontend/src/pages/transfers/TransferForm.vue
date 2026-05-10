<template>
  <q-page padding>
    <q-card>
      <q-card-section>
        <div class="text-h6">Crear Transferencia</div>
      </q-card-section>

      <q-card-section class="q-pa-md">
        <q-form @submit="onSubmit" class="q-gutter-md">
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-3">
              <q-input v-model="formData.date" type="date" label="Fecha" lazy-rules :rules="[val => !!val || 'Requerido']" outlined dense />
            </div>
            <div class="col-12 col-md-4.5">
              <q-select
                v-model="formData.fromWarehouseId"
                :options="warehouses"
                label="Desde Almacén"
                option-value="id"
                option-label="name"
                emit-value
                map-options
                lazy-rules
                :rules="[val => !!val || 'Requerido']"
                outlined
                dense
              />
            </div>
            <div class="col-12 col-md-4.5">
              <q-select
                v-model="formData.toWarehouseId"
                :options="warehouses"
                label="Hacia Almacén"
                option-value="id"
                option-label="name"
                emit-value
                map-options
                lazy-rules
                :rules="[val => !!val || 'Requerido', val => val !== formData.fromWarehouseId || 'No puede ser el mismo almacén']"
                outlined
                dense
              />
            </div>

            <!-- Product Selection -->
            <div class="col-12">
              <q-select
                v-model="selectedProduct"
                :options="products"
                label="Buscar Producto"
                use-input
                @filter="filterProducts"
                option-label="name"
                @update:model-value="addProduct"
                outlined
                dense
              >
                <template v-slot:no-option>
                  <q-item><q-item-section class="text-grey">Sin resultados</q-item-section></q-item>
                </template>
              </q-select>
            </div>

            <!-- Items Table -->
            <div class="col-12">
              <q-table
                flat bordered
                :rows="formData.details"
                :columns="itemColumns"
                row-key="productId"
                hide-bottom
              >
                <template v-slot:body-cell-quantity="props">
                  <q-td :props="props">
                    <q-input
                      v-model.number="props.row.quantity"
                      type="number"
                      dense
                      @update:model-value="calculateTotal(props.row)"
                    />
                  </q-td>
                </template>
                <template v-slot:body-cell-actions="props">
                  <q-td :props="props">
                    <q-btn flat round color="negative" icon="delete" @click="removeItem(props.row)" />
                  </q-td>
                </template>
              </q-table>
            </div>

            <div class="col-12">
              <q-input v-model="formData.notes" label="Notas" type="textarea" autogrow outlined dense />
            </div>
          </div>

          <div class="row justify-between items-center q-mt-md">
            <div class="text-h6 text-primary">Total: ${{ grandTotal.toFixed(2) }}</div>
            <div class="q-gutter-sm">
              <q-btn label="Cancelar" color="primary" flat to="/transfers" />
              <q-btn label="Guardar Transferencia" color="primary" type="submit" :loading="saving" :disable="formData.details.length === 0" />
            </div>
          </div>
        </q-form>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { transferService } from '@/services/transfer.service';
import type { Transfer, TransferDetail } from '@/types'
import { warehouseService } from '@/services/warehouse.service';
import type { Warehouse } from '@/types'
import { productService } from '@/services/product.service';
import type { Product } from '@/types'

const $q = useQuasar()
const router = useRouter()
const warehouses = ref<Warehouse[]>([])
const products = ref<Product[]>([])
const allProducts = ref<Product[]>([])
const selectedProduct = ref(null)
const saving = ref(false)

const formData = reactive<Transfer>({
  date: new Date().toISOString().split('T')[0],
  fromWarehouseId: 0,
  toWarehouseId: 0,
  items: 0,
  grandTotal: 0,
  status: 'Completed',
  notes: '',
  details: []
})

const itemColumns = [
  { name: 'name', label: 'Producto', field: (row: any) => getProductName(row.productId), align: 'left' as const },
  { name: 'cost', label: 'Costo', field: 'cost', align: 'right' as const },
  { name: 'quantity', label: 'Cantidad', field: 'quantity', align: 'center' as const },
  { name: 'total', label: 'Subtotal', field: 'total', format: (val: number) => `$${val.toFixed(2)}`, align: 'right' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const getProductName = (id: number) => allProducts.value.find((p: Product) => p.id === id)?.name || 'Desconocido'

const grandTotal = computed(() => formData.details.reduce((sum: number, item: TransferDetail) => sum + item.total, 0))

const fetchData = async () => {
  try {
    const [wRes, pRes] = await Promise.all([
      warehouseService.getAll(),
      productService.getAll()
    ])
    warehouses.value = wRes.data
    allProducts.value = pRes.data
    products.value = pRes.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar catálogos' })
  }
}

const filterProducts = (val: string, update: any) => {
  update(() => {
    const needle = val.toLowerCase()
    products.value = allProducts.value.filter((v: Product) => v.name.toLowerCase().indexOf(needle) > -1)
  })
}

const addProduct = (product: Product) => {
  if (!product) return
  
  const existing = formData.details.find((d: TransferDetail) => d.productId === product.id)
  if (existing) {
    existing.quantity++
    calculateTotal(existing)
  } else {
    formData.details.push({
      productId: product.id!,
      cost: product.cost,
      quantity: 1,
      total: product.cost
    })
  }
  selectedProduct.value = null
}

const calculateTotal = (item: TransferDetail) => {
  item.total = item.cost * item.quantity
}

const removeItem = (item: TransferDetail) => {
  const index = formData.details.indexOf(item)
  if (index > -1) formData.details.splice(index, 1)
}

const onSubmit = async () => {
  saving.value = true
  try {
    formData.grandTotal = grandTotal.value
    formData.items = formData.details.length
    await transferService.create(formData)
    $q.notify({ color: 'positive', message: 'Transferencia guardada con éxito' })
    router.push('/transfers')
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar transferencia' })
  } finally {
    saving.value = false
  }
}

onMounted(fetchData)
</script>
