<template>
    <q-page padding>
        <q-card>
            <q-card-section>
                <div class="text-h6">
                    {{
                        isEdit
                            ? "Ver Devolución de Compra"
                            : "Registrar Devolución de Compra"
                    }}
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
                                :options="providerOptions"
                                emit-value
                                map-options
                                lazy-rules
                                :rules="[(val) => !!val || 'Requerido']"
                                outlined
                                dense
                                :readonly="isEdit"
                            />
                        </div>
                        <div class="col-12 col-md-4">
                            <q-select
                                v-model="formData.warehouseId"
                                label="Almacén"
                                :options="warehouseOptions"
                                emit-value
                                map-options
                                lazy-rules
                                :rules="[(val) => !!val || 'Requerido']"
                                outlined
                                dense
                                :readonly="isEdit"
                            />
                        </div>
                    </div>

                    <!-- Product Search -->
                    <div class="row items-center q-col-gutter-sm">
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
                            <q-btn
                                color="primary"
                                label="Añadir"
                                @click="addProductToPurchaseReturn"
                                :disable="!selectedProduct || isEdit"
                                class="full-width"
                            />
                        </div>
                    </div>
                    <!-- Details Table -->
                    <div class="app-table-container">
                        <q-table
                            :rows="formData.details"
                            :columns="detailsColumns"
                            row-key="productId"
                            flat
                            bordered
                            :grid="$q.screen.lt.md"
                        >
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
                                                    {{
                                                        props.row.productName ||
                                                        allProducts.find(
                                                            (p) =>
                                                                p.id ===
                                                                props.row
                                                                    .productId,
                                                        )?.name ||
                                                        "Producto"
                                                    }}
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
                                                    <div>
                                                        {{
                                                            formatCurrency(
                                                                props.row.cost,
                                                            )
                                                        }}
                                                    </div>
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
                                    <q-item-section
                                        >Total a Devolver:</q-item-section
                                    >
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
                                    label="Tipo (Nota Crédito, etc.)"
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
                    <q-btn flat label="Cancelar" to="/returns" />
                    <q-btn
                        type="submit"
                        color="primary"
                        label="Guardar Devolución"
                        :loading="saving"
                        :disable="
                            saving || formData.details.length === 0 || isEdit
                        "
                    />
                </q-card-actions>
            </q-form>
        </q-card>
    </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from "vue";
import { useRouter } from "vue-router";
import { useQuasar } from "quasar";
import { returnService } from "@/services/return.service";
import { providerService } from "@/services/provider.service";
import { warehouseService } from "@/services/warehouse.service";
import { productService } from "@/services/product.service";
import type {
    PurchaseReturn,
    PurchaseReturnDetail,
    Product,
    Voucher,
} from "@/types";

import { useCurrency } from "@/composables/useCurrency";

const $q = useQuasar();
const router = useRouter();
const saving = ref(false);
const { formatCurrency } = useCurrency();

const providerOptions = ref<any[]>([]);
const warehouseOptions = ref<any[]>([]);
const productOptions = ref<any[]>([]);
let allProducts: Product[] = [];
const selectedProduct = ref<Product | null>(null);
const isEdit = ref(false);

const formData = reactive<
    Partial<PurchaseReturn> & { details: PurchaseReturnDetail[] }
>({
    ref: `PRR-${Date.now()}`,
    date: new Date().toISOString().substr(0, 10),
    providerId: undefined,
    warehouseId: undefined,
    grandTotal: 0,
    paidAmount: 0,
    status: "completed",
    paymentStatus: "unpaid",
    details: [],
});

const voucher = reactive<Partial<Voucher> & { id?: number }>({
    id: undefined,
    voucherType: "",
    voucherNumber: "",
    cae: "",
    caeExpiration: "",
});

const detailsColumns = [
    {
        name: "name",
        label: "Producto",
        field: (row: any) =>
            allProducts.find((p) => p.id === row.productId)?.name || "",
        align: "left" as const,
    },
    {
        name: "cost",
        label: "Costo",
        field: "cost",
        format: (val: number) => formatCurrency(val),
        align: "right" as const,
    },
    {
        name: "quantity",
        label: "Cantidad",
        field: "quantity",
        align: "center" as const,
    },
    { name: "total", label: "Total", field: "total", align: "right" as const },
    { name: "actions", label: "", field: "actions" },
];

const subTotal = computed(() =>
    formData.details.reduce(
        (acc: number, item: PurchaseReturnDetail) =>
            acc + item.quantity * item.cost,
        0,
    ),
);

const updateTotals = () => {
    formData.grandTotal = subTotal.value;
    formData.paidAmount = subTotal.value; // Assuming fully paid for simplicity, can be changed
};

const addProductToPurchaseReturn = () => {
    if (!selectedProduct.value) return;

    const existingDetail = formData.details.find(
        (d: PurchaseReturnDetail) => d.productId === selectedProduct.value!.id,
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

const removeProduct = (row: PurchaseReturnDetail) => {
    const index = formData.details.findIndex(
        (d: PurchaseReturnDetail) => d.productId === row.productId,
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
        const res = await returnService.createPurchaseReturn(
            payload as PurchaseReturn,
        );
        const returnId = res.data?.id || 1;

        $q.dialog({
            title: "Devolución Exitosa",
            message: `La devolución de compra se ha registrado correctamente. ¿Desea imprimir el comprobante?`,
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
                router.push(`/returns/purchases/print/${returnId}`);
            })
            .onCancel(() => {
                router.push("/returns");
            });
    } catch (error) {
        $q.notify({
            color: "negative",
            message: "Error al registrar la devolución de compra",
        });
    } finally {
        saving.value = false;
    }
};

onMounted(async () => {
    const [providersRes, warehousesRes, productsRes] = await Promise.all([
        providerService.getAll(),
        warehouseService.getAll(),
        productService.getAll(),
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

    // Check if we are in edit mode
    const returnId = Number(router.currentRoute.value.params.id);
    if (returnId) {
        isEdit.value = true;
        try {
            const res = await returnService.getPurchaseReturnById(returnId);
            const returnData = res.data;

            if (returnData) {
                // Populate formData
                Object.assign(formData, {
                    ...returnData,
                    details: returnData.details || [],
                });
                // Populate voucher
                if (returnData.voucher) {
                    Object.assign(voucher, returnData.voucher);
                }
                updateTotals();
            }
        } catch (error) {
            $q.notify({
                color: "negative",
                message: "Error al cargar la devolución para edición",
            });
            router.push("/returns");
        }
    }
});
</script>
