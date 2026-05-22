<template>
    <q-page padding>
        <div class="row q-col-gutter-sm">
            <div class="col-12 app-table-container">
                <q-table
                    title="Productos"
                    :rows="products"
                    :columns="columns"
                    row-key="id"
                    :loading="loading"
                    :filter="filter"
                >
                    <template #top-right>
                        <div class="row items-center no-wrap">
                            <BaseSearch
                                @search="filter = $event"
                                class="full-width-xs"
                            />
                            <q-btn
                                flat
                                round
                                color="primary"
                                icon="qr_code_scanner"
                                class="q-ml-sm"
                                @click="openScanner('search')"
                            >
                                <q-tooltip>Escanear código</q-tooltip>
                            </q-btn>
                        </div>
                        <BaseButton
                            label="Nuevo Producto"
                            icon="add"
                            @click="openDialog()"
                            class="q-ml-md full-width-xs mobile-only-mt"
                        />
                    </template>

                    <template v-slot:body-cell-image="props">
                        <q-td :props="props" class="text-center">
                            <q-avatar 
                                v-if="props.row.image"
                                size="48px" 
                                class="cursor-pointer hover-scale shadow-1 rounded-borders animate-scale"
                                @click="openPreview(props.row)"
                            >
                                <img :src="props.row.image" />
                                <q-tooltip>Click para ampliar</q-tooltip>
                            </q-avatar>
                            <q-avatar
                                v-else
                                size="48px"
                                class="bg-grey-2 rounded-borders"
                            >
                                <q-icon
                                    name="inventory_2"
                                    color="grey-5"
                                    size="24px"
                                />
                            </q-avatar>
                        </q-td>
                    </template>

                    <template v-slot:body-cell-actions="props">
                        <q-td :props="props">
                            <q-btn
                                flat
                                round
                                color="primary"
                                icon="edit"
                                @click="openDialog(props.row)"
                            />
                            <q-btn
                                flat
                                round
                                color="negative"
                                icon="delete"
                                @click="confirmDelete(props.row)"
                            />
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
            <div class="text-subtitle1 text-primary q-mb-xs">
                Información Básica
            </div>
            <div class="row q-col-gutter-sm q-mb-md">
                <div class="col-12 col-md-6">
                    <BaseInput
                        v-model="formData.name"
                        label="Nombre"
                        lazy-rules
                        :rules="[(val) => !!val || 'Requerido']"
                    />
                </div>
                <div class="col-12 col-md-6">
                    <BaseInput
                        v-model="formData.code"
                        label="Código"
                        lazy-rules
                        :rules="[(val) => !!val || 'Requerido']"
                    >
                        <template v-slot:append>
                            <q-icon
                                name="qr_code_scanner"
                                class="cursor-pointer"
                                @click="openScanner('form')"
                            >
                                <q-tooltip>Escanear código</q-tooltip>
                            </q-icon>
                        </template>
                    </BaseInput>
                </div>
                <div class="col-12 col-md-6">
                    <div class="row no-wrap items-center">
                        <q-select
                            ref="categorySelect"
                            v-slot:default
                            v-model="formData.categoryId"
                            :options="categories"
                            label="Categoría"
                            option-value="id"
                            option-label="name"
                            emit-value
                            map-options
                            lazy-rules
                            :rules="[(val) => !!val || 'Requerido']"
                            outlined
                            dense
                            class="col"
                        />
                        <q-btn
                            flat
                            round
                            dense
                            color="primary"
                            icon="add"
                            class="q-ml-xs q-mb-md"
                            @click="openQuickAdd('category')"
                        >
                            <q-tooltip>Nueva Categoría</q-tooltip>
                        </q-btn>
                    </div>
                </div>
                <div class="col-12 col-md-6">
                    <div class="row no-wrap items-center">
                        <q-select
                            ref="unitSelect"
                            v-model="formData.unitId"
                            :options="units"
                            label="Unidad"
                            option-value="id"
                            option-label="name"
                            emit-value
                            map-options
                            outlined
                            dense
                            class="col"
                        />
                        <q-btn
                            flat
                            round
                            dense
                            color="primary"
                            icon="add"
                            class="q-ml-xs"
                            @click="openQuickAdd('unit')"
                        >
                            <q-tooltip>Nueva Unidad</q-tooltip>
                        </q-btn>
                    </div>
                </div>
            </div>

            <q-separator />

            <!-- Sección 2: Finanzas e Inventario -->
            <div class="text-subtitle1 text-primary q-mt-md q-mb-xs">
                Finanzas e Inventario
            </div>
            <div class="row q-col-gutter-sm q-mb-md">
                <div class="col-12 col-md-4">
                    <BaseInput
                        v-model.number="formData.cost"
                        label="Costo"
                        type="number"
                        step="0.01"
                        prefix="$"
                    />
                </div>
                <div class="col-12 col-md-4">
                    <BaseInput
                        v-model.number="formData.price"
                        label="Precio"
                        type="number"
                        step="0.01"
                        prefix="$"
                    />
                </div>
                <div class="col-12 col-md-4">
                    <BaseInput
                        v-model.number="formData.stockAlert"
                        label="Alerta Stock"
                        type="number"
                    />
                </div>
            </div>

            <q-separator />

            <!-- Sección 3: Opciones y Notas -->
            <div class="row q-col-gutter-sm">
                <div class="col-12 col-md-8">
                    <BaseInput
                        v-model="formData.note"
                        label="Notas"
                        type="textarea"
                        autogrow
                    />
                    <div class="q-mt-sm row q-gutter-md">
                        <q-checkbox
                            v-model="formData.isActive"
                            label="Activo"
                        />
                        <q-checkbox
                            v-model="formData.notSelling"
                            label="No disponible para venta"
                        />
                    </div>
                </div>

                <div class="col-12 col-md-4">
                    <div class="row items-center justify-between q-mb-sm">
                        <div class="text-subtitle2">Imagen referencial</div>
                        <q-btn
                            flat
                            round
                            dense
                            color="primary"
                            icon="photo_camera"
                            @click="showCamera = true"
                        >
                            <q-tooltip>Usar cámara</q-tooltip>
                        </q-btn>
                    </div>
                    <q-card bordered flat class="image-upload-card">
                        <div v-if="formData.image" class="image-preview">
                            <img :src="formData.image" alt="Product image" />
                            <q-btn
                                round
                                size="sm"
                                color="negative"
                                icon="close"
                                class="remove-btn"
                                @click="removeImage"
                            />
                        </div>
                        <div
                            v-else
                            class="upload-placeholder"
                            @click="triggerFileInput"
                        >
                            <q-icon
                                name="add_photo_alternate"
                                size="48px"
                                color="grey-6"
                            />
                            <div class="text-grey-6 text-center">
                                Haga clic para agregar
                            </div>
                        </div>
                        <input
                            ref="fileInput"
                            type="file"
                            accept="image/*"
                            style="display: none"
                            @change="handleFileChange"
                        />
                    </q-card>
                </div>
            </div>
        </FormDialog>

        <!-- Modal de Previsualización Glassmorphism -->
        <q-dialog v-model="previewModal" backdrop-filter="blur(8px)">
            <q-card class="glass-preview-card no-shadow">
                <q-card-section class="row items-center q-pb-none">
                    <div class="text-h6 text-white text-shadow text-bold">{{ selectedProduct?.name }}</div>
                    <q-space />
                    <q-btn icon="close" flat round dense v-close-popup color="white" />
                </q-card-section>

                <q-card-section class="q-pa-md">
                    <ProgressiveImage 
                        v-if="selectedProduct && selectedProduct.image"
                        :src="selectedProduct.image" 
                        :alt="selectedProduct.name"
                        ratio="1/1"
                    />
                </q-card-section>

                <q-card-section class="bg-black-transparent text-white text-center q-py-sm">
                    <div class="text-subtitle2 text-grey-3">Código: {{ selectedProduct?.code }}</div>
                    <div class="text-primary text-bold text-h6 q-mt-xs">
                        {{ formatCurrency(selectedProduct?.price || 0) }}
                    </div>
                </q-card-section>
            </q-card>
        </q-dialog>

        <!-- Scanner Dialog -->
        <BarcodeScannerDialog
            v-model="showScanner"
            @detect="handleScan"
            :title="scannerTarget === 'search' ? 'Escanear para buscar' : 'Escanear código de producto'"
        />

        <!-- Camera Dialog -->
        <CameraDialog
            v-model="showCamera"
            @captured="handleCapture"
        />

        <!-- Quick Add Dialogs -->
        <q-dialog v-model="quickAdd.show" persistent backdrop-filter="blur(4px)">
            <q-card style="width: 350px; border-radius: 15px" class="glass-dialog">
                <q-card-section class="bg-primary text-white row items-center q-pb-none">
                    <div class="text-h6">Nueva {{ quickAdd.type === 'category' ? 'Categoría' : 'Unidad' }}</div>
                    <q-space />
                    <q-btn icon="close" flat round dense v-close-popup />
                </q-card-section>

                <q-card-section class="q-pt-md q-gutter-y-sm">
                    <BaseInput
                        v-model="quickAdd.form.name"
                        :label="quickAdd.type === 'category' ? 'Nombre de Categoría' : 'Nombre de Unidad'"
                        autofocus
                        @keyup.enter="saveQuickAdd"
                        :rules="[val => !!val || 'Requerido']"
                    />
                    <BaseInput
                        v-if="quickAdd.type === 'unit'"
                        v-model="quickAdd.form.shortName"
                        label="Nombre Corto"
                        :rules="[val => !!val || 'Requerido']"
                    />
                    <BaseInput
                        v-if="quickAdd.type === 'category'"
                        v-model="quickAdd.form.code"
                        label="Código (Opcional)"
                    />
                </q-card-section>

                <q-card-actions align="right" class="q-pa-md">
                    <q-btn flat label="Cancelar" color="grey" v-close-popup />
                    <q-btn
                        unelevated
                        label="Guardar"
                        color="primary"
                        :loading="quickAdd.saving"
                        @click="saveQuickAdd"
                        :disable="!quickAdd.form.name || (quickAdd.type === 'unit' && !quickAdd.form.shortName)"
                    />
                </q-card-actions>
            </q-card>
        </q-dialog>
    </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, nextTick } from "vue";
import { useQuasar } from "quasar";
import { productService } from "@/services/product.service";
import { categoryService } from "@/services/category.service";
import { unitService } from "@/services/unit.service";
import type { Product, Category, Unit } from "@/types";
import { useTable } from "@/composables/useTable";
import { useCurrency } from "@/composables/useCurrency";
import { useBarcodeScanner } from "@/composables/useBarcodeScanner";
import BaseSearch from "@/components/base/BaseSearch.vue";
import BaseTable from "@/components/base/BaseTable.vue";
import BaseButton from "@/components/base/BaseButton.vue";
import BaseInput from "@/components/base/BaseInput.vue";
import FormDialog from "@/components/FormDialog.vue";
import BarcodeScannerDialog from "@/components/BarcodeScannerDialog.vue";
import CameraDialog from "@/components/CameraDialog.vue";
import ProgressiveImage from "@/components/ProgressiveImage.vue";
import { compressImage } from "@/utils/image-optimizer";

const $q = useQuasar();
const { formatCurrency } = useCurrency();
const {
    data: products,
    loading,
    pagination,
    fetchItems,
} = useTable<Product>("/products");
const categories = ref<Category[]>([]);
const units = ref<Unit[]>([]);
const saving = ref(false);
const showDialog = ref(false);
const showScanner = ref(false);
const showCamera = ref(false);
const scannerTarget = ref<'search' | 'form'>('search');
const isEdit = ref(false);
const fileInput = ref<HTMLInputElement | null>(null);
const categorySelect = ref<any>(null);
const unitSelect = ref<any>(null);
const filter = ref("");

const previewModal = ref(false);
const selectedProduct = ref<any>(null);

const openPreview = (product: any) => {
    selectedProduct.value = product;
    previewModal.value = true;
};

// Quick Add State
const quickAdd = reactive({
    show: false,
    type: 'category' as 'category' | 'unit',
    saving: false,
    form: {
        name: '',
        shortName: '',
        code: ''
    }
});

const openQuickAdd = (type: 'category' | 'unit') => {
    quickAdd.type = type;
    quickAdd.form = { name: '', shortName: '', code: '' };
    quickAdd.show = true;
};

const saveQuickAdd = async () => {
    quickAdd.saving = true;
    try {
        if (quickAdd.type === 'category') {
            const res = await categoryService.create({ 
                name: quickAdd.form.name, 
                code: quickAdd.form.code || `CAT-${Date.now()}` 
            } as any);
            categories.value.push(res.data);
            formData.categoryId = res.data.id;
        } else {
            const res = await unitService.create({ 
                name: quickAdd.form.name, 
                shortName: quickAdd.form.shortName 
            } as any);
            units.value.push(res.data);
            formData.unitId = res.data.id;
        }
        quickAdd.show = false;
        $q.notify({ color: 'positive', message: 'Registro exitoso' });
        
        // Devolver el foco al formulario (QSelect) correspondientemente
        nextTick(() => {
            if (quickAdd.type === 'category') {
                categorySelect.value?.focus();
            } else {
                unitSelect.value?.focus();
            }
        });
    } catch (error) {
        $q.notify({ color: 'negative', message: 'Error al registrar' });
    } finally {
        quickAdd.saving = false;
    }
};

// Physical Scanner for Product List
useBarcodeScanner({
    onScan: (code) => {
        if (showDialog.value) {
            // Si el diálogo está abierto, registramos el código en el formulario
            formData.code = code;
            $q.notify({
                message: "Código capturado",
                color: "positive",
                icon: "qr_code",
                timeout: 800
            });
        } else {
            // Si no, buscamos en la tabla
            filter.value = code;
        }
    }
});

const openScanner = (target: 'search' | 'form') => {
    scannerTarget.value = target;
    showScanner.value = true;
};

const handleScan = (code: string) => {
    if (scannerTarget.value === 'search') {
        filter.value = code;
    } else {
        formData.code = code;
    }
};

const handleCapture = (dataUrl: string) => {
    formData.image = dataUrl;
};

const formData = reactive<Product & { image?: string }>({
    name: "",
    code: "",
    cost: 0,
    price: 0,
    categoryId: undefined,
    unitId: undefined,
    stockAlert: 0,
    note: "",
    isActive: true,
    notSelling: false,
    image: "",
});

const columns = [
    {
        name: "image",
        label: "Imagen",
        field: "image",
        align: "center" as const,
    },
    {
        name: "code",
        label: "Código",
        field: "code",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "name",
        label: "Nombre",
        field: "name",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "category",
        label: "Categoría",
        field: (row: any) => row.category?.name || "N/A",
        align: "left" as const,
    },
    {
        name: "stock",
        label: "Stock",
        field: "stock",
        sortable: true,
        align: "center" as const,
    },
    {
        name: "cost",
        label: "Costo",
        field: "cost",
        format: (val: number) => `$${val.toFixed(2)}`,
        align: "right" as const,
    },
    {
        name: "price",
        label: "Precio",
        field: "price",
        format: (val: number) => `$${val.toFixed(2)}`,
        align: "right" as const,
    },
    {
        name: "actions",
        label: "Acciones",
        field: "actions",
        align: "center" as const,
    },
];

const fetchData = async () => {
    loading.value = true;
    try {
        const [cRes, uRes] = await Promise.all([
            categoryService.getAll(),
            unitService.getAll(),
        ]);
        categories.value = cRes.data;
        units.value = uRes.data;
        await fetchItems();
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al cargar datos" });
    } finally {
        loading.value = false;
    }
};

const openDialog = (product?: any) => {
    if (product) {
        isEdit.value = true;
        // Map data ensuring IDs are correctly assigned even if they come from nested objects
        Object.assign(formData, { 
            ...product,
            categoryId: product.categoryId || product.category_id || product.category?.id,
            unitId: product.unitId || product.unit_id || product.unit?.id
        });
    } else {
        isEdit.value = false;
        Object.assign(formData, {
            name: "",
            code: "",
            cost: 0,
            price: 0,
            categoryId: undefined,
            unitId: undefined,
            stockAlert: 0,
            note: "",
            isActive: true,
            notSelling: false,
            image: "",
        });
    }
    showDialog.value = true;
};

const triggerFileInput = () => {
    fileInput.value?.click();
};

const handleFileChange = async (event: Event) => {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];
    if (file) {
        try {
            $q.loading.show({ message: 'Optimizando imagen...' });
            // Comprimir la imagen a un tamaño máximo de 500x500px y calidad de 85%
            const optimizedFile = await compressImage(file, 500, 500, 0.85);
            
            const reader = new FileReader();
            reader.onload = (e) => {
                formData.image = e.target?.result as string;
            };
            reader.readAsDataURL(optimizedFile);
        } catch (error) {
            $q.notify({
                color: "negative",
                message: "Error al optimizar la imagen",
            });
        } finally {
            $q.loading.hide();
        }
    }
};

const removeImage = () => {
    formData.image = "";
};

const saveProduct = async () => {
    saving.value = true;
    try {
        if (isEdit.value) {
            await productService.update(formData.id!, formData);
            $q.notify({ color: "positive", message: "Producto actualizado" });
        } else {
            await productService.create(formData);
            $q.notify({ color: "positive", message: "Producto creado" });
        }
        showDialog.value = false;
        fetchData();
    } catch (error) {
        $q.notify({ color: "negative", message: "Error al guardar producto" });
    } finally {
        saving.value = false;
    }
};

const confirmDelete = (product: Product) => {
    $q.dialog({
        title: "Confirmar eliminación",
        message: `¿Eliminar el producto ${product.name}?`,
        cancel: true,
        persistent: true,
    }).onOk(async () => {
        try {
            await productService.delete(product.id!);
            $q.notify({ color: "positive", message: "Producto eliminado" });
            fetchData();
        } catch (error) {
            $q.notify({
                color: "negative",
                message: "Error al eliminar producto",
            });
        }
    });
};

onMounted(fetchData);
</script>

<style scoped>
.product-image-container {
    width: 80px;
    height: 80px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 8px; /* Consistent border-radius */
    overflow: hidden;
    border: 1px solid var(--color-border);
    background-color: var(--color-background-elevated);

    .body--dark & {
        border-color: var(--color-border-dark);
        background-color: var(--color-background-elevated-dark);
    }
}
@media (min-width: 768px) {
    .product-image-container {
        width: 100px; /* Slightly larger for tablets */
        height: 100px;
    }
}
@media (min-width: 1024px) {
    .product-image-container {
        width: 120px; /* Further larger for desktops */
        height: 120px;
    }
}
.product-image {
    max-width: 100%;
    max-height: 100%;
    object-fit: contain;
    border-radius: 4px; /* Slight inner border-radius */
}

/* Styling for the q-icon placeholder when no image */
.q-table .q-icon[name="inventory_2"] {
    color: var(--color-text-primary) !important;
    opacity: 0.4;
    font-size: 48px !important; /* Ensure consistent size */
}

/* Image Upload Card */
.image-upload-card {
    min-height: 200px;
    cursor: pointer;
    border-radius: 12px; /* Consistent with q-card */
    border: 1px solid var(--color-border);
    box-shadow: none !important; /* Remove default Quasar card shadow, manage with global.scss */
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    background-color: var(--color-background-elevated);

    &:hover {
        border-color: var(--color-primary); /* Highlight on hover */
        box-shadow: 0 0 0 2px rgba(var(--color-primary-rgb), 0.1);
    }

    .body--dark & {
        border-color: var(--color-border-dark);
        background-color: var(--color-background-elevated-dark);
        &:hover {
            border-color: var(--color-primary);
            box-shadow: 0 0 0 2px rgba(var(--color-primary-rgb), 0.2);
        }
    }
}

.upload-placeholder {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 200px;
    padding: 20px;
    text-align: center;
    font-family: var(--font-family-body);
    color: var(--color-text-primary);
    opacity: 0.7; /* Subdued text */

    .q-icon {
        font-size: 64px !important; /* Larger icon */
        color: var(--color-text-primary) !important;
        opacity: 0.5;
        margin-bottom: 8px;
    }
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
    border-radius: 8px; /* Consistent border-radius for preview image */
}
.remove-btn {
    position: absolute;
    top: 10px;
    right: 10px;
    background-color: rgba(
        var(--color-text-dark-rgb),
        0.8
    ); /* Semi-transparent white background */
    color: var(
        --color-negative
    ) !important; /* Use negative color for close icon */
    z-index: 10;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);

    &:hover {
        background-color: var(--color-negative);
        color: white !important;
    }
}
.glass-dialog {
    background: rgba(var(--color-background-elevated-rgb), 0.8);
    backdrop-filter: blur(15px);
    border: 1px solid rgba(255, 255, 255, 0.1);
}

.glass-preview-card {
    background: rgba(0, 0, 0, 0.4) !important;
    backdrop-filter: blur(15px);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-radius: 20px;
    width: 100%;
    max-width: 500px;
}

.bg-black-transparent {
    background: rgba(0, 0, 0, 0.6);
    border-bottom-left-radius: 20px;
    border-bottom-right-radius: 20px;
}

.text-shadow {
    text-shadow: 0 2px 4px rgba(0, 0, 0, 0.5);
}

.hover-scale {
    transition: transform 0.2s ease-in-out;
    &:hover {
        transform: scale(1.1);
    }
}
</style>
