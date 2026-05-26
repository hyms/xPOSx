<template>
  <q-btn flat round :color="activeShift ? 'positive' : 'negative'" icon="point_of_sale">
    <q-tooltip>{{ activeShift ? 'Caja Abierta' : 'Caja Cerrada' }}</q-tooltip>
    <q-menu>
      <q-list style="min-width: 200px">
        <q-item v-if="activeShift">
          <q-item-section>
            <q-item-label>Caja Abierta por {{ activeShift.username }}</q-item-label>
            <q-item-label caption>Monto Inicial: Bs. {{ activeShift.startingCash }}</q-item-label>
          </q-item-section>
        </q-item>
        <q-item v-else clickable @click="showOpenDialog = true">
          <q-item-section>
            <q-item-label>Caja Cerrada</q-item-label>
            <q-item-label caption>Haz clic para abrir</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-menu>

    <q-dialog v-model="showOpenDialog">
      <q-card style="min-width: 300px">
        <q-card-section>
          <div class="text-h6">Abrir Caja</div>
        </q-card-section>
        <q-card-section>
          <q-input v-model.number="startingCash" type="number" label="Monto Inicial" prefix="BOB" outlined dense />
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat label="Cancelar" v-close-popup />
          <q-btn color="primary" label="Abrir" @click="handleOpen" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-btn>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useCashShiftStore } from '@/stores/cashShiftStore';

const store = useCashShiftStore();
const activeShift = computed(() => store.activeShift);
const showOpenDialog = ref(false);
const startingCash = ref(0);

const handleOpen = async () => {
    // Logic to select register not fully implemented here for brevity, assumes default
    await store.openShift(store.registers[0]?.id || 1, startingCash.value);
    showOpenDialog.value = false;
};
</script>
