<template>
  <q-dialog v-model="showDialog" persistent :full-width="fullWidth">
    <q-card :style="cardStyle">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">{{ title }}</div>
        <q-space />
        <q-btn v-if="showClose" icon="close" flat round dense v-close-popup />
      </q-card-section>

      <q-card-section :class="contentClass">
        <q-form @submit="$emit('submit')" class="q-gutter-md">
          <slot />

          <div class="row justify-end q-mt-md">
            <q-btn label="Cancelar" color="primary" flat v-close-popup />
            <q-btn :label="isEdit ? 'Actualizar' : 'Guardar'" color="primary" type="submit" :loading="saving" icon="save" />
          </div>
        </q-form>
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps({
  modelValue: Boolean,
  title: String,
  minWidth: {
    type: String,
    default: '400px'
  },
  fullWidth: Boolean,
  contentClass: {
    type: String,
    default: 'q-pt-none q-pa-md'
  },
  showClose: {
    type: Boolean,
    default: true
  },
  isEdit: Boolean,
  saving: Boolean
})

const emit = defineEmits(['update:modelValue', 'submit'])

const showDialog = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
})

const cardStyle = computed(() => {
  if (props.fullWidth) return ''
  return `min-width: ${props.minWidth}`
})
</script>