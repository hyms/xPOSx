<template>
    <q-page padding class="reports-page">
        <q-card flat bordered class="q-mb-md">
            <!-- Desktop Tabs -->
            <q-tabs
                v-model="tab"
                dense
                class="text-grey"
                active-color="primary"
                indicator-color="primary"
                align="justify"
                narrow-indicator
            >
                <q-tab
                    v-for="t in tabOptions"
                    :key="t.value"
                    :name="t.value"
                    :label="t.label"
                    :icon="t.icon"
                />
            </q-tabs>

            <q-separator />

            <!-- Filters (Responsive) -->
            <q-expansion-item
                icon="filter_alt"
                label="Filtros de Reporte"
                header-class="text-primary text-weight-bold"
                v-model="filtersExpanded"
            >
                <q-card-section class="row q-col-gutter-sm items-center">
                    <div
                        class="col-12 col-sm-6 col-md-3"
                        v-if="
                            tab === 'sales' ||
                            tab === 'purchases' ||
                            tab === 'profit_loss'
                        "
                    >
                        <q-input
                            v-model="filters.startDate"
                            label="Desde"
                            type="date"
                            dense
                            stack-label
                            outlined
                        />
                    </div>
                    <div
                        class="col-12 col-sm-6 col-md-3"
                        v-if="
                            tab === 'sales' ||
                            tab === 'purchases' ||
                            tab === 'profit_loss'
                        "
                    >
                        <q-input
                            v-model="filters.endDate"
                            label="Hasta"
                            type="date"
                            dense
                            stack-label
                            outlined
                        />
                    </div>
                    <div
                        class="col-12 col-sm-6 col-md-3"
                        v-if="
                            tab === 'sales' ||
                            tab === 'purchases' ||
                            tab === 'stock' ||
                            tab === 'activity'
                        "
                    >
                        <q-select
                            v-model="filters.warehouseId"
                            :options="warehouseOptions"
                            label="Almacén"
                            emit-value
                            map-options
                            dense
                            clearable
                            outlined
                        />
                    </div>
                    <div
                        class="col-12 col-sm-6 col-md-3"
                        v-if="tab === 'activity'"
                    >
                        <q-select
                            v-model="filters.userId"
                            :options="userOptions"
                            label="Usuario"
                            emit-value
                            map-options
                            dense
                            clearable
                            outlined
                        />
                    </div>
                    <div
                        class="col-12 col-sm-6 col-md-3"
                        v-if="tab === 'activity'"
                    >
                        <q-select
                            v-model="filters.productId"
                            :options="productOptions"
                            label="Producto"
                            emit-value
                            map-options
                            dense
                            clearable
                            outlined
                        />
                    </div>
                    <div
                        class="col-12 col-sm-6 col-md-3 flex items-center q-gutter-x-sm"
                    >
                        <q-btn
                            color="primary"
                            label="Filtrar"
                            icon="filter_alt"
                            @click="fetchReport"
                            unelevated
                            class="full-width-xs"
                        />
                        <q-btn
                            color="secondary"
                            label="PDF"
                            icon="picture_as_pdf"
                            @click="exportToPdf"
                            unelevated
                            class="full-width-xs"
                        />
                    </div>
                </q-card-section>
            </q-expansion-item>

            <q-separator />

            <q-tab-panels v-model="tab" animated>
                <!-- Sales Report -->
                <q-tab-panel name="sales" class="q-pa-none">
                    <q-table
                        :rows="salesData"
                        :columns="salesColumns"
                        row-key="ref"
                        :loading="loading"
                        flat
                        bordered
                    >
                        <template v-slot:body-cell-grandTotal="props">
                            <q-td :props="props">{{
                                formatCurrency(props.row.grandTotal)
                            }}</q-td>
                        </template>
                        <template v-slot:item="props">
                            <div class="q-pa-xs col-xs-12 col-sm-6 col-md-4">
                                <q-card flat bordered>
                                    <q-card-section>
                                        <div class="text-subtitle2">
                                            {{ props.row.ref }}
                                        </div>
<div class="text-caption text-grey">
                                             {{ moment(props.row.date).format("DD/MM/YYYY") }}
                                         </div>
                                    </q-card-section>
                                    <q-separator />
                                    <q-card-section>
                                        <div class="row no-wrap items-center">
                                            <div class="col text-grey">
                                                Cliente:
                                            </div>
                                            <div class="col text-right">
                                                {{ props.row.clientName }}
                                            </div>
                                        </div>
                                        <div
                                            class="row no-wrap items-center q-mt-xs"
                                        >
                                            <div class="col text-grey">
                                                Almacén:
                                            </div>
                                            <div class="col text-right">
                                                {{ props.row.warehouseName }}
                                            </div>
                                        </div>
                                        <div
                                            class="row no-wrap items-center q-mt-xs text-weight-bold"
                                        >
                                            <div class="col">Total:</div>
                                            <div
                                                class="col text-right text-primary"
                                            >
                                                {{
                                                    formatCurrency(
                                                        props.row.grandTotal,
                                                    )
                                                }}
                                            </div>
                                        </div>
                                    </q-card-section>
                                </q-card>
                            </div>
                        </template>
                    </q-table>
                </q-tab-panel>

                <!-- Purchases Report -->
                <q-tab-panel name="purchases" class="q-pa-none">
                    <q-table
                        :rows="purchasesData"
                        :columns="purchaseColumns"
                        row-key="ref"
                        :loading="loading"
                        flat
                        bordered
                    >
                        <template v-slot:body-cell-grandTotal="props">
                            <q-td :props="props">{{
                                formatCurrency(props.row.grandTotal)
                            }}</q-td>
                        </template>
                        <template v-slot:item="props">
                            <div class="q-pa-xs col-xs-12 col-sm-6 col-md-4">
                                <q-card flat bordered>
                                    <q-card-section>
                                        <div class="text-subtitle2">
                                            {{ props.row.ref }}
                                        </div>
<div class="text-caption text-grey">
                                             {{ moment(props.row.date).format("DD/MM/YYYY") }}
                                         </div>
                                    </q-card-section>
                                    <q-separator />
                                    <q-card-section>
                                        <div class="row no-wrap items-center">
                                            <div class="col text-grey">
                                                Proveedor:
                                            </div>
                                            <div class="col text-right">
                                                {{ props.row.providerName }}
                                            </div>
                                        </div>
                                        <div
                                            class="row no-wrap items-center q-mt-xs text-weight-bold"
                                        >
                                            <div class="col">Total:</div>
                                            <div
                                                class="col text-right text-primary"
                                            >
                                                {{
                                                    formatCurrency(
                                                        props.row.grandTotal,
                                                    )
                                                }}
                                            </div>
                                        </div>
                                    </q-card-section>
                                </q-card>
                            </div>
                        </template>
                    </q-table>
                </q-tab-panel>

                <!-- Stock Report -->
                <q-tab-panel name="stock" class="q-pa-none">
                    <q-table
                        :rows="stockData"
                        :columns="stockColumns"
                        row-key="productCode"
                        :loading="loading"
                        flat
                        bordered
                    >
                        <template v-slot:body-cell-quantity="props">
                            <q-td :props="props">
                                <q-chip
                                    :color="
                                        props.row.quantity <=
                                        props.row.stockAlert
                                            ? 'negative'
                                            : 'positive'
                                    "
                                    text-color="white"
                                    dense
                                >
                                    {{ props.row.quantity }}
                                </q-chip>
                            </q-td>
                        </template>
                        <template v-slot:item="props">
                            <div class="q-pa-xs col-xs-12 col-sm-6 col-md-4">
                                <q-card flat bordered>
                                    <q-card-section>
                                        <div class="text-subtitle2">
                                            {{ props.row.productName }}
                                        </div>
                                        <div class="text-caption text-grey">
                                            Ref: {{ props.row.productCode }}
                                        </div>
                                    </q-card-section>
                                    <q-separator />
                                    <q-card-section
                                        class="row items-center justify-between"
                                    >
                                        <div>
                                            <q-chip
                                                size="sm"
                                                color="primary"
                                                text-color="white"
                                                >{{
                                                    props.row.categoryName
                                                }}</q-chip
                                            >
                                        </div>
                                        <div class="text-weight-bold">
                                            Stock:
                                            <q-chip
                                                :color="
                                                    props.row.quantity <=
                                                    props.row.stockAlert
                                                        ? 'negative'
                                                        : 'positive'
                                                "
                                                text-color="white"
                                                dense
                                            >
                                                {{ props.row.quantity }}
                                            </q-chip>
                                        </div>
                                    </q-card-section>
                                </q-card>
                            </div>
                        </template>
                    </q-table>
                </q-tab-panel>

                <!-- Profit & Loss -->
                <q-tab-panel name="profit_loss">
                    <div v-if="profitData" class="row q-col-gutter-sm">
                        <div class="col-12 col-md-6">
                            <q-list
                                bordered
                                separator
                                class="rounded-borders overflow-hidden"
                            >
                                <q-item>
                                    <q-item-section
                                        >Total Ventas (+)</q-item-section
                                    >
                                    <q-item-section
                                        side
                                        class="text-positive text-weight-bold"
                                        >{{
                                            formatCurrency(
                                                profitData.totalSales,
                                            )
                                        }}</q-item-section
                                    >
                                </q-item>
                                <q-item>
                                    <q-item-section
                                        >Total Compras (-)</q-item-section
                                    >
                                    <q-item-section
                                        side
                                        class="text-negative"
                                        >{{
                                            formatCurrency(
                                                profitData.totalPurchases,
                                            )
                                        }}</q-item-section
                                    >
                                </q-item>
                                <q-item>
                                    <q-item-section
                                        >Total Gastos (-)</q-item-section
                                    >
                                    <q-item-section
                                        side
                                        class="text-negative"
                                        >{{
                                            formatCurrency(
                                                profitData.totalExpenses,
                                            )
                                        }}</q-item-section
                                    >
                                </q-item>
                                <q-item>
                                    <q-item-section
                                        >Total Devoluciones (-)</q-item-section
                                    >
                                    <q-item-section
                                        side
                                        class="text-negative"
                                        >{{
                                            formatCurrency(
                                                profitData.totalReturns,
                                            )
                                        }}</q-item-section
                                    >
                                </q-item>
                                <q-separator />
                                <q-item class="bg-grey-2">
                                    <q-item-section class="text-h6"
                                        >Utilidad Neta</q-item-section
                                    >
                                    <q-item-section
                                        side
                                        class="text-h6 text-weight-bolder"
                                        :class="
                                            profitData.netProfit >= 0
                                                ? 'text-positive'
                                                : 'text-negative'
                                        "
                                    >
                                        {{
                                            formatCurrency(profitData.netProfit)
                                        }}
                                    </q-item-section>
                                </q-item>
                            </q-list>
                        </div>
                    </div>
                </q-tab-panel>

                <!-- Clients Report -->
                <q-tab-panel name="clients" class="q-pa-none">
                    <q-table
                        :rows="clientData"
                        :columns="clientColumns"
                        row-key="clientId"
                        :loading="loading"
                        flat
                        bordered
                        :grid="$q.screen.lt.md"
                    >
                        <template v-slot:item="props">
                            <div class="q-pa-xs col-xs-12 col-sm-6 col-md-4">
                                <q-card flat bordered>
                                    <q-card-section>
                                        <div class="text-subtitle2">
                                            {{ props.row.clientName }}
                                        </div>
                                        <div class="text-caption text-grey">
                                            {{ props.row.phone }}
                                        </div>
                                    </q-card-section>
                                    <q-separator />
                                    <q-card-section>
                                        <div class="row justify-between">
                                            <span class="text-grey"
                                                >Ventas:</span
                                            >
                                            <span>{{
                                                props.row.totalSales
                                            }}</span>
                                        </div>
                                        <div
                                            class="row justify-between q-mt-xs"
                                        >
                                            <span class="text-grey"
                                                >Deuda:</span
                                            >
                                            <span
                                                :class="
                                                    props.row.dueAmount > 0
                                                        ? 'text-negative text-weight-bold'
                                                        : 'text-positive'
                                                "
                                            >
                                                {{
                                                    formatCurrency(
                                                        props.row.dueAmount,
                                                    )
                                                }}
                                            </span>
                                        </div>
                                    </q-card-section>
                                </q-card>
                            </div>
                        </template>
                    </q-table>
                </q-tab-panel>

                <!-- Providers Report -->
                <q-tab-panel name="providers" class="q-pa-none">
                    <q-table
                        :rows="providerData"
                        :columns="providerColumns"
                        row-key="providerId"
                        :loading="loading"
                        flat
                        bordered
                        :grid="$q.screen.lt.md"
                    >
                        <template v-slot:item="props">
                            <div class="q-pa-xs col-xs-12 col-sm-6 col-md-4">
                                <q-card flat bordered>
                                    <q-card-section>
                                        <div class="text-subtitle2">
                                            {{ props.row.providerName }}
                                        </div>
                                    </q-card-section>
                                    <q-separator />
                                    <q-card-section>
                                        <div class="row justify-between">
                                            <span class="text-grey"
                                                >Compras:</span
                                            >
                                            <span>{{
                                                props.row.totalPurchases
                                            }}</span>
                                        </div>
                                        <div
                                            class="row justify-between q-mt-xs"
                                        >
                                            <span class="text-grey"
                                                >Pendiente:</span
                                            >
                                            <span
                                                :class="
                                                    props.row.dueAmount > 0
                                                        ? 'text-negative text-weight-bold'
                                                        : 'text-positive'
                                                "
                                            >
                                                {{
                                                    formatCurrency(
                                                        props.row.dueAmount,
                                                    )
                                                }}
                                            </span>
                                        </div>
                                    </q-card-section>
                                </q-card>
                            </div>
                        </template>
                    </q-table>
                </q-tab-panel>

                <!-- Top Products Report -->
                <q-tab-panel name="top_products" class="q-pa-none">
                    <q-table
                        :rows="topProductsData"
                        :columns="topProductsColumns"
                        row-key="name"
                        :loading="loading"
                        flat
                        bordered
                        :grid="$q.screen.lt.md"
                    >
                        <template v-slot:body-cell-total="props">
                            <q-td :props="props">{{
                                formatCurrency(props.row.total)
                            }}</q-td>
                        </template>
                        <template v-slot:item="props">
                            <div class="q-pa-xs col-xs-12 col-sm-6">
                                <q-card flat bordered>
                                    <q-card-section>
                                        <div
                                            class="text-subtitle2 text-weight-bold"
                                        >
                                            {{ props.row.name }}
                                        </div>
                                        <div
                                            class="row justify-between q-mt-sm"
                                        >
                                            <span class="text-grey"
                                                >Cantidad:</span
                                            >
                                            <span>{{
                                                props.row.quantity
                                            }}</span>
                                        </div>
                                        <div
                                            class="row justify-between q-mt-xs"
                                        >
                                            <span class="text-grey"
                                                >Total:</span
                                            >
                                            <span
                                                class="text-primary text-weight-bold"
                                                >{{
                                                    formatCurrency(
                                                        props.row.total,
                                                    )
                                                }}</span
                                            >
                                        </div>
                                    </q-card-section>
                                </q-card>
                            </div>
                        </template>
                    </q-table>
                </q-tab-panel>

                <!-- Stock Alerts Report -->
                <q-tab-panel name="stock_alerts" class="q-pa-none">
                    <q-table
                        :rows="stockAlertsData"
                        :columns="stockAlertsColumns"
                        row-key="code"
                        :loading="loading"
                        flat
                        bordered
                        :grid="$q.screen.lt.md"
                    >
                        <template v-slot:item="props">
                            <div class="q-pa-xs col-xs-12 col-sm-6">
                                <q-card flat bordered>
                                    <q-card-section>
                                        <div
                                            class="text-subtitle2 text-weight-bold"
                                        >
                                            {{ props.row.name }}
                                        </div>
                                        <div class="text-caption text-grey">
                                            Código: {{ props.row.code }}
                                        </div>
                                        <div
                                            class="row justify-between q-mt-sm"
                                        >
                                            <span class="text-grey"
                                                >Stock Actual:</span
                                            >
                                            <q-badge color="negative">{{
                                                props.row.quantity
                                            }}</q-badge>
                                        </div>
                                        <div
                                            class="row justify-between q-mt-xs"
                                        >
                                            <span class="text-grey"
                                                >Alerta en:</span
                                            >
                                            <span>{{
                                                props.row.stockAlert
                                            }}</span>
                                        </div>
                                    </q-card-section>
                                </q-card>
                            </div>
                        </template>
                    </q-table>
                </q-tab-panel>

                <!-- Activity Report -->
                <q-tab-panel name="activity" class="q-pa-none">
                    <q-table
                        :rows="activityData"
                        :columns="activityColumns"
                        row-key="ref"
                        :loading="loading"
                        flat
                        bordered
                        :grid="$q.screen.lt.md"
                    >
                        <template v-slot:body-cell-total="props">
                            <q-td :props="props">{{
                                formatCurrency(props.row.total)
                            }}</q-td>
                        </template>
                        <template v-slot:item="props">
                            <div class="q-pa-xs col-xs-12">
                                <q-card flat bordered>
                                    <q-card-section>
                                        <div class="row justify-between">
                                            <div
                                                class="text-subtitle2 text-weight-bold"
                                            >
                                                {{ props.row.ref }}
                                            </div>
                                            <div class="text-caption text-grey">
                                                {{
                                                    props.row.date.split("T")[0]
                                                }}
                                            </div>
                                        </div>
                                        <div class="row q-mt-sm">
                                            <div class="col">
                                                <div
                                                    class="text-caption text-grey"
                                                >
                                                    Tipo:
                                                </div>
                                                <q-chip
                                                    dense
                                                    size="sm"
                                                    color="info"
                                                    text-color="white"
                                                    >{{
                                                        props.row.type
                                                    }}</q-chip
                                                >
                                            </div>
                                            <div class="col text-right">
                                                <div
                                                    class="text-caption text-grey"
                                                >
                                                    Usuario:
                                                </div>
                                                <div>
                                                    {{ props.row.userName }}
                                                </div>
                                            </div>
                                        </div>
                                        <div
                                            class="row justify-between q-mt-sm items-center"
                                        >
                                            <span class="text-grey">{{
                                                props.row.warehouseName
                                            }}</span>
                                            <span
                                                class="text-subtitle2 text-primary"
                                                >{{
                                                    formatCurrency(
                                                        props.row.total,
                                                    )
                                                }}</span
                                            >
                                        </div>
                                    </q-card-section>
                                </q-card>
                            </div>
                        </template>
                    </q-table>
                </q-tab-panel>
            </q-tab-panels>
        </q-card>
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, watch, computed } from "vue";
import moment from "moment";
import "moment/locale/es"; // Import Spanish locale
moment.locale("es"); // Set moment to use Spanish

import { useQuasar } from "quasar";
import { reportService } from "@/services/report.service";
import { warehouseService } from "@/services/warehouse.service";
import { userService } from "@/services/user.service";
import { productService } from "@/services/product.service";
import { pdfService } from "@/services/pdf.service";
import type {
    SalesReportDto,
    PurchaseReportDto,
    StockReportDto,
    ProfitLossReportDto,
    ClientReportDto,
    ProviderReportDto,
    TopProductDto,
    StockAlertReportDto,
    ActivityReportDto,
} from "@/types";
import { useCurrency } from "@/composables/useCurrency";

const $q = useQuasar();
const tab = ref("sales");
const loading = ref(false);
const filtersExpanded = ref(true);
const warehouseOptions = ref<any[]>([]);
const userOptions = ref<any[]>([]);
const productOptions = ref<any[]>([]);
const { formatCurrency } = useCurrency();

const tabOptions = [
    { label: "Ventas", value: "sales", icon: "receipt_long" },
    { label: "Compras", value: "purchases", icon: "shopping_cart" },
    { label: "Inventario", value: "stock", icon: "inventory_2" },
    { label: "Ganancias", value: "profit_loss", icon: "monetization_on" },
    { label: "Clientes", value: "clients", icon: "people" },
    { label: "Proveedores", value: "providers", icon: "local_shipping" },
    { label: "Top Productos", value: "top_products", icon: "trending_up" },
    { label: "Alertas Stock", value: "stock_alerts", icon: "warning" },
    { label: "Actividad", value: "activity", icon: "timeline" },
];

const currentTabIcon = computed(() => {
    return tabOptions.find((t) => t.value === tab.value)?.icon || "bar_chart";
});

const filters = reactive({
    startDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1)
        .toISOString()
        .substr(0, 10),
    endDate: new Date().toISOString().substr(0, 10),
    warehouseId: null,
    clientId: null,
    providerId: null,
    userId: null,
    productId: null,
});

const salesData = ref<SalesReportDto[]>([]);
const purchasesData = ref<PurchaseReportDto[]>([]);
const stockData = ref<StockReportDto[]>([]);
const profitData = ref<ProfitLossReportDto | null>(null);
const clientData = ref<ClientReportDto[]>([]); // New data for client reports
const providerData = ref<ProviderReportDto[]>([]); // New data for provider reports
const topProductsData = ref<TopProductDto[]>([]);
const stockAlertsData = ref<StockAlertReportDto[]>([]);
const activityData = ref<ActivityReportDto[]>([]);

const salesColumns = [
    {
        name: "date",
        label: "Fecha",
        field: "date",
        format: (val: string) => (val ? moment(val).format("DD/MM/YYYY") : ""),
        align: "left" as const,
    },
    { name: "ref", label: "Ref", field: "ref", align: "left" as const },
    {
        name: "clientName",
        label: "Cliente",
        field: "clientName",
        align: "left" as const,
    },
    {
        name: "warehouseName",
        label: "Almacén",
        field: "warehouseName",
        align: "left" as const,
    },
    {
        name: "grandTotal",
        label: "Total",
        field: "grandTotal",
        align: "right" as const,
    },
];

const purchaseColumns = [
    {
        name: "date",
        label: "Fecha",
        field: "date",
        format: (val: string) => (val ? moment(val).format("DD/MM/YYYY") : ""),
        align: "left" as const,
    },
    { name: "ref", label: "Ref", field: "ref", align: "left" as const },
    {
        name: "providerName",
        label: "Proveedor",
        field: "providerName",
        align: "left" as const,
    },
    {
        name: "warehouseName",
        label: "Almacén",
        field: "warehouseName",
        align: "left" as const,
    },
    {
        name: "grandTotal",
        label: "Total",
        field: "grandTotal",
        align: "right" as const,
    },
];

const stockColumns = [
    {
        name: "productCode",
        label: "Código",
        field: "productCode",
        align: "left" as const,
    },
    {
        name: "productName",
        label: "Producto",
        field: "productName",
        align: "left" as const,
    },
    {
        name: "categoryName",
        label: "Categoría",
        field: "categoryName",
        align: "left" as const,
    },
    {
        name: "warehouseName",
        label: "Almacén",
        field: "warehouseName",
        align: "left" as const,
    },
    {
        name: "quantity",
        label: "Stock",
        field: "quantity",
        align: "center" as const,
    },
];

const clientColumns = [
    {
        name: "clientName",
        label: "Cliente",
        field: "clientName",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "phone",
        label: "Teléfono",
        field: "phone",
        align: "left" as const,
    },
    { name: "email", label: "Email", field: "email", align: "left" as const },
    {
        name: "totalSales",
        label: "Total Ventas",
        field: "totalSales",
        sortable: true,
        align: "right" as const,
    },
    {
        name: "totalAmount",
        label: "Monto Total",
        field: "totalAmount",
        sortable: true,
        align: "right" as const,
    },
    {
        name: "totalPaid",
        label: "Monto Pagado",
        field: "totalPaid",
        sortable: true,
        align: "right" as const,
    },
    {
        name: "dueAmount",
        label: "Deuda Pendiente",
        field: "dueAmount",
        sortable: true,
        align: "right" as const,
    },
];

const providerColumns = [
    {
        name: "providerName",
        label: "Proveedor",
        field: "providerName",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "phone",
        label: "Teléfono",
        field: "phone",
        align: "left" as const,
    },
    { name: "email", label: "Email", field: "email", align: "left" as const },
    {
        name: "totalPurchases",
        label: "Total Compras",
        field: "totalPurchases",
        sortable: true,
        align: "right" as const,
    },
    {
        name: "totalAmount",
        label: "Monto Total",
        field: "totalAmount",
        sortable: true,
        align: "right" as const,
    },
    {
        name: "totalPaid",
        label: "Monto Pagado",
        field: "totalPaid",
        sortable: true,
        align: "right" as const,
    },
    {
        name: "dueAmount",
        label: "Deuda Pendiente",
        field: "dueAmount",
        sortable: true,
        align: "right" as const,
    },
];

const topProductsColumns = [
    {
        name: "name",
        label: "Producto",
        field: "name",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "quantity",
        label: "Cantidad Vendida",
        field: "quantity",
        sortable: true,
        align: "right" as const,
    },
    {
        name: "total",
        label: "Total Ventas",
        field: "total",
        sortable: true,
        align: "right" as const,
    },
];

const stockAlertsColumns = [
    {
        name: "code",
        label: "Código",
        field: "code",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "name",
        label: "Producto",
        field: "name",
        sortable: true,
        align: "left" as const,
    },
    {
        name: "quantity",
        label: "Stock Actual",
        field: "quantity",
        sortable: true,
        align: "right" as const,
    },
    {
        name: "stockAlert",
        label: "Alerta de Stock",
        field: "stockAlert",
        sortable: true,
        align: "right" as const,
    },
];

const activityColumns = [
    {
        name: "date",
        label: "Fecha",
        field: "date",
        format: (val: string) => (val ? moment(val).format("DD/MM/YYYY") : ""),
        align: "left" as const,
    },
    { name: "ref", label: "Referencia", field: "ref", align: "left" as const },
    { name: "type", label: "Tipo", field: "type", align: "left" as const },
    {
        name: "warehouseName",
        label: "Almacén",
        field: "warehouseName",
        align: "left" as const,
    },
    {
        name: "userName",
        label: "Usuario",
        field: "userName",
        align: "left" as const,
    },
    { name: "total", label: "Total", field: "total", align: "right" as const },
];

const fetchReport = async () => {
    loading.value = true;
    try {
        if (tab.value === "sales") {
            const res = await reportService.getSalesReport(filters);
            console.log("Sales report response:", res);
            salesData.value = res.data;
        } else if (tab.value === "purchases") {
            const res = await reportService.getPurchaseReport(filters);
            console.log("Purchases report response:", res);
            purchasesData.value = res.data;
        } else if (tab.value === "stock") {
            const res = await reportService.getStockReport({
                warehouseId: filters.warehouseId,
            });
            console.log("Stock report response:", res);
            stockData.value = res.data;
        } else if (tab.value === "profit_loss") {
            const res = await reportService.getProfitLossReport(filters);
            console.log("Profit/Loss report response:", res);
            profitData.value = res.data;
        } else if (tab.value === "clients") {
            const res = await reportService.getClientReport();
            console.log("Clients report response:", res);
            clientData.value = res.data;
        } else if (tab.value === "providers") {
            const res = await reportService.getProviderReport();
            console.log("Providers report response:", res);
            providerData.value = res.data;
        } else if (tab.value === "top_products") {
            const res = await reportService.getTopProductsReport({ limit: 10 }); // Default limit
            console.log("Top products report response:", res);
            topProductsData.value = res.data;
        } else if (tab.value === "stock_alerts") {
            const res = await reportService.getStockAlertsReport();
            console.log("Stock alerts report response:", res);
            stockAlertsData.value = res.data;
        } else if (tab.value === "activity") {
            const res = await reportService.getActivityReport(filters);
            console.log("Activity report response:", res);
            activityData.value = res.data;
        }
    } catch (error) {
        console.error("Error fetching report:", error);
    } finally {
        loading.value = false;
    }
};

const fetchWarehouses = async () => {
    const res = await warehouseService.getAll();
    warehouseOptions.value = res.data.map((w) => ({
        label: w.name,
        value: w.id,
    }));
};

const fetchUsers = async () => {
    const res = await userService.getAll();
    userOptions.value = res.data.map((u) => ({
        label: u.username,
        value: u.id,
    }));
};

const fetchProducts = async () => {
    const res = await productService.getAll();
    productOptions.value = res.data.map((p) => ({
        label: p.name,
        value: p.id,
    }));
};

const exportToPdf = () => {
    let columns: any[] = [];
    let data: any[] = [];
    let title = "";

    if (tab.value === "sales") {
        columns = salesColumns.map((c) => ({
            header: c.label,
            dataKey: c.field as string,
        }));
        data = salesData.value;
        title = "Reporte de Ventas";
    } else if (tab.value === "purchases") {
        columns = purchaseColumns.map((c) => ({
            header: c.label,
            dataKey: c.field as string,
        }));
        data = purchasesData.value;
        title = "Reporte de Compras";
    } else if (tab.value === "clients") {
        columns = clientColumns.map((c) => ({
            header: c.label,
            dataKey: c.field as string,
        }));
        data = clientData.value;
        title = "Reporte de Clientes";
    } else if (tab.value === "providers") {
        columns = providerColumns.map((c) => ({
            header: c.label,
            dataKey: c.field as string,
        }));
        data = providerData.value;
        title = "Reporte de Proveedores";
    } else if (tab.value === "stock") {
        columns = stockColumns.map((c) => ({
            header: c.label,
            dataKey: c.field as string,
        }));
        data = stockData.value;
        title = "Reporte de Inventario";
    } else if (tab.value === "top_products") {
        columns = topProductsColumns.map((c) => ({
            header: c.label,
            dataKey: c.field as string,
        }));
        data = topProductsData.value;
        title = "Reporte de Top Productos";
    } else if (tab.value === "stock_alerts") {
        columns = stockAlertsColumns.map((c) => ({
            header: c.label,
            dataKey: c.field as string,
        }));
        data = stockAlertsData.value;
        title = "Reporte de Alertas de Stock";
    } else if (tab.value === "activity") {
        columns = activityColumns.map((c) => ({
            header: c.label,
            dataKey: c.field as string,
        }));
        data = activityData.value;
        title = "Reporte de Actividad";
    }

    if (columns.length > 0 && data.length > 0) {
        // Format data for PDF (currency, dates, etc.)
        const formattedData = data.map((row) => {
            const newRow = { ...row };
            if (
                tab.value === "sales" ||
                tab.value === "purchases" ||
                tab.value === "activity"
            ) {
                newRow.grandTotal = formatCurrency(row.grandTotal);
                newRow.total = formatCurrency(row.total);
            }
            if (tab.value === "clients" || tab.value === "providers") {
                newRow.totalAmount = formatCurrency(row.totalAmount);
                newRow.totalPaid = formatCurrency(row.totalPaid);
                newRow.dueAmount = formatCurrency(row.dueAmount);
            }
            if (tab.value === "top_products") {
                newRow.total = formatCurrency(row.total);
            }
            return newRow;
        });
        pdfService.generateTablePdf(
            title,
            columns,
            formattedData,
            `${title}.pdf`,
        );
    } else if (tab.value === "profit_loss" && profitData.value) {
        // Custom PDF for Profit & Loss
        const columns = [
            { header: "Concepto", dataKey: "label" },
            { header: "Monto", dataKey: "value" },
        ];
        const data = [
            {
                label: "Total Ventas (+)",
                value: formatCurrency(profitData.value.totalSales),
            },
            {
                label: "Total Compras (-)",
                value: formatCurrency(profitData.value.totalPurchases),
            },
            {
                label: "Total Gastos (-)",
                value: formatCurrency(profitData.value.totalExpenses),
            },
            {
                label: "Total Devoluciones (-)",
                value: formatCurrency(profitData.value.totalReturns),
            },
            {
                label: "Utilidad Neta",
                value: formatCurrency(profitData.value.netProfit),
            },
        ];
        pdfService.generateTablePdf(
            "Reporte de Ganancias y Pérdidas",
            columns,
            data,
            "Reporte_Ganancias_Perdidas.pdf",
        );
    }
};

watch(tab, () => {
    fetchReport();
});

onMounted(() => {
    fetchWarehouses();
    fetchUsers();
    fetchProducts();
    fetchReport();
});
</script>
