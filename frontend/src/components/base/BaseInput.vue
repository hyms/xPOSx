<template>
    <q-input v-bind="$attrs" outlined dense hide-bottom-space>
        <template v-for="(_, name) in $slots" v-slot:[name]="slotData">
            <slot :name="name" v-bind="slotData || {}" />
        </template>
    </q-input>
</template>
