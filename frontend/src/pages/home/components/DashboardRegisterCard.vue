<template>
    <q-card flat bordered>
        <q-card-section>
            <div class="row items-center justify-between">
                <div class="text-h6">Caja: {{ registerName }}</div>
                <q-badge :color="activeShift ? 'positive' : 'negative'" class="text-uppercase">
                    {{ activeShift ? 'Abierta' : 'Cerrada' }}
                </q-badge>
            </div>
        </q-card-section>

        <q-card-section>
            <div v-if="activeShift">
                <div class="text-subtitle2">Abierta el {{ formatDate(activeShift.openedAt) }}</div>
                <q-btn color="negative" label="Cerrar Caja" class="q-mt-md full-width" @click="showCloseDialog = true" />
            </div>
            <div v-else>
                <div class="text-subtitle2">Caja actualmente cerrada</div>
                <q-btn color="primary" label="Abrir Caja" class="q-mt-md full-width" @click="openDialog" />
            </div>
        </q-card-section>

        <!-- Diálogo Cerrar -->
        <q-dialog v-model="showCloseDialog">
            <q-card style="min-width: 300px">
                <q-card-section>
                    <div class="text-h6">Cerrar Turno</div>
                </q-card-section>
                <q-card-section>
                    <q-input v-model.number="endingCashActual" type="number" label="Efectivo Físico Final" prefix="BOB" outlined dense />
                    <q-input v-model="closingNotes" label="Notas de cierre" outlined dense class="q-mt-sm" />
                </q-card-section>
                <q-card-actions align="right">
                    <q-btn flat label="Cancelar" v-close-popup />
                    <q-btn color="negative" label="Cerrar" @click="handleClose" :loading="loading" />
                </q-card-actions>
            </q-card>
        </q-dialog>

        <!-- Diálogo Abrir -->
        <q-dialog v-model="showOpenDialog">
            <q-card style="min-width: 300px">
                <q-card-section>
                    <div class="text-h6">Abrir Turno</div>
                </q-card-section>
                <q-card-section>
                    <q-select v-model="selectedRegisterId" :options="registers" option-label="name" option-value="id" emit-value map-options label="Caja" outlined dense />
                    <q-input v-model.number="startingCash" type="number" label="Monto Inicial" prefix="BOB" outlined dense class="q-mt-sm" />
                </q-card-section>
                <q-card-actions align="right">
                    <q-btn flat label="Cancelar" v-close-popup />
                    <q-btn color="primary" label="Abrir" @click="handleOpen" :loading="loading" />
                </q-card-actions>
            </q-card>
        </q-dialog>

        <!-- Diálogo Comprobante de Arqueo -->
        <q-dialog v-model="showReceiptDialog" persistent>
            <q-card style="min-width: 400px; max-width: 480px">
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
    </q-card>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useCashShiftStore } from '@/stores/cashShiftStore';
import { cashShiftService } from '@/services/cashShift.service';
import { useQuasar } from 'quasar';
import { date } from 'quasar';

const $q = useQuasar();
const store = useCashShiftStore();
const showCloseDialog = ref(false);
const showOpenDialog = ref(false);
const showReceiptDialog = ref(false);
const receiptText = ref('');
const loading = computed(() => store.loading);

const endingCashActual = ref(0);
const closingNotes = ref('');
const startingCash = ref(0);
const selectedRegisterId = ref<number | null>(null);

const activeShift = computed(() => store.activeShift);
const registers = computed(() => store.registers);
const registerName = computed(() => activeShift.value?.registerName || 'No asignada');

const formatDate = (val: string) => date.formatDate(val, 'DD/MM/YYYY HH:mm');

const openDialog = () => {
    if (registers.value.length > 0) {
        selectedRegisterId.value = registers.value[0].id;
    }
    showOpenDialog.value = true;
};

const handleClose = async () => {
    if (!activeShift.value) return;
    const shiftId = activeShift.value.shiftId;
    try {
        await store.closeShift(shiftId, endingCashActual.value, closingNotes.value);
        $q.notify({ color: 'positive', message: 'Caja cerrada exitosamente' });
        showCloseDialog.value = false;

        try {
            const res = await cashShiftService.getReceiptPayload(shiftId);
            receiptText.value = res.data.formattedText;
            showReceiptDialog.value = true;
        } catch (err) {
            $q.notify({ color: 'warning', message: 'Caja cerrada, pero no se pudo generar el comprobante' });
        }
    } catch (e) {
        $q.notify({ color: 'negative', message: 'Error al cerrar' });
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

const handleOpen = async () => {
    if (!selectedRegisterId.value) return;
    try {
        await store.openShift(selectedRegisterId.value, startingCash.value);
        $q.notify({ color: 'positive', message: 'Caja abierta exitosamente' });
        showOpenDialog.value = false;
    } catch (e) {
        $q.notify({ color: 'negative', message: 'Error al abrir' });
    }
};

onMounted(() => {
    store.fetchActiveShift();
    store.fetchRegisters();
});
</script>

<style scoped>
</style>
