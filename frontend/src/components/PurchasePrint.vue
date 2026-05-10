<template>
  <div class="print-page">
    <div class="print-container" v-if="purchase">
      <div class="header text-center">
        <div class="text-h5">Comprobante de Compra</div>
        <div class="text-subtitle1">Ref: {{ purchase.ref }}</div>
        <div class="text-body2">Fecha: {{ purchase.date ? new Date(purchase.date).toLocaleString() : '' }}</div>
      </div>

      <q-separator class="q-my-md" />

      <div class="content">
        <div v-if="purchase.voucher" class="voucher-info q-mb-md">
          <div><strong>Tipo:</strong> {{ purchase.voucher.voucherType }}</div>
          <div><strong>Número:</strong> {{ purchase.voucher.voucherNumber }}</div>
          <div><strong>CAE:</strong> {{ purchase.voucher.cae }}</div>
        </div>

        <table class="items-table">
          <thead>
            <tr>
              <th class="text-left">Producto</th>
              <th class="text-right">Cant</th>
              <th class="text-right">Costo Unitario</th>
              <th class="text-right">Total</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in purchase.details" :key="item.productId">
              <td>{{ item.productName }}</td>
              <td class="text-right">{{ item.quantity }}</td>
              <td class="text-right">{{ formatCurrency(item.cost) }}</td>
              <td class="text-right">{{ formatCurrency(item.total) }}</td>
            </tr>
          </tbody>
        </table>

        <q-separator class="q-my-md" />

        <div class="totals text-right">
          <div class="text-h6">Total: {{ formatCurrency(purchase.grandTotal) }}</div>
        </div>
      </div>

      <div class="footer text-center q-mt-lg">
        <p>¡Gracias por su negocio!</p>
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
import { purchaseService } from '@/services/purchase.service'
import type { Purchase } from '@/types'

import { useCurrency } from '@/composables/useCurrency';

const route = useRoute()
const purchase = ref<Purchase | null>(null)
const loading = ref(true)
const { formatCurrency } = useCurrency();


const fetchPurchase = async () => {
  try {
    const purchaseId = Number(route.params.id)
    const res = await purchaseService.getById(purchaseId)
    purchase.value = res.data
  } catch (error) {
    console.error("Error fetching purchase:", error)
    // Handle error (e.g., show notification)
  } finally {
    loading.value = false
  }
}

const printVoucher = () => {
  window.print()
}

onMounted(() => {
  fetchPurchase()
})
</script>

<style scoped>
.print-page {
  display: flex;
  justify-content: center;
  padding: 1rem;
}
.print-container {
  width: 100%;
  max-width: 300px; /* Tamaño típico para impresora térmica */
  margin: 0 auto;
  font-family: 'Courier New', Courier, monospace;
  font-size: 10px;
}

.items-table {
  width: 100%;
  border-collapse: collapse;
}

.items-table th, .items-table td {
  padding: 4px;
}

@media print {
  .no-print {
    display: none;
  }
  .print-page {
    padding: 0;
  }
  body {
    background: #fff;
  }
  .q-layout {
      min-height: 0 !important;
  }
}
</style>
