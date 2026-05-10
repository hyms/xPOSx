<template>
  <q-page padding>
    <q-card>
      <q-card-section>
        <div class="text-h6">Nuevo Ajuste de Inventario</div>
      </q-card-section>

      <q-card-section class="q-pa-md">
        <q-form @submit="onSubmit" class="q-gutter-md">
          <div class="row q-col-gutter-md">
            <div class="col-12 col-md-4">
              <q-input v-model="formData.date" type="date" label="Fecha" lazy-rules :rules="[val => !!val || 'Requerido']" outlined dense />
            </div>
            <div class="col-12 col-md-8">
              <q-select
                v-model="formData.warehouseId"
                :options="warehouses"
                label="Almacén"
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
                <template v-slot:body-cell-type="props">
                  <q-td :props="props">
                    <q-select
                      v-model="props.row.type"
                      :options="[
                        { label: 'Suma (+)', value: 'add' },
                        { label: 'Resta (-)', value: 'sub' }
                      ]"
                      dense
                      outlined
                      emit-value
                      map-options
                    />
                  </q-td>
                </template>
                <template v-slot:body-cell-quantity="props">
                  <q-td :props="props">
                    <q-input
                      v-model.number="props.row.quantity"
                      type="number"
                      dense
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

          <div class="row justify-end q-gutter-sm q-mt-md">
            <q-btn label="Cancelar" color="primary" flat to="/adjustments" />
            <q-btn label="Guardar Ajuste" color="primary" type="submit" :loading="saving" :disable="formData.details.length === 0" />
          </div>
        </q-form>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { adjustmentService } from '@/services/adjustment.service';
import type { Adjustment, AdjustmentDetail } from '@/types'
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

const formData = reactive<Adjustment>({
  date: new Date().toISOString().split('T')[0],
  warehouseId: 0,
  items: 0,
  notes: '',
  details: []
})

const itemColumns = [
  { name: 'name', label: 'Producto', field: (row: any) => getProductName(row.productId), align: 'left' as const },
  { name: 'type', label: 'Tipo', field: 'type', align: 'center' as const },
  { name: 'quantity', label: 'Cantidad', field: 'quantity', align: 'center' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const getProductName = (id: number) => allProducts.value.find((p: Product) => p.id === id)?.name || 'Desconocido'

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
  
  const existing = formData.details.find((d: AdjustmentDetail) => d.productId === product.id)
  if (existing) {
    existing.quantity++
  } else {
    formData.details.push({
      productId: product.id!,
      quantity: 1,
      type: 'add'
    })
  }
  selectedProduct.value = null
}

const removeItem = (item: AdjustmentDetail) => {
  const index = formData.details.indexOf(item)
  if (index > -1) formData.details.splice(index, 1)
}

const onSubmit = async () => {
  saving.value = true
  try {
    formData.items = formData.details.length
    await adjustmentService.create(formData)
    $q.notify({ color: 'positive', message: 'Ajuste de inventario guardado con éxito' })
    router.push('/adjustments')
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar el ajuste' })
  } finally {
    saving.value = false
  }
}

onMounted(fetchData)
</script>
