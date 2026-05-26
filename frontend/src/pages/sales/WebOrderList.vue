<template>
  <q-page padding>
    <div class="row q-col-gutter-sm">
      <div class="col-12">
        <q-table
          title="Pedidos Web por Confirmar"
          :rows="webOrders"
          :columns="columns"
          row-key="id"
          :loading="loading"
          :pagination="pagination"
          no-data-label="No hay pedidos web pendientes de verificación"
        >
          <template v-slot:top-right>
            <q-btn
              flat
              round
              dense
              icon="refresh"
              color="primary"
              @click="fetchOrders"
            />
          </template>

          <template v-slot:loading>
            <q-inner-loading showing color="primary" />
          </template>

          <!-- Status column customization -->
          <template v-slot:body-cell-status="props">
            <q-td :props="props">
              <q-badge :color="getStatusColor(props.row.status)">
                {{ getStatusLabel(props.row.status) }}
              </q-badge>
            </q-td>
          </template>

          <!-- Payment status column -->
          <template v-slot:body-cell-paymentStatus="props">
            <q-td :props="props">
              <q-badge :color="props.row.paymentStatus === 'paid' ? 'positive' : 'warning'">
                {{ props.row.paymentStatus === 'paid' ? 'PAGADO' : 'PENDIENTE' }}
              </q-badge>
            </q-td>
          </template>

          <!-- Amount column -->
          <template v-slot:body-cell-grandTotal="props">
            <q-td :props="props" class="text-weight-bold text-right text-primary">
              {{ formatPrice(props.row.grandTotal) }}
            </q-td>
          </template>

          <!-- Date column -->
          <template v-slot:body-cell-date="props">
            <q-td :props="props">
              {{ new Date(props.row.date).toLocaleString() }}
            </q-td>
          </template>

          <!-- Actions column -->
          <template v-slot:body-cell-actions="props">
            <q-td :props="props" class="text-center">
              <q-btn 
                flat
                round
                color="primary" 
                icon="visibility" 
                @click="openVerifyModal(props.row)" 
              />
            </q-td>
          </template>
        </q-table>

    <!-- Glassmorphism Order Verification Modal -->
    <q-dialog v-model="verifyModalOpen">
      <q-card style="width: 800px; max-width: 90vw; border-radius: 12px;">
        <!-- Header -->
        <q-card-section class="row items-center justify-between border-bottom q-py-md">
          <div class="row items-center">
            <q-icon name="qr_code_2" size="md" color="primary" class="q-mr-sm" />
            <div>
              <div class="text-h6 text-weight-bold">Verificar Pedido {{ selectedOrder?.ref }}</div>
              <div class="text-caption text-grey-7">Fecha: {{ selectedOrder ? new Date(selectedOrder.date).toLocaleString() : '' }}</div>
            </div>
          </div>
          <q-btn icon="close" flat round dense v-close-popup />
        </q-card-section>

        <!-- Details -->
        <q-card-section v-if="loadingOrderDetails" class="row justify-center q-py-xl">
          <q-spinner-dots color="primary" size="40px" />
        </q-card-section>

        <q-card-section v-else-if="selectedOrderDetails" class="q-pa-lg">
          <div class="row q-col-gutter-lg">
            <!-- Left subcolumn: Details & Billing -->
            <div class="col-12 col-md-6">
              <!-- Billing Info -->
              <div class="rounded-borders q-pa-md q-mb-md bordered">
                <span class="text-weight-bold text-subtitle1 block text-primary q-mb-sm">Datos de Facturación</span>
                <div class="row q-mb-xs">
                  <span class="col-5">NIT / C.I.:</span>
                  <span class="col-7 text-weight-medium">{{ selectedOrderDetails.nit || 'No provisto' }}</span>
                </div>
                <div class="row q-mb-xs">
                  <span class="col-5">Razón Social:</span>
                  <span class="col-7 text-weight-medium">{{ selectedOrderDetails.razonSocial || 'No provisto' }}</span>
                </div>
                <div class="row q-mb-xs">
                  <span class="col-5">Comentarios:</span>
                  <span class="col-7 text-weight-medium italic text-grey-7">{{ selectedOrderDetails.notes || 'Ninguno' }}</span>
                </div>
              </div>

              <!-- Products -->
              <span class="text-weight-bold text-subtitle1 block text-primary q-mb-sm">Productos Comprados</span>
              <q-list separator class="bordered rounded-borders">
                <q-item v-for="item in selectedOrderDetails.details" :key="item.id" class="q-py-sm">
                  <q-item-section>
                    <q-item-label class="text-weight-bold">{{ item.productName }}</q-item-label>
                    <q-item-label caption>Cantidad: {{ item.quantity }}</q-item-label>
                  </q-item-section>
                  <q-item-section side class="text-right">
                    <span class="text-weight-bold text-primary">{{ formatPrice(item.total) }}</span>
                    <span class="text-caption">{{ formatPrice(item.price) }} c/u</span>
                  </q-item-section>
                </q-item>
              </q-list>

              <div class="row justify-between items-center q-mt-md q-px-sm">
                <span class="text-h6">Total General:</span>
                <span class="text-h5 text-weight-bolder text-primary">{{ formatPrice(selectedOrderDetails.grandTotal) }}</span>
              </div>
            </div>

            <!-- Right subcolumn: Receipt preview -->
            <div class="col-12 col-md-6 text-center">
              <span class="text-weight-bold text-subtitle1 block text-primary text-left q-mb-sm">Comprobante del Cliente</span>
              <div class="rounded-borders bordered q-pa-sm flex flex-center" style="min-height: 250px;">
                <q-img 
                  v-if="selectedOrderDetails.paymentReceiptPath" 
                  :src="getReceiptUrl(selectedOrderDetails.paymentReceiptPath)" 
                  fit="contain" 
                  max-height="300px" 
                  class="cursor-pointer"
                  @click="openFullReceipt"
                >
                  <template #loading>
                    <q-spinner color="primary" size="30px" />
                  </template>
                </q-img>
                <div v-else class="text-grey-6">
                  <q-icon name="image_not_supported" size="50px" />
                  <div>Sin captura de comprobante</div>
                </div>
              </div>
              <p class="text-caption text-grey-6 q-mt-xs">Haz clic en la imagen para ver en tamaño completo</p>
            </div>
          </div>
        </q-card-section>

        <!-- Footer actions -->
        <q-card-actions align="right" class="border-top q-py-md q-px-lg row justify-between">
          <div>
            <q-chip outline color="warning" v-if="selectedOrder?.status === 'PENDING_VERIFICATION'" class="text-weight-bold">
              ESTADO: PENDIENTE DE REVISIÓN
            </q-chip>
          </div>
          <div class="row q-gutter-sm">
            <q-btn 
              color="grey" 
              outline 
              no-caps 
              label="Cerrar" 
              v-close-popup 
              class="q-px-md"
            />
            <q-btn 
              v-if="selectedOrder?.status === 'PENDING_VERIFICATION'"
              color="negative" 
              unelevated 
              no-caps 
              label="Rechazar" 
              icon="cancel" 
              class="q-px-md text-weight-bold" 
              :loading="processing"
              @click="rejectOrder"
            />
            <q-btn 
              v-if="selectedOrder?.status === 'PENDING_VERIFICATION'"
              color="positive" 
              unelevated 
              no-caps 
              label="Aprobar" 
              icon="check_circle" 
              class="q-px-md text-weight-bold" 
              :loading="processing"
              @click="approveOrder"
            />
          </div>
        </q-card-actions>
      </q-card>
    </q-dialog>

    <!-- Fullscreen Receipt Viewer -->
    <q-dialog v-model="fullscreenReceiptOpen">
      <q-card class="bg-black" style="max-width: 95vw; max-height: 95vh;">
        <q-img 
          :src="getReceiptUrl(selectedOrderDetails?.paymentReceiptPath)" 
          fit="contain" 
          style="max-width: 100%; max-height: 90vh;" 
        />
        <q-card-actions align="right" class="bg-grey-10 text-white">
          <q-btn label="Cerrar" color="primary" v-close-popup dense no-caps class="q-px-md" />
        </q-card-actions>
      </q-card>
    </q-dialog>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useQuasar } from 'quasar';
import api from '@/api';
import { useCurrency } from '@/composables/useCurrency';

const $q = useQuasar();
const { formatCurrency } = useCurrency();

const loading = ref(true);
const processing = ref(false);
const verifyModalOpen = ref(false);
const fullscreenReceiptOpen = ref(false);
const loadingOrderDetails = ref(false);

const sales = ref<any[]>([]);
const webOrders = ref<any[]>([]);
const selectedOrder = ref<any>(null);
const selectedOrderDetails = ref<any>(null);

const pagination = ref({
  sortBy: 'date',
  descending: true,
  page: 1,
  rowsPerPage: 10
});

const columns = [
  { name: 'ref', align: 'left' as const, label: 'Referencia', field: 'ref', sortable: true },
  { name: 'date', align: 'left' as const, label: 'Fecha/Hora', field: 'date', sortable: true },
  { name: 'clientName', align: 'left' as const, label: 'Cliente', field: 'clientName', sortable: true },
  { name: 'razonSocial', align: 'left' as const, label: 'Razón Social Factura', field: 'razonSocial', sortable: true },
  { name: 'grandTotal', align: 'right' as const, label: 'Monto Total', field: 'grandTotal', sortable: true },
  { name: 'status', align: 'left' as const, label: 'Estado', field: 'status', sortable: true },
  { name: 'paymentStatus', align: 'left' as const, label: 'Pago', field: 'paymentStatus', sortable: true },
  { name: 'actions', align: 'center' as const, label: 'Acciones', field: 'id' }
];

const fetchOrders = async () => {
  loading.value = true;
  try {
    // Call the base Sales API to list transactions, paging can be customized
    const res = await api.get('/sales', {
      params: {
        page: 1,
        pageSize: 100,
        sortBy: 'date',
        sortDescending: true
      }
    });
    sales.value = res.data.items || [];
    
    // Filter web orders reactively (only those starting with 'WEB-')
    webOrders.value = sales.value.filter(sale => sale.ref && sale.ref.startsWith('WEB-'));
  } catch (error) {
    console.error('Error fetching web orders:', error);
    $q.notify({
      color: 'negative',
      message: 'No se pudieron recuperar los pedidos web.',
      icon: 'report_problem'
    });
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  await fetchOrders();
});

const formatPrice = (price: number) => {
  return formatCurrency ? formatCurrency(price) : `${price} BOB`;
};

const getStatusColor = (status: string) => {
  switch (status.toUpperCase()) {
    case 'PENDING_VERIFICATION': return 'orange';
    case 'PROCESSING': return 'blue';
    case 'COMPLETED': return 'green';
    case 'REJECTED': return 'red';
    case 'CANCELLED': return 'grey';
    default: return 'primary';
  }
};

const getStatusLabel = (status: string) => {
  switch (status.toUpperCase()) {
    case 'PENDING_VERIFICATION': return 'POR VERIFICAR';
    case 'PROCESSING': return 'PROCESANDO';
    case 'COMPLETED': return 'ENTREGADO';
    case 'REJECTED': return 'RECHAZADO';
    case 'CANCELLED': return 'CANCELADO';
    default: return status;
  }
};

const getReceiptUrl = (path: string | null) => {
  if (!path) return '';
  return path.startsWith('http')
    ? path
    : `${process.env.VITE_API_URL ? process.env.VITE_API_URL.replace('/api', '') : ''}${path}`;
};

const openVerifyModal = async (order: any) => {
  selectedOrder.value = order;
  verifyModalOpen.value = true;
  loadingOrderDetails.value = true;

  try {
    // Call the online sales get by ID endpoint to load full details
    const res = await api.get(`/sales/online/${order.id}`);
    selectedOrderDetails.value = res.data;
  } catch (error) {
    console.error('Error loading order details:', error);
    $q.notify({
      color: 'negative',
      message: 'No se pudieron cargar los detalles del pedido.',
      icon: 'report_problem'
    });
    verifyModalOpen.value = false;
  } finally {
    loadingOrderDetails.value = false;
  }
};

const openFullReceipt = () => {
  fullscreenReceiptOpen.value = true;
};

const approveOrder = () => {
  $q.dialog({
    title: 'Aprobar Pedido',
    message: '¿Está seguro de aprobar este pedido? Se actualizará el estado a PROCESANDO y se descontará el stock correspondiente del almacén.',
    ok: { label: 'Aprobar', color: 'positive', flat: false },
    cancel: { label: 'Cancelar', color: 'grey', flat: true },
    persistent: true
  }).onOk(async () => {
    processing.value = true;
    try {
      await api.put(`/sales/online/${selectedOrder.value.id}/approve`);
      $q.notify({
        color: 'positive',
        message: 'El pedido ha sido aprobado exitosamente y el stock ha sido descontado.',
        icon: 'check_circle'
      });
      verifyModalOpen.value = false;
      await fetchOrders();
    } catch (error: any) {
      console.error('Error approving order:', error);
      $q.notify({
        color: 'negative',
        message: error.response?.data?.message || 'Error al aprobar el pedido.',
        icon: 'report_problem'
      });
    } finally {
      processing.value = false;
    }
  });
};

const rejectOrder = () => {
  $q.dialog({
    title: 'Rechazar / Cancelar Pedido',
    message: '¿Está seguro de rechazar este pedido? Su estado cambiará a RECHAZADO.',
    ok: { label: 'Rechazar', color: 'negative', flat: false },
    cancel: { label: 'Cancelar', color: 'grey', flat: true },
    persistent: true
  }).onOk(async () => {
    processing.value = true;
    try {
      await api.put(`/sales/online/${selectedOrder.value.id}/reject`);
      $q.notify({
        color: 'positive',
        message: 'El pedido ha sido rechazado exitosamente.',
        icon: 'check'
      });
      verifyModalOpen.value = false;
      await fetchOrders();
    } catch (error: any) {
      console.error('Error rejecting order:', error);
      $q.notify({
        color: 'negative',
        message: error.response?.data?.message || 'Error al rechazar el pedido.',
        icon: 'report_problem'
      });
    } finally {
      processing.value = false;
    }
  });
};
</script>

<style scoped>
.web-orders-page {
  min-height: calc(100vh - 50px);
}
</style>
