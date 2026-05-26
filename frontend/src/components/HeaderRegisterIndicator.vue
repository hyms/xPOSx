<template>
  <q-btn flat round :color="activeShift ? 'positive' : 'negative'" icon="point_of_sale">
    <q-tooltip>{{ activeShift ? 'Caja Abierta' : 'Caja Cerrada' }}</q-tooltip>
    <q-menu transition-show="jump-down" transition-hide="jump-up">
      <q-list style="min-width: 260px" class="q-pa-sm">
        <q-item v-if="activeShift">
          <q-item-section>
            <q-item-label class="text-weight-bold">Caja Abierta por {{ activeShift.username }}</q-item-label>
            <q-item-label caption class="q-mt-xs">Abierta el: {{ formatDate(activeShift.openedAt) }}</q-item-label>
            <q-item-label caption>Monto Inicial: Bs. {{ activeShift.startingCash.toFixed(2) }}</q-item-label>
            
            <q-btn
              color="negative"
              label="Cerrar Caja (Arqueo)"
              icon="lock"
              size="sm"
              unelevated
              class="q-mt-md full-width"
              @click="showCloseDialog = true"
            />
          </q-item-section>
        </q-item>
        <q-item v-else clickable @click="showOpenDialog = true">
          <q-item-section avatar>
            <q-icon name="lock_open" color="primary" />
          </q-item-section>
          <q-item-section>
            <q-item-label class="text-weight-bold">Caja Cerrada</q-item-label>
            <q-item-label caption>Click para iniciar turno</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-menu>

    <!-- Diálogo Abrir -->
    <q-dialog v-model="showOpenDialog">
      <q-card style="min-width: 320px" class="glass-card">
        <q-card-section class="bg-primary text-white">
          <div class="text-h6">Abrir Turno</div>
        </q-card-section>
        <q-card-section class="q-pt-md">
          <q-select v-model="selectedRegisterId" :options="registers" option-label="name" option-value="id" emit-value map-options label="Seleccionar Caja" outlined dense class="q-mb-md" />
          <q-input v-model.number="startingCash" type="number" label="Monto Inicial" prefix="BOB" outlined dense />
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat label="Cancelar" v-close-popup />
          <q-btn color="primary" label="Abrir" @click="handleOpen" :loading="loading" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <!-- Diálogo Cerrar -->
    <q-dialog v-model="showCloseDialog">
      <q-card style="min-width: 320px" class="glass-card">
        <q-card-section class="bg-negative text-white">
          <div class="text-h6">Cerrar Caja (Arqueo)</div>
        </q-card-section>
        <q-card-section class="q-pt-md">
          <q-input v-model.number="endingCashActual" type="number" label="Efectivo Físico Final" prefix="BOB" outlined dense class="q-mb-md" />
          <q-input v-model="closingNotes" label="Notas de Cierre" type="textarea" outlined dense rows="3" />
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat label="Cancelar" v-close-popup />
          <q-btn color="negative" label="Cerrar" @click="handleClose" :loading="loading" />
        </q-card-actions>
      </q-card>
    </q-dialog>

    <!-- Diálogo Comprobante de Arqueo (Voucher) -->
    <q-dialog v-model="showReceiptDialog" persistent>
      <q-card style="min-width: 400px; max-width: 480px" class="glass-card">
        <q-card-section class="bg-primary text-white row items-center">
          <q-icon name="print" size="md" class="q-mr-sm" />
          <div class="text-h6 text-bold">Comprobante de Arqueo</div>
        </q-card-section>

        <q-card-section class="q-pa-md bg-grey-2 text-dark scroll" style="max-height: 60vh">
          <pre class="q-ma-none text-weight-medium" style="font-family: 'Courier New', Courier, monospace; font-size: 13px; white-space: pre-wrap; line-height: 1.4;">{{ receiptText }}</pre>
        </q-card-section>

        <q-card-actions align="right" class="q-pa-md">
          <q-btn flat label="Cerrar" color="grey" v-close-popup />
          <q-btn color="primary" label="Imprimir" icon="print" @click="printReceipt" unelevated />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-btn>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useCashShiftStore } from '@/stores/cashShiftStore';
import { cashShiftService } from '@/services/cashShift.service';
import { useQuasar } from 'quasar';
import { date } from 'quasar';

const $q = useQuasar();
const store = useCashShiftStore();
const activeShift = computed(() => store.activeShift);
const registers = computed(() => store.registers);
const loading = computed(() => store.loading);

const showOpenDialog = ref(false);
const showCloseDialog = ref(false);
const showReceiptDialog = ref(false);
const receiptText = ref('');

const startingCash = ref(0);
const selectedRegisterId = ref<number | null>(null);
const endingCashActual = ref(0);
const closingNotes = ref('');

const formatDate = (val: string) => date.formatDate(val, 'DD/MM/YYYY HH:mm');

onMounted(async () => {
  await store.fetchActiveShift();
  await store.fetchRegisters();
  if (registers.value.length > 0) {
    selectedRegisterId.value = registers.value[0].id;
  }
});

const handleOpen = async () => {
    if (!selectedRegisterId.value) {
        $q.notify({ color: 'warning', message: 'Debe seleccionar una caja' });
        return;
    }
    try {
        await store.openShift(selectedRegisterId.value, startingCash.value);
        $q.notify({ color: 'positive', message: 'Caja abierta exitosamente' });
        showOpenDialog.value = false;
    } catch (e) {
        $q.notify({ color: 'negative', message: 'Error al abrir caja' });
    }
};

const handleClose = async () => {
    if (!activeShift.value) return;
    const shiftId = activeShift.value.shiftId;
    try {
        await store.closeShift(shiftId, endingCashActual.value, closingNotes.value);
        $q.notify({ color: 'positive', message: 'Caja cerrada exitosamente' });
        showCloseDialog.value = false;

        // Fetch voucher print payload and show it
        try {
            const res = await cashShiftService.getReceiptPayload(shiftId);
            receiptText.value = res.data.formattedText;
            showReceiptDialog.value = true;
        } catch (err) {
            $q.notify({ color: 'warning', message: 'Caja cerrada, pero no se pudo generar el comprobante' });
        }
    } catch (e) {
        $q.notify({ color: 'negative', message: 'Error al cerrar caja' });
    }
};

const printReceipt = () => {
    const printWindow = window.open('', '_blank');
    if (printWindow) {
        printWindow.document.write(`
            <html>
                <head>
                    <title>Imprimir Comprobante de Arqueo</title>
                    <style>
                        body {
                            font-family: 'Courier New', Courier, monospace;
                            white-space: pre-wrap;
                            font-size: 14px;
                            padding: 20px;
                            width: 80mm;
                            margin: 0 auto;
                        }
                    </style>
                </head>
                <body>
                    <pre>${receiptText.value}</pre>
                    <script>
                        window.onload = function() {
                            window.print();
                            window.close();
                        }
                    <\/script>
                </body>
            </html>
        `);
        printWindow.document.close();
    }
};
</script>

<style scoped>
.glass-card {
    background: rgba(255, 255, 255, 0.7);
    backdrop-filter: blur(10px);
    border: 1px solid rgba(255, 255, 255, 0.3);
}
</style>
