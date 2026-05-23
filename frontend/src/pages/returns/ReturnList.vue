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
        <q-tab name="sale_returns" label="Devoluciones Ventas" icon="assignment_return" />
        <q-tab name="purchase_returns" label="Devoluciones Compras" icon="keyboard_return" />
      </q-tabs>

      <q-separator />

      <q-tab-panels v-model="tab" animated>
        <!-- Sale Returns Tab -->
        <q-tab-panel name="sale_returns">
          <q-table
            title="Devoluciones de Ventas"
            :rows="saleReturns"
            :columns="saleColumns"
            row-key="id"
            :loading="loadingSales"
            >
            <template v-slot:top-right>
              <q-btn color="primary" label="Nueva" icon="add" to="/returns/sales/create" class="full-width-xs" />
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
                    <div class="q-mt-xs text-caption text-grey">Almacén: {{ props.row.warehouseName }}</div>
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
                    <q-btn flat round color="primary" icon="visibility" size="sm" @click="viewSaleReturn(props.row.id)" />
                    <q-btn flat round color="accent" icon="receipt" size="sm" @click="printSaleReturnVoucher(props.row.id)" />
                    <q-btn flat round color="negative" icon="delete" size="sm" @click="confirmDeleteSaleReturn(props.row)" />
                  </q-card-actions>
                </q-card>
              </div>
            </template>
          </q-table>
        </q-tab-panel>

        <!-- Purchase Returns Tab -->
        <q-tab-panel name="purchase_returns">
          <q-table
            title="Devoluciones de Compras"
            :rows="purchaseReturns"
            :columns="purchaseColumns"
            row-key="id"
            :loading="loadingPurchases"
            >
            <template v-slot:top-right>
              <q-btn color="primary" label="Nueva" icon="add" to="/returns/purchases/create" class="full-width-xs" />
            </template>

            <template v-slot:item="props">
              <div class="q-pa-xs col-xs-12 col-sm-6 col-md-4">
                <q-card flat bordered>
                  <q-card-section>
                    <div class="row items-center justify-between">
                      <div class="text-subtitle2 text-weight-bold">{{ props.row.ref }}</div>
                      <div class="text-caption text-grey">{{ props.row.date.split('T')[0] }}</div>
                    </div>
                    <div class="q-mt-sm text-caption text-grey">Proveedor: {{ props.row.providerName }}</div>
                    <div class="q-mt-xs text-caption text-grey">Almacén: {{ props.row.warehouseName }}</div>
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
                    <q-btn flat round color="primary" icon="visibility" size="sm" @click="viewPurchaseReturn(props.row.id)" />
                    <q-btn flat round color="accent" icon="receipt" size="sm" @click="printPurchaseReturnVoucher(props.row.id)" />
                    <q-btn flat round color="negative" icon="delete" size="sm" @click="confirmDeletePurchaseReturn(props.row)" />
                  </q-card-actions>
                </q-card>
              </div>
            </template>
          </q-table>
        </q-tab-panel>
      </q-tab-panels>
    </q-card>
    <!-- Return Detail Dialog -->
    <ReturnDetailDialog
      v-model="detailDialog"
      :return-id="selectedReturnId"
      :return-type="returnType"
      :associate-name="selectedAssociateName"
      :warehouse-name="selectedWarehouseName"
    />
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useQuasar } from 'quasar'
import { returnService } from '@/services/return.service'
import { useConfirm } from '@/composables/useConfirm'
import type { SaleReturnReadDto, PurchaseReturnReadDto } from '@/types'
import { useRouter } from 'vue-router'
import { useCurrency } from '@/composables/useCurrency';
import ReturnDetailDialog from './components/ReturnDetailDialog.vue';

const $q = useQuasar()
const { confirmDelete } = useConfirm()
const router = useRouter()
const tab = ref('sale_returns')
const { formatCurrency } = useCurrency();

const detailDialog = ref(false)
const selectedReturnId = ref<number | null>(null)
const returnType = ref<'sale' | 'purchase'>('sale')
const selectedAssociateName = ref('')
const selectedWarehouseName = ref('')

const loadingSales = ref(false)
const loadingPurchases = ref(false)

const saleReturns = ref<SaleReturnReadDto[]>([])
const purchaseReturns = ref<PurchaseReturnReadDto[]>([])

const saleColumns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => val.split('T')[0], sortable: true, align: 'left' as const },
  { name: 'ref', label: 'Referencia', field: 'ref', sortable: true, align: 'left' as const },
  { name: 'clientName', label: 'Cliente', field: 'clientName', sortable: true, align: 'left' as const },
  { name: 'warehouseName', label: 'Almacén', field: 'warehouseName', sortable: true, align: 'left' as const },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', sortable: true, align: 'right' as const },
  { name: 'voucherId', label: 'Voucher ID', field: 'voucherId', sortable: true, align: 'left' as const, format: (val: any) => val || 'N/A'  },
  { name: 'status', label: 'Estado', field: 'status', align: 'center' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const purchaseColumns = [
  { name: 'date', label: 'Fecha', field: 'date', format: (val: string) => val.split('T')[0], sortable: true, align: 'left' as const },
  { name: 'ref', label: 'Referencia', field: 'ref', sortable: true, align: 'left' as const },
  { name: 'providerName', label: 'Proveedor', field: 'providerName', sortable: true, align: 'left' as const },
  { name: 'warehouseName', label: 'Almacén', field: 'warehouseName', sortable: true, align: 'left' as const },
  { name: 'grandTotal', label: 'Total', field: 'grandTotal', sortable: true, align: 'right' as const },
  { name: 'voucherId', label: 'Voucher ID', field: 'voucherId', sortable: true, align: 'left' as const, format: (val: any) => val || 'N/A'  },
  { name: 'status', label: 'Estado', field: 'status', align: 'center' as const },
  { name: 'actions', label: 'Acciones', field: 'actions', align: 'center' as const }
]

const fetchSaleReturns = async () => {
  loadingSales.value = true
  try {
    const response = await returnService.getAllSaleReturns()
    saleReturns.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar devoluciones de ventas' })
  } finally {
    loadingSales.value = false
  }
}

const fetchPurchaseReturns = async () => {
  loadingPurchases.value = true
  try {
    const response = await returnService.getAllPurchaseReturns()
    purchaseReturns.value = response.data
  } catch (error) {
    $q.notify({ color: 'negative', message: 'Error al cargar devoluciones de compras' })
  } finally {
    loadingPurchases.value = false
  }
}

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'completed': return 'positive'
    case 'pending': return 'warning'
    default: return 'grey'
  }
}

const viewSaleReturn = (id: number) => {
  const row = saleReturns.value.find((r) => r.id === id);
  selectedReturnId.value = id;
  returnType.value = 'sale';
  selectedAssociateName.value = row?.clientName || '';
  selectedWarehouseName.value = row?.warehouseName || '';
  detailDialog.value = true;
}

const printSaleReturnVoucher = (id: number) => {
  router.push(`/returns/sales/print/${id}`)
}

const viewPurchaseReturn = (id: number) => {
  const row = purchaseReturns.value.find((r) => r.id === id);
  selectedReturnId.value = id;
  returnType.value = 'purchase';
  selectedAssociateName.value = row?.providerName || '';
  selectedWarehouseName.value = row?.warehouseName || '';
  detailDialog.value = true;
}

const printPurchaseReturnVoucher = (id: number) => {
  router.push(`/returns/purchases/print/${id}`)
}

const confirmDeleteSaleReturn = (item: SaleReturnReadDto) => {
  confirmDelete(item.ref, async () => {
    await returnService.deleteSaleReturn(item.id)
    fetchSaleReturns()
  })
}

const confirmDeletePurchaseReturn = (item: PurchaseReturnReadDto) => {
  confirmDelete(item.ref, async () => {
    await returnService.deletePurchaseReturn(item.id)
    fetchPurchaseReturns()
  })
}

onMounted(() => {
  fetchSaleReturns()
  fetchPurchaseReturns()
})
</script>
