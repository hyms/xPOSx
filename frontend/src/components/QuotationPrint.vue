<template>
  <div class="print-page">
    <div class="print-container" v-if="quotation">
      <div class="header text-center">
        <div class="text-h5">Cotización</div>
        <div class="text-subtitle1">Ref: {{ quotation.ref }}</div>
        <div class="text-body2">Fecha: {{ quotation.date ? new Date(quotation.date).toLocaleString() : '' }}</div>
      </div>

      <q-separator class="q-my-md" />

      <div class="content">
        <table class="items-table">
          <thead>
            <tr>
              <th class="text-left">Producto</th>
              <th class="text-right">Cant</th>
              <th class="text-right">Precio</th>
              <th class="text-right">Total</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in quotation.details" :key="item.productId">
              <td>{{ item.name }}</td>
              <td class="text-right">{{ item.quantity }}</td>
              <td class="text-right">{{ formatCurrency(item.price) }}</td>
              <td class="text-right">{{ formatCurrency(item.total) }}</td>
            </tr>
          </tbody>
        </table>

        <q-separator class="q-my-md" />

        <div class="totals text-right">
          <div class="text-h6">Total: {{ formatCurrency(quotation.grandTotal) }}</div>
        </div>
      </div>

      <div class="footer text-center q-mt-lg">
        <p>Este documento es una cotización y no tiene validez fiscal.</p>
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
import { quotationService } from '@/services/quotation.service'
import type { Quotation } from '@/types'

import { useCurrency } from '@/composables/useCurrency';

const route = useRoute()
const quotation = ref<Quotation | null>(null)
const loading = ref(true)
const { formatCurrency } = useCurrency();

const fetchQuotation = async () => {
  try {
    const quotationId = Number(route.params.id)
    const res = await quotationService.getById(quotationId)
    quotation.value = res.data
  } catch (error) {
    console.error("Error fetching quotation:", error)
  } finally {
    loading.value = false
  }
}

const printVoucher = () => {
  window.print()
}

onMounted(() => {
  fetchQuotation()
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
  max-width: 300px;
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
