<template>
  <q-page padding>
    <q-card>
      <q-tabs
        v-model="tab"
        dense
        class="text-grey"
        active-color="primary"
        indicator-color="primary"
        align="justify"
        narrow-indicator
      >
        <q-tab name="sales" label="Ventas" icon="receipt_long" />
        <q-tab name="purchases" label="Compras" icon="shopping_cart" />
        <q-tab name="stock" label="Inventario" icon="inventory_2" />
        <q-tab name="profit_loss" label="Ganancias / Pérdidas" icon="monetization_on" />
        <q-tab name="clients" label="Clientes" icon="people" />
        <q-tab name="providers" label="Proveedores" icon="local_shipping" />
        <q-tab name="top_products" label="Top Productos" icon="trending_up" />
        <q-tab name="stock_alerts" label="Alertas de Stock" icon="warning" />
        <q-tab name="activity" label="Actividad" icon="timeline" />
      </q-tabs>

      <q-separator />

      <!-- Filters -->
      <q-card-section class="row q-col-gutter-sm items-center">
        <div class="col-12 col-md-3" v-if="tab === 'sales' || tab === 'purchases' || tab === 'profit_loss'">
          <q-input v-model="filters.startDate" label="Desde" type="date" dense stack-label />
        </div>
        <div class="col-12 col-md-3" v-if="tab === 'sales' || tab === 'purchases' || tab === 'profit_loss'">
          <q-input v-model="filters.endDate" label="Hasta" type="date" dense stack-label />
        </div>
        <div class="col-12 col-md-3" v-if="tab === 'sales' || tab === 'purchases' || tab === 'stock' || tab === 'activity'">
          <q-select
            v-model="filters.warehouseId"
            :options="warehouseOptions"
            label="Almacén"
            emit-value
            map-options
            dense
            clearable
          />
        </div>
        <div class="col-12 col-md-3" v-if="tab === 'activity'">
          <q-select
            v-model="filters.userId"
            :options="userOptions"
            label="Usuario"
            emit-value
            map-options
            dense
            clearable
          />
        </div>
        <div class="col-12 col-md-3" v-if="tab === 'activity'">
          <q-select
            v-model="filters.productId"
            :options="productOptions"
            label="Producto"
            emit-value
            map-options
            dense
            clearable
          />
        </div>
        <div class="col-12 col-md-3">
          <q-btn color="primary" label="Filtrar" icon="filter_alt" @click="fetchReport" class="q-mr-sm" />
          <q-btn color="secondary" label="PDF" icon="picture_as_pdf" @click="exportToPdf" />
        </div>
      </q-card-section>

      <q-tab-panels v-model="tab" animated>
        <!-- Sales Report -->
        <q-tab-panel name="sales">
          <q-table
            :rows="salesData"
            :columns="salesColumns"
            row-key="ref"
            :loading="loading"
          >
            <template v-slot:body-cell-grandTotal="props">
              <q-td :props="props">{{ formatCurrency(props.row.grandTotal) }}</q-td>
            </template>
          </q-table>
        </q-tab-panel>

        <!-- Purchases Report -->
        <q-tab-panel name="purchases">
          <q-table
            :rows="purchasesData"
            :columns="purchaseColumns"
            row-key="ref"
            :loading="loading"
          >
            <template v-slot:body-cell-grandTotal="props">
              <q-td :props="props">{{ formatCurrency(props.row.grandTotal) }}</q-td>
            </template>
          </q-table>
        </q-tab-panel>

        <!-- Stock Report -->
        <q-tab-panel name="stock">
          <q-table
            :rows="stockData"
            :columns="stockColumns"
            row-key="productCode"
            :loading="loading"
          >
            <template v-slot:body-cell-quantity="props">
              <q-td :props="props">
                <q-chip :color="props.row.quantity <= props.row.stockAlert ? 'negative' : 'positive'" text-color="white" dense>
                  {{ props.row.quantity }}
                </q-chip>
              </q-td>
            </template>
          </q-table>
        </q-tab-panel>

        <!-- Profit & Loss -->
        <q-tab-panel name="profit_loss">
          <div v-if="profitData" class="row q-col-gutter-md">
            <div class="col-12 col-md-6">
              <q-list bordered separator>
                <q-item>
                  <q-item-section>Total Ventas (+)</q-item-section>
                  <q-item-section side class="text-positive text-weight-bold">{{ formatCurrency(profitData.totalSales) }}</q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>Total Compras (-)</q-item-section>
                  <q-item-section side class="text-negative">{{ formatCurrency(profitData.totalPurchases) }}</q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>Total Gastos (-)</q-item-section>
                  <q-item-section side class="text-negative">{{ formatCurrency(profitData.totalExpenses) }}</q-item-section>
                </q-item>
                <q-item>
                  <q-item-section>Total Devoluciones (-)</q-item-section>
                  <q-item-section side class="text-negative">{{ formatCurrency(profitData.totalReturns) }}</q-item-section>
                </q-item>
                <q-separator />
                <q-item class="bg-grey-2">
                  <q-item-section class="text-h6">Utilidad Neta</q-item-section>
                  <q-item-section side class="text-h6 text-weight-bolder" :class="profitData.netProfit >= 0 ? 'text-positive' : 'text-negative'">
                    {{ formatCurrency(profitData.netProfit) }}
                  </q-item-section>
                </q-item>
              </q-list>
            </div>
          </div>
        </q-tab-panel>

        <!-- Clients Report -->
        <q-tab-panel name="clients">
          <q-table
            :rows="clientData"
            :columns="clientColumns"
            row-key="clientId"
            :loading="loading"
          >
            <template v-slot:body-cell-totalAmount="props">
              <q-td :props="props">{{ formatCurrency(props.row.totalAmount) }}</q-td>
            </template>
            <template v-slot:body-cell-totalPaid="props">
              <q-td :props="props">{{ formatCurrency(props.row.totalPaid) }}</q-td>
            </template>
            <template v-slot:body-cell-dueAmount="props">
              <q-td :props="props" :class="props.row.dueAmount > 0 ? 'text-negative text-weight-bold' : 'text-positive'">
                {{ formatCurrency(props.row.dueAmount) }}
              </q-td>
            </template>
          </q-table>
        </q-tab-panel>

        <!-- Providers Report -->
        <q-tab-panel name="providers">
          <q-table
            :rows="providerData"
            :columns="providerColumns"
            row-key="providerId"
            :loading="loading"
          >
            <template v-slot:body-cell-totalAmount="props">
              <q-td :props="props">{{ formatCurrency(props.row.totalAmount) }}</q-td>
            </template>
            <template v-slot:body-cell-totalPaid="props">
              <q-td :props="props">{{ formatCurrency(props.row.totalPaid) }}</q-td>
            </template>
            <template v-slot:body-cell-dueAmount="props">
              <q-td :props="props" :class="props.row.dueAmount > 0 ? 'text-negative text-weight-bold' : 'text-positive'">
                {{ formatCurrency(props.row.dueAmount) }}
              </q-td>
            </template>
          </q-table>
        </q-tab-panel>

        <!-- Top Products Report -->
        <q-tab-panel name="top_products">
          <q-table
            :rows="topProductsData"
            :columns="topProductsColumns"
            row-key="name"
            :loading="loading"
          />
        </q-tab-panel>

        <!-- Stock Alerts Report -->
        <q-tab-panel name="stock_alerts">
          <q-table
            :rows="stockAlertsData"
            :columns="stockAlertsColumns"
            row-key="code"
            :loading="loading"
          />
        </q-tab-panel>
        
        <!-- Activity Report -->
        <q-tab-panel name="activity">
          <q-table
            :rows="activityData"
            :columns="activityColumns"
            row-key="ref"
            :loading="loading"
          />
        </q-tab-panel>
      </q-tab-panels>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, watch } from 'vue'
import { reportService } from '@/services/report.service'
import { warehouseService } from '@/services/warehouse.service'
import { userService } from '@/services/user.service'
import { productService } from '@/services/product.service'
import { pdfService } from '@/services/pdf.service' // Import our new service
import type { SalesReportDto, PurchaseReportDto, StockReportDto, ProfitLossReportDto, ClientReportDto, ProviderReportDto, TopProductDto, StockAlertReportDto, ActivityReportDto } from '@/types'

const tab = ref('sales')
const loading = ref(false)
const warehouseOptions = ref<any[]>([])
const userOptions = ref<any[]>([])
const productOptions = ref<any[]>([])

const filters = reactive({
  startDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().substr(0, 10),
  endDate: new Date().toISOString().substr(0, 10),
  warehouseId: null,
  clientId: null,
  providerId: null,
  userId: null,
  productId: null
})

const salesData = ref<SalesReportDto[]>([])
const purchasesData = ref<PurchaseReportDto[]>([])
const stockData = ref<StockReportDto[]>([])
const profitData = ref<ProfitLossReportDto | null>(null)
const clientData = ref<ClientReportDto[]>([]) // New data for client reports
const providerData = ref<ProviderReportDto[]>([]) // New data for provider reports
const topProductsData = ref<TopProductDto[]>([])
const stockAlertsData = ref<StockAlertReportDto[]>([])
const activityData = ref<ActivityReportDto[]>([])

const salesColumns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => val.split('T')[0], align: 'left' as const },
  { name: 'ref', label: 'Ref', field: 'ref', align: 'left' as const },
  { name: 'clientName', label: 'Cliente', field: 'clientName', align: 'left' as const },
  { name: 'warehouseName', label: 'Almacén', field: 'warehouseName', align: 'left' as const },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', align: 'right' as const }
]

const purchaseColumns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => val.split('T')[0], align: 'left' as const },
  { name: 'ref', label: 'Ref', field: 'ref', align: 'left' as const },
  { name: 'providerName', label: 'Proveedor', field: 'providerName', align: 'left' as const },
  { name: 'warehouseName', label: 'Almacén', field: 'warehouseName', align: 'left' as const },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', align: 'right' as const }
]

const stockColumns = [
  { name: 'productCode', label: 'Código', field: 'productCode', align: 'left' as const },
  { name: 'productName', label: 'Producto', field: 'productName', align: 'left' as const },
  { name: 'categoryName', label: 'Categoría', field: 'categoryName', align: 'left' as const },
  { name: 'warehouseName', label: 'Almacén', field: 'warehouseName', align: 'left' as const },
  { name: 'quantity', label: 'Stock', field: 'quantity', align: 'center' as const }
]

const clientColumns = [
  { name: 'clientName', label: 'Cliente', field: 'clientName', sortable: true, align: 'left' as const },
  { name: 'phone', label: 'Teléfono', field: 'phone', align: 'left' as const },
  { name: 'email', label: 'Email', field: 'email', align: 'left' as const },
  { name: 'totalSales', label: 'Total Ventas', field: 'totalSales', sortable: true, align: 'right' as const },
  { name: 'totalAmount', label: 'Monto Total', field: 'totalAmount', sortable: true, align: 'right' as const },
  { name: 'totalPaid', label: 'Monto Pagado', field: 'totalPaid', sortable: true, align: 'right' as const },
  { name: 'dueAmount', label: 'Deuda Pendiente', field: 'dueAmount', sortable: true, align: 'right' as const }
]

const providerColumns = [
  { name: 'providerName', label: 'Proveedor', field: 'providerName', sortable: true, align: 'left' as const },
  { name: 'phone', label: 'Teléfono', field: 'phone', align: 'left' as const },
  { name: 'email', label: 'Email', field: 'email', align: 'left' as const },
  { name: 'totalPurchases', label: 'Total Compras', field: 'totalPurchases', sortable: true, align: 'right' as const },
  { name: 'totalAmount', label: 'Monto Total', field: 'totalAmount', sortable: true, align: 'right' as const },
  { name: 'totalPaid', label: 'Monto Pagado', field: 'totalPaid', sortable: true, align: 'right' as const },
  { name: 'dueAmount', label: 'Deuda Pendiente', field: 'dueAmount', sortable: true, align: 'right' as const }
]

const topProductsColumns = [
  { name: 'name', label: 'Producto', field: 'name', sortable: true, align: 'left' as const },
  { name: 'quantity', label: 'Cantidad Vendida', field: 'quantity', sortable: true, align: 'right' as const },
  { name: 'total', label: 'Total Ventas', field: 'total', sortable: true, align: 'right' as const }
]

const stockAlertsColumns = [
  { name: 'code', label: 'Código', field: 'code', sortable: true, align: 'left' as const },
  { name: 'name', label: 'Producto', field: 'name', sortable: true, align: 'left' as const },
  { name: 'quantity', label: 'Stock Actual', field: 'quantity', sortable: true, align: 'right' as const },
  { name: 'stockAlert', label: 'Alerta de Stock', field: 'stockAlert', sortable: true, align: 'right' as const }
]

const activityColumns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => val.split('T')[0], align: 'left' as const },
  { name: 'ref', label: 'Referencia', field: 'ref', align: 'left' as const },
  { name: 'type', label: 'Tipo', field: 'type', align: 'left' as const },
  { name: 'warehouseName', label: 'Almacén', field: 'warehouseName', align: 'left' as const },
  { name: 'userName', label: 'Usuario', field: 'userName', align: 'left' as const },
  { name: 'total', label: 'Total', field: 'total', align: 'right' as const }
]

const fetchReport = async () => {
  loading.value = true
  try {
    if (tab.value === 'sales') {
      const res = await reportService.getSalesReport(filters)
      salesData.value = res.data
    } else if (tab.value === 'purchases') {
      const res = await reportService.getPurchaseReport(filters)
      purchasesData.value = res.data
    } else if (tab.value === 'stock') {
      const res = await reportService.getStockReport({ warehouseId: filters.warehouseId })
      stockData.value = res.data
    } else if (tab.value === 'profit_loss') {
      const res = await reportService.getProfitLossReport(filters)
      profitData.value = res.data
    } else if (tab.value === 'clients') {
      const res = await reportService.getClientReport()
      clientData.value = res.data
    } else if (tab.value === 'providers') {
      const res = await reportService.getProviderReport()
      providerData.value = res.data
    } else if (tab.value === 'top_products') {
      const res = await reportService.getTopProductsReport({ limit: 10 }) // Default limit
      topProductsData.value = res.data
    } else if (tab.value === 'stock_alerts') {
      const res = await reportService.getStockAlertsReport()
      stockAlertsData.value = res.data
    } else if (tab.value === 'activity') {
      const res = await reportService.getActivityReport(filters)
      activityData.value = res.data
    }
  } catch (error) {
    console.error('Error fetching report:', error)
  } finally {
    loading.value = false
  }
}

const fetchWarehouses = async () => {
  const res = await warehouseService.getAll()
  warehouseOptions.value = res.data.map(w => ({ label: w.name, value: w.id }))
}

const fetchUsers = async () => {
  const res = await userService.getAll()
  userOptions.value = res.data.map(u => ({ label: u.username, value: u.id }))
}

const fetchProducts = async () => {
  const res = await productService.getAll()
  productOptions.value = res.data.map(p => ({ label: p.name, value: p.id }))
}

const formatCurrency = (val: number) => {
  return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(val)
}

const exportToPdf = () => {
  let columns: any[] = []
  let data: any[] = []
  let title = ''

  if (tab.value === 'sales') {
    columns = salesColumns.map(c => ({ header: c.label, dataKey: c.field as string }))
    data = salesData.value
    title = 'Reporte de Ventas'
  } else if (tab.value === 'purchases') {
    columns = purchaseColumns.map(c => ({ header: c.label, dataKey: c.field as string }))
    data = purchasesData.value
    title = 'Reporte de Compras'
  } else if (tab.value === 'clients') {
    columns = clientColumns.map(c => ({ header: c.label, dataKey: c.field as string }))
    data = clientData.value
    title = 'Reporte de Clientes'
  } else if (tab.value === 'providers') {
    columns = providerColumns.map(c => ({ header: c.label, dataKey: c.field as string }))
    data = providerData.value
    title = 'Reporte de Proveedores'
  }
  // Add more cases for other reports if needed

  if (columns.length > 0 && data.length > 0) {
    pdfService.generateTablePdf(title, columns, data, `${title}.pdf`)
  }
}

watch(tab, () => {
  fetchReport()
})

onMounted(() => {
  fetchWarehouses()
  fetchUsers()
  fetchProducts()
  fetchReport()
})
</script>
