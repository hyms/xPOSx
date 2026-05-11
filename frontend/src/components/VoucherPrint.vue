<template>
  <div class="print-page">
    <div class="print-container" v-if="sale">
      <div class="header text-center">
        <div class="text-h5" :style="{ fontFamily: 'var(--font-family-display)' }">Comprobante de Venta</div>
        <div class="text-subtitle1" :style="{ fontFamily: 'var(--font-family-body)' }">Ref: {{ sale.ref }}</div>
        <div class="text-body2" :style="{ fontFamily: 'var(--font-family-body)' }">Fecha: {{ sale.date ? new Date(sale.date).toLocaleString() : '' }}</div>
      </div>

      <q-separator class="q-my-md print-separator" />

      <div class="content">
        <div v-if="sale.voucher" class="voucher-info q-mb-md">
          <div :style="{ fontFamily: 'var(--font-family-body)' }"><strong>Tipo:</strong> {{ sale.voucher.voucherType }}</div>
          <div :style="{ fontFamily: 'var(--font-family-body)' }"><strong>Número:</strong> {{ sale.voucher.voucherNumber }}</div>
          <div :style="{ fontFamily: 'var(--font-family-body)' }"><strong>CAE:</strong> {{ sale.voucher.cae }}</div>
          <div v-if="sale.voucher.caeExpiration" :style="{ fontFamily: 'var(--font-family-body)' }"><strong>Vencimiento CAE:</strong> {{ new Date(sale.voucher.caeExpiration).toLocaleDateString() }}</div>
        </div>

        <table class="items-table">
          <thead>
            <tr>
              <th class="text-left table-header" :style="{ fontFamily: 'var(--font-family-body)' }">Producto</th>
              <th class="text-right table-header" :style="{ fontFamily: 'var(--font-family-body)' }">Cant</th>
              <th class="text-right table-header" :style="{ fontFamily: 'var(--font-family-body)' }">Precio Unit.</th>
              <th class="text-right table-header" :style="{ fontFamily: 'var(--font-family-body)' }">Total</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in sale.details" :key="item.productId">
              <td :style="{ fontFamily: 'var(--font-family-body)' }">{{ item.productName }}</td>
              <td class="text-right" :style="{ fontFamily: 'var(--font-family-body)' }">{{ item.quantity }}</td>
              <td class="text-right" :style="{ fontFamily: 'var(--font-family-body)' }">{{ formatCurrency(item.price) }}</td>
              <td class="text-right" :style="{ fontFamily: 'var(--font-family-body)' }">{{ formatCurrency(item.total) }}</td>
            </tr>
          </tbody>
        </table>

        <q-separator class="q-my-md print-separator" />

        <div class="totals text-right">
          <div class="text-h6" :style="{ fontFamily: 'var(--font-family-display)' }">Total: {{ formatCurrency(sale.grandTotal) }}</div>
        </div>
      </div>

      <div class="footer text-center q-mt-lg" :style="{ fontFamily: 'var(--font-family-body)' }">
        <p>¡Gracias por su compra!</p>
      </div>
    </div>
    
    <div class="no-print q-pa-md fixed-bottom-right">
      <q-btn fab color="primary" icon="print" @click="printVoucher" />
    </div>

    <q-inner-loading :showing="loading">
        <q-spinner-gears size="50px" color="primary" />
    </q-inner-loading>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { saleService } from '@/services/sale.service'
import type { Sale } from '@/types'

import { useCurrency } from '@/composables/useCurrency';

const route = useRoute()
const sale = ref<Sale | null>(null)
const loading = ref(true)
const { formatCurrency } = useCurrency();


const fetchSale = async () => {
  try {
    const saleId = Number(route.params.id)
    const res = await saleService.getById(saleId)
    sale.value = res.data
  } catch (error) {
    console.error("Error fetching sale:", error)
    // Handle error (e.g., show notification)
  } finally {
    loading.value = false
  }
}

const printVoucher = () => {
  window.print()
}

onMounted(() => {
  fetchSale()
})
</script>

<style scoped>
.print-page {
  display: flex;
  justify-content: center;
  padding: 1rem;
  background-color: var(--color-background-elevated-light); /* Light background for print preview */

  .body--dark & {
    background-color: var(--color-background-elevated-dark);
  }
}
.print-container {
  width: 100%;
  max-width: 300px; /* Thermal printer size */
  margin: 0 auto;
  font-family: 'Roboto Mono', monospace; /* More modern monospace font */
  font-size: 11px; /* Slightly larger for readability */
  line-height: 1.4;
  color: black; /* Ensure black text for print */
  background-color: white; /* White background for the actual voucher */
  padding: 15px;
  box-shadow: 0 0 10px rgba(0,0,0,0.1); /* Subtle shadow for visual separation */
}

.body--dark .print-container {
  color: #eee; /* Light text for dark mode print preview */
  background-color: #333; /* Dark background for print preview in dark mode */
}


.header .text-h5 {
  font-size: 1.2em;
  font-weight: 700;
  margin-bottom: 5px;
  color: black;
}
.header .text-subtitle1 {
  font-size: 0.9em;
  color: #333;
}
.header .text-body2 {
  font-size: 0.8em;
  color: #555;
}

.print-separator {
  background-color: #ddd !important; /* Light grey separator */
  height: 1px !important;
  margin: 10px 0 !important;
}

.voucher-info div {
  margin-bottom: 3px;
  font-size: 0.9em;
  color: #333;
}
.voucher-info strong {
  font-weight: 700;
  color: black;
}

.items-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 10px;
}

.items-table th, .items-table td {
  padding: 5px 2px; /* Adjusted padding for print */
  border-bottom: 1px dashed #eee; /* Dashed separator for items */
  color: black;
}
.items-table th {
  font-weight: 700;
  text-transform: uppercase;
  font-size: 0.8em;
  color: #333;
}
.items-table td {
  font-size: 0.9em;
}

.totals {
  margin-top: 10px;
  font-size: 1em;
}
.totals .text-h6 {
  font-size: 1.3em;
  font-weight: 700;
  color: black;
}

.footer {
  margin-top: 20px;
  font-size: 0.8em;
  color: #555;
}

@media print {
  .no-print {
    display: none;
  }
  .print-page {
    padding: 0;
    background-color: white !important; /* Ensure white background when actually printing */
  }
  .print-container {
    box-shadow: none !important;
    background-color: white !important;
    color: black !important;
  }
  .header .text-h5, .header .text-subtitle1, .header .text-body2,
  .voucher-info div, .voucher-info strong,
  .items-table th, .items-table td,
  .totals .text-h6,
  .footer {
    color: black !important; /* Force black text for print */
  }
  .print-separator {
    background-color: #eee !important;
  }
  body {
    background: #fff;
  }
  .q-layout {
      min-height: 0 !important;
  }
}
</style>
