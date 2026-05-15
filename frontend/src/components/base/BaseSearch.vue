<template>
    <q-input
        v-model="model"
        outlined
        dense
        placeholder="Buscar..."
        debounce="300"
        @update:model-value="$emit('search', $event)"
    >
        <template #append>
            <q-icon name="search" />
        </template>
    </q-input>
</template>

<script setup lang="ts">
import { ref } from "vue";
const model = ref("");
defineEmits(["search"]);
</script>
