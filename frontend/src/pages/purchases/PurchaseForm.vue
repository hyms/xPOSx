<template>
    <q-page padding>
        <q-card>
            <q-card-section>
                <div class="text-h6">
                    {{ isEdit ? "Ver Compra" : "Registrar Compra" }}
                </div>
            </q-card-section>

            <q-form @submit.prevent="onSubmit">
                <q-card-section class="q-pa-md">
                    <div class="row q-col-gutter-sm">
                        <div class="col-12 col-md-4">
                            <q-input
                                v-model="formData.ref"
                                label="Referencia"
                                outlined
                                dense
                                readonly
                            />
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
                                :rules="[(val) => !!val || 'Requerido']"
                                :readonly="isEdit"
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
                                :rules="[(val) => !!val || 'Requerido']"
                                :readonly="isEdit"
                            />
                        </div>
                    </div>

                    <!-- Product Search -->
                    <div class="row items-center q-col-gutter-sm q-mb-md">
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
                                :readonly="isEdit"
                                class="col"
                            />
                        </div>
                        <div class="col-12 col-md-2">
                            <q-btn
                                color="primary"
                                label="Añadir"
                                icon="add"
                                @click="addProductToPurchase"
                                :disable="!selectedProduct || isEdit"
                                class="full-width"
                            />
                        </div>
                        <div class="col-12 flex justify-end q-mt-sm">
                            <q-btn
                                flat
                                round
                                dense
                                color="primary"
                                icon="add"
                                @click="openQuickAddProduct"
                                :disable="isEdit"
                            >
                                <q-tooltip>Nuevo Producto Rápido</q-tooltip>
                            </q-btn>
                        </div>
                    </div>
                    <!-- Purchase Details Table -->
                    <div class="app-table-container">
                        <q-table
                            :rows="formData.details"
                            :columns="detailsColumns"
                            row-key="productId"
                            flat
                            bordered
                            :grid="$q.screen.lt.md"
                        >
                            <template v-slot:body-cell-cost="props">
                                <q-td :props="props">
                                    <q-input
                                        v-model.number="props.row.cost"
                                        type="number"
                                        dense
                                        @update:model-value="updateTotals"
                                        :readonly="isEdit"
                                        style="
                                            max-width: 100px;
                                            margin-left: auto;
                                        "
                                    />
                                </q-td>
                            </template>
                            <template v-slot:body-cell-quantity="props">
                                <q-td :props="props">
                                    <q-input
                                        v-model.number="props.row.quantity"
                                        type="number"
                                        dense
                                        @update:model-value="updateTotals"
                                        :readonly="isEdit"
                                        style="
                                            max-width: 80px;
                                            margin-left: auto;
                                        "
                                    />
                                </q-td>
                            </template>
                            <template v-slot:body-cell-total="props">
                                <q-td :props="props">{{
                                    formatCurrency(
                                        props.row.quantity * props.row.cost,
                                    )
                                }}</q-td>
                            </template>
                            <template v-slot:body-cell-actions="props">
                                <q-td :props="props">
                                    <q-btn
                                        flat
                                        round
                                        color="negative"
                                        icon="delete"
                                        @click="removeProduct(props.row)"
                                        :disable="isEdit"
                                    />
                                </q-td>
                            </template>

                            <template v-slot:item="props">
                                <div class="q-pa-xs col-xs-12">
                                    <q-card flat bordered>
                                        <q-card-section>
                                            <div
                                                class="row items-center justify-between"
                                            >
                                                <div
                                                    class="text-subtitle2 text-weight-bold"
                                                >
                                                    {{ props.row.productName }}
                                                </div>
                                                <q-btn
                                                    flat
                                                    round
                                                    color="negative"
                                                    icon="delete"
                                                    size="sm"
                                                    @click="
                                                        removeProduct(props.row)
                                                    "
                                                    :disable="isEdit"
                                                />
                                            </div>
                                            <div
                                                class="row q-mt-sm items-center justify-between"
                                            >
                                                <div class="col-4">
                                                    <div
                                                        class="text-caption text-grey"
                                                    >
                                                        Costo
                                                    </div>
                                                    <q-input
                                                        v-model.number="
                                                            props.row.cost
                                                        "
                                                        type="number"
                                                        dense
                                                        borderless
                                                        @update:model-value="
                                                            updateTotals
                                                        "
                                                        :readonly="isEdit"
                                                        input-class="text-left"
                                                    />
                                                </div>
                                                <div class="col-4">
                                                    <div
                                                        class="text-caption text-grey"
                                                    >
                                                        Cantidad
                                                    </div>
                                                    <q-input
                                                        v-model.number="
                                                            props.row.quantity
                                                        "
                                                        type="number"
                                                        dense
                                                        borderless
                                                        @update:model-value="
                                                            updateTotals
                                                        "
                                                        :readonly="isEdit"
                                                        input-class="text-center"
                                                    />
                                                </div>
                                                <div class="col-4 text-right">
                                                    <div
                                                        class="text-caption text-grey"
                                                    >
                                                        Total
                                                    </div>
                                                    <div
                                                        class="text-weight-bold text-primary"
                                                    >
                                                        {{
                                                            formatCurrency(
                                                                props.row
                                                                    .quantity *
                                                                    props.row
                                                                        .cost,
                                                            )
                                                        }}
                                                    </div>
                                                </div>
                                            </div>
                                        </q-card-section>
                                    </q-card>
                                </div>
                            </template>
                        </q-table>
                    </div>

                    <!-- Totals -->
                    <div class="row justify-end q-mt-md">
                        <div class="col-12 col-md-4">
                            <q-list bordered>
                                <q-item>
                                    <q-item-section>Subtotal:</q-item-section>
                                    <q-item-section side>{{
                                        formatCurrency(subTotal)
                                    }}</q-item-section>
                                </q-item>
                                <q-item>
                                    <q-item-section>Total:</q-item-section>
                                    <q-item-section side class="text-h6">{{
                                        formatCurrency(formData.grandTotal)
                                    }}</q-item-section>
                                </q-item>
                                <q-item>
                                    <q-item-section>Total:</q-item-section>
                                    <q-item-section side class="text-h6">{{
                                        formatCurrency(formData.grandTotal)
                                    }}</q-item-section>
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
                        <div class="row q-col-gutter-sm q-pt-md">
                            <div class="col-12 col-md-3">
                                <q-input
                                    v-model="voucher.voucherType"
                                    label="Tipo (Factura A, Ticket B, etc.)"
                                    outlined
                                    dense
                                    :readonly="isEdit"
                                />
                            </div>
                            <div class="col-12 col-md-3">
                                <q-input
                                    v-model="voucher.voucherNumber"
                                    label="Número de Comprobante"
                                    outlined
                                    dense
                                    :readonly="isEdit"
                                />
                            </div>
                            <div class="col-12 col-md-3">
                                <q-input
                                    v-model="voucher.cae"
                                    label="CAE"
                                    outlined
                                    dense
                                    :readonly="isEdit"
                                />
                            </div>
                            <div class="col-12 col-md-3">
                                <q-input
                                    v-model="voucher.caeExpiration"
                                    label="Vencimiento CAE"
                                    type="date"
                                    stack-label
                                    outlined
                                    dense
                                    :readonly="isEdit"
                                />
                            </div>
                        </div>
                    </q-expansion-item>
                </q-card-section>

                <q-card-actions align="right">
                    <q-btn flat label="Cancelar" to="/purchases" />
                    <q-btn
                        type="submit"
                        color="primary"
                        label="Guardar Compra"
                        :loading="saving"
                        :disable="
                            saving || formData.details.length === 0 || isEdit
                        "
                    />
                </q-card-actions>
            </q-form>
        </q-card>

        <!-- Quick Add Product Dialog -->
        <q-dialog v-model="quickAddProduct.show" persistent backdrop-filter="blur(4px)">
            <q-card style="width: 600px; max-width: 90vw; border-radius: 15px" class="glass-dialog">
                <q-card-section class="bg-primary text-white row items-center q-pb-none">
                    <div class="text-h6">Nuevo Producto Rápido</div>
                    <q-space />
                    <q-btn icon="close" flat round dense v-close-popup />
                </q-card-section>

                <q-form @submit.prevent="saveQuickAddProduct">
                    <q-card-section class="q-gutter-y-sm">
                        <q-input
                            v-model="quickAddProduct.form.name"
                            label="Nombre del Producto"
                            outlined
                            dense
                            autofocus
                            lazy-rules
                            :rules="[val => !!val || 'Requerido']"
                        />
                        <q-input
                            v-model="quickAddProduct.form.code"
                            label="Código (Opcional)"
                            outlined
                            dense
                        />
                        <div class="row no-wrap items-center">
                            <q-select
                                v-model="quickAddProduct.form.categoryId"
                                :options="categories"
                                label="Categoría"
                                option-value="id"
                                option-label="name"
                                emit-value
                                map-options
                                lazy-rules
                                :rules="[val => !!val || 'Requerido']"
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
                                @click="openQuickAdd('category')"
                            >
                                <q-tooltip>Nueva Categoría</q-tooltip>
                            </q-btn>
                        </div>
                        <div class="row no-wrap items-center">
                            <q-select
                                v-model="quickAddProduct.form.unitId"
                                :options="units"
                                label="Unidad"
                                option-value="id"
                                option-label="name"
                                emit-value
                                map-options
                                lazy-rules
                                :rules="[val => !!val || 'Requerido']"
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
                        <q-input
                            v-model.number="quickAddProduct.form.cost"
                            label="Costo"
                            type="number"
                            step="0.01"
                            prefix="$"
                            outlined
                            dense
                        />
                        <q-input
                            v-model.number="quickAddProduct.form.price"
                            label="Precio de Venta"
                            type="number"
                            step="0.01"
                            prefix="$"
                            outlined
                            dense
                        />
                    </q-card-section>

                    <q-card-actions align="right" class="q-pa-md">
                        <q-btn flat label="Cancelar" color="grey" v-close-popup />
                        <q-btn
                            unelevated
                            label="Guardar Producto"
                            color="primary"
                            type="submit"
                            :loading="quickAddProduct.saving"
                            :disable="!quickAddProduct.form.name || !quickAddProduct.form.categoryId || !quickAddProduct.form.unitId"
                        />
                    </q-card-actions>
                </q-form>
            </q-card>
        </q-dialog>

        <!-- Quick Add Category/Unit Dialog -->
        <q-dialog v-model="quickAdd.show" persistent backdrop-filter="blur(4px)">
            <q-card style="width: 350px; max-width: 90vw; border-radius: 15px" class="glass-dialog">
                <q-card-section class="bg-primary text-white row items-center q-pb-none">
                    <div class="text-h6">Nueva {{ quickAdd.type === 'category' ? 'Categoría' : 'Unidad' }}</div>
                    <q-space />
                    <q-btn icon="close" flat round dense v-close-popup />
                </q-card-section>

                <q-form @submit.prevent="saveQuickAdd">
                    <q-card-section class="q-pt-md q-gutter-y-sm">
                        <q-input
                            v-model="quickAdd.form.name"
                            :label="quickAdd.type === 'category' ? 'Nombre de Categoría' : 'Nombre de Unidad'"
                            autofocus
                            @keyup.enter="saveQuickAdd"
                            :rules="[val => !!val || 'Requerido']"
                            outlined
                            dense
                        />
                        <q-input
                            v-if="quickAdd.type === 'unit'"
                            v-model="quickAdd.form.shortName"
                            label="Nombre Corto"
                            :rules="[val => !!val || 'Requerido']"
                            outlined
                            dense
                        />
                        <q-input
                            v-if="quickAdd.type === 'category'"
                            v-model="quickAdd.form.code"
                            label="Código (Opcional)"
                            outlined
                            dense
                        />
                    </q-card-section>

                    <q-card-actions align="right" class="q-pa-md">
                        <q-btn flat label="Cancelar" color="grey" v-close-popup />
                        <q-btn
                            unelevated
                            label="Guardar"
                            color="primary"
                            type="submit"
                            :loading="quickAdd.saving"
                            :disable="!quickAdd.form.name || (quickAdd.type === 'unit' && !quickAdd.form.shortName)"
                        />
                    </q-card-actions>
                </q-form>
            </q-card>
        </q-dialog>
    </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from "vue";
import { useRouter } from "vue-router";
import { useQuasar } from "quasar";
import { purchaseService } from "@/services/purchase.service";
import { providerService } from "@/services/provider.service";
import { warehouseService } from "@/services/warehouse.service";
import { productService } from "@/services/product.service";
import { categoryService } from "@/services/category.service";
import { unitService } from "@/services/unit.service";
import type { Purchase, PurchaseDetail, Product, Voucher, Category, Unit } from "@/types";

import { useCurrency } from "@/composables/useCurrency";

const $q = useQuasar();
const router = useRouter();
const { formatCurrency, currencySymbol } = useCurrency();
const saving = ref(false);

const providerOptions = ref<any[]>([]);
const warehouseOptions = ref<any[]>([]);
const productOptions = ref<any[]>([]);
let allProducts: Product[] = [];
const selectedProduct = ref<Product | null>(null);

const categories = ref<Category[]>([]);
const units = ref<Unit[]>([]);

// Quick Add Product state
const quickAddProduct = reactive({
    show: false,
    saving: false,
    form: {
        name: '',
        code: '',
        categoryId: undefined as number | undefined,
        unitId: undefined as number | undefined,
        cost: 0,
        price: 0,
    }
});

// Quick Add Category/Unit state
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

const formData = reactive<Partial<Purchase> & { details: PurchaseDetail[] }>({
    ref: `PR-${Date.now()}`,
    date: new Date().toISOString().substr(0, 10),
    providerId: undefined,
    warehouseId: undefined,
    grandTotal: 0,
    paidAmount: 0,
    status: "received",
    paymentStatus: "paid",
    details: [],
});

const voucher = reactive<Partial<Voucher> & { id?: number }>({
    id: undefined,
    voucherType: "",
    voucherNumber: "",
    cae: "",
    caeExpiration: "",
});

const subTotal = ref(0);
const isEdit = ref(false); // Add this

const detailsColumns = [
    {
        name: "name",
        label: "Producto",
        field: "productName",
        align: "left" as const,
    },
    {
        name: "cost",
        label: `Costo (${currencySymbol.value})`,
        field: "cost",
        align: "right" as const,
    },
    {
        name: "quantity",
        label: "Cantidad",
        field: "quantity",
        align: "center" as const,
    },
    { name: "total", label: "Total", field: "total", align: "right" as const },
    { name: "actions", label: "", field: "actions", align: "center" as const },
];

const openQuickAddProduct = () => {
    quickAddProduct.form = { 
        name: '', code: '', categoryId: undefined, unitId: undefined, cost: 0, price: 0 
    };
    quickAddProduct.show = true;
};

const saveQuickAddProduct = async () => {
    quickAddProduct.saving = true;
    try {
        const newProduct = await productService.create({
            ...quickAddProduct.form,
            isActive: true, // Default to active
            notSelling: false, // Default to selling
            stockAlert: 0, // Default
            image: '', // Default
        } as Product);

        // Add to allProducts and select it
        allProducts.push(newProduct.data);
        selectedProduct.value = newProduct.data;
        addProductToPurchase(); // Add to current purchase immediately
        
        $q.notify({ color: 'positive', message: 'Producto creado y añadido a la compra' });
        quickAddProduct.show = false;
    } catch (error) {
        $q.notify({ color: 'negative', message: 'Error al crear producto' });
    } finally {
        quickAddProduct.saving = false;
    }
};

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
            quickAddProduct.form.categoryId = res.data.id;
        } else {
            const res = await unitService.create({ 
                name: quickAdd.form.name, 
                shortName: quickAdd.form.shortName 
            } as any);
            units.value.push(res.data);
            quickAddProduct.form.unitId = res.data.id;
        }
        quickAdd.show = false;
        $q.notify({ color: 'positive', message: 'Registro exitoso' });
    } catch (error) {
        $q.notify({ color: 'negative', message: 'Error al registrar' });
    } finally {
        quickAdd.saving = false;
    }
};

const updateTotals = () => {
    subTotal.value = formData.details.reduce(
        (acc, detail) => acc + detail.quantity * detail.cost,
        0,
    );
    formData.grandTotal =
        subTotal.value - (formData.discount || 0) + (formData.shipping || 0);
};

const addProductToPurchase = () => {
    if (!selectedProduct.value) return;

    const existingDetail = formData.details.find(
        (d: PurchaseDetail) => d.productId === selectedProduct.value!.id,
    );
    if (existingDetail) {
        existingDetail.quantity++;
    } else {
        formData.details.push({
            productId: selectedProduct.value.id!,
            cost: selectedProduct.value.cost,
            quantity: 1,
            total: selectedProduct.value.cost,
            productName: selectedProduct.value.name, // Add product name for display
        });
    }
    updateTotals();
    selectedProduct.value = null;
};

const removeProduct = (row: PurchaseDetail) => {
    const index = formData.details.findIndex(
        (d: PurchaseDetail) => d.productId === row.productId,
    );
    if (index > -1) {
        formData.details.splice(index, 1);
    }
    updateTotals();
};

const filterProducts = (val: string, update: (cb: () => void) => void) => {
    if (val === "") {
        update(() => {
            productOptions.value = [];
        });
        return;
    }
    update(() => {
        const needle = val.toLowerCase();
        productOptions.value = allProducts.filter(
            (v) =>
                v.name.toLowerCase().indexOf(needle) > -1 ||
                v.code.toLowerCase().indexOf(needle) > -1,
        );
    });
};

const onSubmit = async () => {
    saving.value = true;
    try {
        const payload = { ...formData };
        if (voucher.voucherType && voucher.voucherNumber && voucher.cae) {
            (payload as any).voucher = {
                ...voucher,
                issuedAt:
                    voucher.issuedAt || new Date().toISOString().substr(0, 10),
            };
        }
        // Ensure voucher.id is passed if it exists and is an edit
        if (isEdit.value && voucher.id) {
            (payload as any).voucher.id = voucher.id;
        }

        const res = await (isEdit.value
            ? purchaseService.update(formData.id!, payload as Purchase)
            : purchaseService.create(payload as Purchase));
        const purchaseId = res.data?.id || formData.id || 1; // Use formData.id if it's an edit and no new id is returned

        $q.dialog({
            title: "Compra Exitosa",
            message: `La compra se ha registrado correctamente. ¿Desea imprimir el comprobante?`,
            persistent: true,
            ok: {
                label: "Imprimir",
                color: "primary",
                unelevated: true,
                icon: "print",
            },
            cancel: { label: "No, gracias", flat: true, color: "grey" },
        })
            .onOk(() => {
                router.push(`/purchases/print/${purchaseId}`);
            })
            .onCancel(() => {
                router.push("/purchases");
            });
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al registrar la compra",
        });
    } finally {
        saving.value = false;
    }
};

onMounted(async () => {
    const [providersRes, warehousesRes, productsRes, categoriesRes, unitsRes] = await Promise.all([
        providerService.getAll(),
        warehouseService.getAll(),
        productService.getAll(),
        categoryService.getAll(),
        unitService.getAll(),
    ]);
    providerOptions.value = providersRes.data.map((p) => ({
        label: p.name,
        value: p.id,
    }));
    warehouseOptions.value = warehousesRes.data.map((w) => ({
        label: w.name,
        value: w.id,
    }));
    allProducts = productsRes.data;
    categories.value = categoriesRes.data;
    units.value = unitsRes.data;

    // Check if we are in edit mode
    const purchaseId = Number(router.currentRoute.value.params.id);
    if (purchaseId) {
        isEdit.value = true;
        try {
            const res = await purchaseService.getById(purchaseId);
            const purchaseData = res.data;

            if (purchaseData) {
                // Populate formData
                Object.assign(formData, {
                    ...purchaseData,
                    details: purchaseData.details || [],
                });
                // Populate voucher
                if (purchaseData.voucher) {
                    Object.assign(voucher, purchaseData.voucher);
                }
                updateTotals();
            }
        } catch (error) {
            $q.notify({
                color: "negative",
                message: "Error al cargar la compra para edición",
            });
            router.push("/purchases");
        }
    }
});
</script>

<style lang="scss" scoped>
.glass-dialog {
    background: rgba(var(--color-background-elevated-rgb), 0.8);
    backdrop-filter: blur(15px);
    border: 1px solid rgba(255, 255, 255, 0.1);
}
</style>
