<template>
  <q-table v-bind="$attrs" :loading="loading" :pagination="pagination" @request="onRequest">
    <template v-for="(_, name) in $slots" #[name]="slotData">
      <slot :name="name" v-bind="slotData" />
    </template>
  </q-table>
</template>

<script setup lang="ts">
defineProps(['loading', 'pagination']);
const emit = defineEmits(['request']);

function onRequest(props: any) {
  emit('request', props);
}
</script>
