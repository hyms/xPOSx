<template>
  <q-page padding>
    <div class="app-table-container">
      <q-table
        title="Cotizaciones"
        :title-style="{ fontFamily: 'var(--font-family-display)', fontSize: '1.5rem', fontWeight: '700' }"
        :rows="quotations"
        :columns="columns"
        row-key="id"
        :loading="loading"
        >
      <template v-slot:top-right>
        <q-btn color="primary" label="Nueva" icon="add" to="/quotations/create" class="full-width-xs" />
      </template>

      <template v-slot:item="props">
        <div class="q-pa-xs col-xs-12 col-sm-6 col-md-4">
          <q-card flat bordered>
            <q-card-section>
              <div class="row items-center justify-between">
                <div class="text-subtitle2 text-weight-bold">{{ props.row.ref }}</div>
                <div class="text-caption text-grey">{{ props.row.date.split('T')[0] }}</div>
              </div>
              <div class="q-mt-sm text-caption text-grey">Cliente: {{ props.row.clientName }}</div>
              <div class="row q-mt-sm items-center justify-between">
                <q-chip :color="getStatusColor(props.row.status)" text-color="white" dense size="sm">
                  {{ props.row.status }}
                </q-chip>
                <div class="text-subtitle1 text-weight-bolder text-primary">
                  {{ formatCurrency(props.row.grandTotal) }}
                </div>
              </div>
            </q-card-section>
            <q-separator />
            <q-card-actions align="right">
              <q-btn flat round color="primary" icon="visibility" size="sm" @click="viewQuotation(props.row.id)" />
              <q-btn flat round color="accent" icon="receipt" size="sm" @click="printQuotation(props.row.id)" />
              <q-btn flat round color="negative" icon="delete" size="sm" @click="confirmDeleteQuotation(props.row)" />
            </q-card-actions>
          </q-card>
        </div>
      </template>

      <template v-slot:body-cell-grandTotal="props">
        <q-td :props="props">
          {{ formatCurrency(props.row.grandTotal) }}
        </q-td>
      </template>

      <template v-slot:body-cell-status="props">
        <q-td :props="props">
          <q-chip :color="getStatusColor(props.row.status)" text-color="white" dense>
            {{ props.row.status }}
          </q-chip>
        </q-td>
      </template>

          <template v-slot:body-cell-actions="props">
            <q-td :props="props">
              <q-btn flat round color="primary" icon="visibility" @click="viewQuotation(props.row.id)" />
              <q-btn flat round color="accent" icon="receipt" @click="printQuotation(props.row.id)" />
              <q-btn flat round color="negative" icon="delete" @click="confirmDeleteQuotation(props.row)" />
            </q-td>
          </template>
    </q-table>
    <!-- Detail Dialog -->
    <QuotationDetailDialog
      v-model="detailDialog"
      :quotation-id="selectedQuotationId"
      :client-name="selectedQuotationClientName"
      :warehouse-name="selectedQuotationWarehouseName"
    />
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import { quotationService } from '@/services/quotation.service';
import type { QuotationReadDto } from '@/types'
import { useConfirm } from '@/composables/useConfirm'
import { useCurrency } from '@/composables/useCurrency';
import QuotationDetailDialog from './components/QuotationDetailDialog.vue';

const $q = useQuasar()
const router = useRouter()
const { confirmDelete } = useConfirm()
const { formatCurrency } = useCurrency();
const loading = ref(false)
const quotations = ref<QuotationReadDto[]>([])
const detailDialog = ref(false)
const selectedQuotationId = ref<number | null>(null)
const selectedQuotationClientName = ref('')
const selectedQuotationWarehouseName = ref('')


const columns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => val.split('T')[0], sortable: true, align: 'left' as const },
  { name: 'ref', label: 'Referencia', field: 'ref', sortable: true, align: 'left' as const },
  { name: 'clientName', label: 'Cliente', field: 'clientName', sortable: true, align: 'left' as const },
  { name: 'warehouseName', label: 'Almacén', field: 'warehouseName', sortable: true, align: 'left' as const },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', sortable: true, align: 'right' as const },
  { name: 'status', label: 'Estado', field: 'status', align: 'center' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const fetchQuotations = async () => {
  loading.value = true
  try {
    const response = await quotationService.getAll()
    quotations.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar cotizaciones' })
  } finally {
    loading.value = false
  }
}

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'sent': return 'positive'
    case 'pending': return 'warning'
    default: return 'grey'
  }
}

const viewQuotation = (id: number) => {
  const row = quotations.value.find((q) => q.id === id);
  selectedQuotationId.value = id;
  selectedQuotationClientName.value = row?.clientName || "";
  selectedQuotationWarehouseName.value = row?.warehouseName || "";
  detailDialog.value = true;
};

const printQuotation = (id: number) => {
  router.push(`/quotations/print/${id}`)
}

const confirmDeleteQuotation = (item: QuotationReadDto) => {
  confirmDelete(item.ref, async () => {
    await quotationService.delete(item.id)
    fetchQuotations()
  })
}

onMounted(fetchQuotations)
</script>
