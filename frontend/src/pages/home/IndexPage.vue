<template>
  <q-page padding>
    <div class="row q-col-gutter-md q-mb-md">
      <!-- Quick Stats -->
      <div class="col-12 col-md-3">
        <q-card class="bg-primary text-white">
          <q-card-section>
            <div class="text-subtitle2">Ventas de Hoy</div>
            <div class="text-h4">{{ formatCurrency(summary.todaySales) }}</div>
            <div class="text-caption">{{ summary.todaySalesCount }} transacciones</div>
          </q-card-section>
        </q-card>
      </div>
      <div class="col-12 col-md-3">
        <q-card class="bg-positive text-white">
          <q-card-section>
            <div class="text-subtitle2">Ventas del Mes</div>
            <div class="text-h4">{{ formatCurrency(summary.monthlySales) }}</div>
          </q-card-section>
        </q-card>
      </div>
      <div class="col-12 col-md-3">
        <q-card class="bg-negative text-white">
          <q-card-section>
            <div class="text-subtitle2">Compras del Mes</div>
            <div class="text-h4">{{ formatCurrency(summary.monthlyPurchases) }}</div>
          </q-card-section>
        </q-card>
      </div>
      <div class="col-12 col-md-3">
        <q-card class="bg-info text-white">
          <q-card-section>
            <div class="text-subtitle2">Balance Mensual</div>
            <div class="text-h4">{{ formatCurrency(summary.monthlySales - summary.monthlyPurchases) }}</div>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <div class="row q-col-gutter-md">
      <!-- Recent Sales Table -->
      <div class="col-12 col-md-6">
        <q-card>
          <q-card-section class="row items-center">
            <div class="text-h6">Ventas Recientes</div>
            <q-space />
            <q-btn flat round icon="visibility" to="/sales" />
          </q-card-section>
          <q-separator />
          <q-table
            flat
            :rows="summary.recentSales"
            :columns="recentColumns"
            row-key="ref"
            hide-pagination
          >
            <template v-slot:body-cell-grandTotal="props">
              <q-td :props="props">{{ formatCurrency(props.row.grandTotal) }}</q-td>
            </template>
            <template v-slot:body-cell-status="props">
              <q-td :props="props">
                <q-chip :color="props.row.status === 'completed' ? 'positive' : 'warning'" text-color="white" dense>
                  {{ props.row.status }}
                </q-chip>
              </q-td>
            </template>
          </q-table>
        </q-card>
      </div>

      <!-- Top Products Table -->
      <div class="col-12 col-md-6">
        <q-card>
          <q-card-section>
            <div class="text-h6">Productos Más Vendidos</div>
          </q-card-section>
          <q-separator />
          <q-table
            flat
            :rows="summary.topProducts"
            :columns="topProductColumns"
            row-key="name"
            hide-pagination
          >
            <template v-slot:body-cell-total="props">
              <q-td :props="props">{{ formatCurrency(props.row.total) }}</q-td>
            </template>
          </q-table>
        </q-card>
      </div>

      <!-- Quick Links -->
      <div class="col-12">
        <q-card>
          <q-card-section>
            <div class="text-h6">Acciones Rápidas</div>
          </q-card-section>
          <q-separator />
          <q-list>
            <q-item clickable to="/pos">
              <q-item-section avatar><q-icon name="point_of_sale" color="primary" /></q-item-section>
              <q-item-section>Abrir POS</q-item-section>
            </q-item>
            <q-item clickable to="/products">
              <q-item-section avatar><q-icon name="inventory_2" color="secondary" /></q-item-section>
              <q-item-section>Gestionar Productos</q-item-section>
            </q-item>
            <q-item clickable to="/reports">
              <q-item-section avatar><q-icon name="bar_chart" color="accent" /></q-item-section>
              <q-item-section>Ver Reportes</q-item-section>
            </q-item>
          </q-list>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { reportService } from '@/services/report.service'
import type { DashboardSummaryDto } from '@/types'

import { useCurrency } from '@/composables/useCurrency';

const summary = ref<DashboardSummaryDto>({
  sales: 0,
  purchases: 0,
  expenses: 0,
  revenue: 0,
  todaySales: 0,
  todaySalesCount: 0,
  monthlySales: 0,
  monthlyPurchases: 0,
  recentSales: [],
  topProducts: []
})
const { formatCurrency } = useCurrency();


const recentColumns = ref([
  { name: 'ref', label: 'Ref', field: 'ref', align: 'left' as const },
  { name: 'clientName', label: 'Cliente', field: 'clientName', align: 'left' as const },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', align: 'right' as const },
  { name: 'status', label: 'Estado', field: 'status', align: 'center' as const }
])

const topProductColumns = ref([
  { name: 'name', label: 'Producto', field: 'name', align: 'left' as const },
  { name: 'quantity', label: 'Cant.', field: 'quantity', align: 'center' as const },
  { name: 'total', label: 'Monto Total', field: 'total', align: 'right' as const }
])

const fetchSummary = async () => {
  try {
    const res = await reportService.getDashboardSummary()
    summary.value = res.data
  } catch (error) {
    console.error('Error fetching dashboard summary:', error)
  }
}

onMounted(fetchSummary)
</script>
