<template>
    <div class="row q-col-gutter-sm q-mb-sm">
        <div class="col-12 col-md-6">
            <q-input
                v-model="searchModel"
                placeholder="Buscar producto o Código (F2)..."
                outlined
                dense
                bg-color="transparent"
                class="pos-search-input"
                @keyup.enter="$emit('scan')"
            >
                <template v-slot:prepend>
                    <q-icon name="search" />
                </template>
                <template v-slot:append>
                    <q-icon
                        name="qr_code_scanner"
                        class="cursor-pointer"
                        @click="$emit('open-scanner')"
                    />
                </template>
            </q-input>
        </div>

        <div class="col-12 col-md-6">
            <q-select
                v-model="categoryModel"
                :options="categories"
                option-label="name"
                option-value="id"
                label="Categoría"
                outlined
                dense
                bg-color="transparent"
                clearable
                emit-value
                map-options
            />
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { Category } from '@/types';

const props = defineProps<{
    search: string;
    selectedCategory: any;
    categories: Category[];
}>();

const emit = defineEmits(['update:search', 'update:selectedCategory', 'scan', 'open-scanner']);

const searchModel = computed({
    get: () => props.search,
    set: (val) => emit('update:search', val)
});

const categoryModel = computed({
    get: () => props.selectedCategory,
    set: (val) => emit('update:selectedCategory', val)
});
</script>

<style lang="scss" scoped>
.pos-search-input {
    :deep(.q-field__control) {
        background-color: var(--color-background-elevated) !important;
        border-color: var(--color-border) !important;
        .body--dark & {
            background-color: var(--color-background-elevated-dark) !important;
            border-color: var(--color-border-dark) !important;
        }
    }
    :deep(.q-field__marginal) {
        color: var(--color-text-primary) !important;
    }
}
</style>
