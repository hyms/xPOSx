<template>
  <q-page padding>
    <div class="row q-col-gutter-md">
      <div class="col-12">
        <q-table
          title="Productos"
          :rows="products"
          :columns="columns"
          row-key="id"
          :loading="loading"
          :filter="filter"
        >
          <template v-slot:top-right>
            <q-input v-model="filter" debounce="300" placeholder="Buscar..." dense borderless>
              <template v-slot:prepend>
                <q-icon name="search" />
              </template>
            </q-input>
            <q-btn color="primary" label="Nuevo Producto" icon="add" @click="openDialog()" class="q-ml-md" />
          </template>

          <template v-slot:body-cell-image="props">
            <q-td :props="props">
              <div v-if="props.row.image" class="product-image-container">
                <img :src="props.row.image" class="product-image" />
              </div>
              <q-icon v-else name="inventory_2" color="grey-5" size="40px" />
            </q-td>
          </template>

          <template v-slot:body-cell-actions="props">
            <q-td :props="props">
              <q-btn flat round color="primary" icon="edit" @click="openDialog(props.row)" />
              <q-btn flat round color="negative" icon="delete" @click="confirmDelete(props.row)" />
            </q-td>
          </template>
        </q-table>
      </div>
    </div>

    <!-- Product Dialog -->
    <FormDialog
      v-model="showDialog"
      :title="isEdit ? 'Editar Producto' : 'Nuevo Producto'"
      @submit="saveProduct"
      :saving="saving"
      full-width
    >
      <!-- Sección 1: Información Básica -->
      <div class="text-subtitle1 text-primary q-mb-xs">Información Básica</div>
      <div class="row q-col-gutter-md q-mb-md">
        <div class="col-12 col-md-6">
          <q-input v-model="formData.name" label="Nombre" lazy-rules :rules="[ val => !!val || 'Requerido']" outlined dense />
        </div>
        <div class="col-12 col-md-6">
          <q-input v-model="formData.code" label="Código" lazy-rules :rules="[ val => !!val || 'Requerido']" outlined dense />
        </div>
        <div class="col-12 col-md-6">
          <q-select
            v-model="formData.categoryId"
            :options="categories"
            label="Categoría"
            option-value="id"
            option-label="name"
            emit-value
            map-options
            lazy-rules
            :rules="[ val => !!val || 'Requerido']"
            outlined
            dense
          />
        </div>
        <div class="col-12 col-md-6">
          <q-select
            v-model="formData.unitId"
            :options="units"
            label="Unidad"
            option-value="id"
            option-label="name"
            emit-value
            map-options
            outlined
            dense
          />
        </div>
      </div>

      <q-separator />

      <!-- Sección 2: Finanzas e Inventario -->
      <div class="text-subtitle1 text-primary q-mt-md q-mb-xs">Finanzas e Inventario</div>
      <div class="row q-col-gutter-md q-mb-md">
        <div class="col-12 col-md-4">
          <q-input v-model.number="formData.cost" label="Costo" type="number" step="0.01" prefix="$" outlined dense />
        </div>
        <div class="col-12 col-md-4">
          <q-input v-model.number="formData.price" label="Precio" type="number" step="0.01" prefix="$" outlined dense />
        </div>
        <div class="col-12 col-md-4">
          <q-input v-model.number="formData.stockAlert" label="Alerta Stock" type="number" outlined dense />
        </div>
      </div>

      <q-separator />

      <!-- Sección 3: Opciones y Notas -->
      <div class="row q-col-gutter-md">
        <div class="col-12 col-md-8">
          <q-input v-model="formData.note" label="Notas" type="textarea" autogrow outlined dense />
          <div class="q-mt-sm row q-gutter-md">
            <q-checkbox v-model="formData.isActive" label="Activo" />
            <q-checkbox v-model="formData.notSelling" label="No disponible para venta" />
          </div>
        </div>
        
        <div class="col-12 col-md-4">
          <div class="text-subtitle2 q-mb-sm">Imagen referencial</div>
          <q-card bordered flat class="image-upload-card">
            <div v-if="formData.image" class="image-preview">
              <img :src="formData.image" alt="Product image" />
              <q-btn round size="sm" color="negative" icon="close" class="remove-btn" @click="removeImage" />
            </div>
            <div v-else class="upload-placeholder" @click="triggerFileInput">
              <q-icon name="add_photo_alternate" size="48px" color="grey-6" />
              <div class="text-grey-6 text-center">Haga clic para agregar</div>
            </div>
            <input ref="fileInput" type="file" accept="image/*" style="display: none" @change="handleFileChange" />
          </q-card>
        </div>
      </div>
    </FormDialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { useQuasar } from 'quasar'
import { productService } from '@/services/product.service';
import type { Product } from '@/types'
import { categoryService } from '@/services/category.service';
import type { Category } from '@/types'
import { unitService } from '@/services/unit.service';
import type { Unit } from '@/types'
import FormDialog from '@/components/FormDialog.vue'

const $q = useQuasar()
const products = ref<Product[]>([])
const categories = ref<Category[]>([])
const units = ref<Unit[]>([])
const loading = ref(true)
const saving = ref(false)
const showDialog = ref(false)
const isEdit = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)
const filter = ref('')

const formData = reactive<Product & { image?: string }>({
  name: '',
  code: '',
  cost: 0,
  price: 0,
  categoryId: undefined,
  unitId: undefined,
  stockAlert: 0,
  note: '',
  isActive: true,
  notSelling: false,
  image: ''
})

const columns = [
  { name: 'image', label: 'Imagen', field: 'image', align: 'center' as const },
  { name: 'code', label: 'Código', field: 'code', sortable: true, align: 'left' as const },
  { name: 'name', label: 'Nombre', field: 'name', sortable: true, align: 'left' as const },
  { name: 'category', label: 'Categoría', field: (row: any) => row.category?.name || 'N/A', align: 'left' as const },
  { name: 'stock', label: 'Stock', field: 'stock', sortable: true, align: 'center' as const },
  { name: 'cost', label: 'Costo', field: 'cost', format: (val: number) => `$${val.toFixed(2)}`, align: 'right' as const },
  { name: 'price', label: 'Precio', field: 'price', format: (val: number) => `$${val.toFixed(2)}`, align: 'right' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const fetchData = async () => {
  loading.value = true
  try {
    const [pRes, cRes, uRes] = await Promise.all([
      productService.getAll(),
      categoryService.getAll(),
      unitService.getAll()
    ])
    products.value = pRes.data
    categories.value = cRes.data
    units.value = uRes.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar datos' })
  } finally {
    loading.value = false
  }
}

const openDialog = (product?: Product) => {
  if (product) {
    isEdit.value = true
    Object.assign(formData, { ...product })
  } else {
    isEdit.value = false
    Object.assign(formData, { 
      name: '', code: '', cost: 0, price: 0, categoryId: undefined, 
      unitId: undefined, stockAlert: 0, note: '', isActive: true, notSelling: false, image: ''
    })
  }
  showDialog.value = true
}

const triggerFileInput = () => {
  fileInput.value?.click()
}

const handleFileChange = (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (file) {
    if (file.size > 2 * 1024 * 1024) {
      $q.notify({ color: 'negative', message: 'La imagen debe ser menor a 2MB' })
      return
    }
    const reader = new FileReader()
    reader.onload = (e) => {
      formData.image = e.target?.result as string
    }
    reader.readAsDataURL(file)
  }
}

const removeImage = () => {
  formData.image = ''
}

const saveProduct = async () => {
  saving.value = true
  try {
    if (isEdit.value) {
      await productService.update(formData.id!, formData)
      $q.notify({ color: 'positive', message: 'Producto actualizado' })
    } else {
      await productService.create(formData)
      $q.notify({ color: 'positive', message: 'Producto creado' })
    }
    showDialog.value = false
    fetchData()
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al guardar producto' })
  } finally {
    saving.value = false
  }
}

const confirmDelete = (product: Product) => {
  $q.dialog({
    title: 'Confirmar eliminación',
    message: `¿Eliminar el producto ${product.name}?`,
    cancel: true,
    persistent: true
  }).onOk(async () => {
    try {
      await productService.delete(product.id!)
      $q.notify({ color: 'positive', message: 'Producto eliminado' })
      fetchData()
    } catch (error) {
      $q.notify({ color: 'negative', message: 'Error al eliminar producto' })
    }
  })
}

onMounted(fetchData)
</script>

<style scoped>
.product-image-container {
  width: 80px;
  height: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
}
@media (min-width: 768px) {
  .product-image-container {
    width: 120px;
    height: 120px;
  }
}
@media (min-width: 1024px) {
  .product-image-container {
    width: 160px;
    height: 160px;
  }
}
.product-image {
  max-width: 100%;
  max-height: 100%;
  object-fit: contain;
  border-radius: 4px;
}
.image-upload-card {
  min-height: 200px;
  cursor: pointer;
}
.upload-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 200px;
  padding: 20px;
  text-align: center;
}
.image-preview {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 200px;
  padding: 10px;
}
.image-preview img {
  max-width: 100%;
  max-height: 200px;
  object-fit: contain;
}
.remove-btn {
  position: absolute;
  top: 10px;
  right: 10px;
}
</style>
