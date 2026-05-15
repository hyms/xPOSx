<template>
    <q-page padding>
        <div class="row q-col-gutter-sm justify-center">
            <div class="col-12 col-md-8">
                <q-card>
                    <q-card-section class="bg-primary text-white">
                        <div class="text-h6">Mi Perfil</div>
                    </q-card-section>

                    <q-card-section class="q-pa-md">
                        <q-form @submit="onUpdate" class="q-gutter-md">
                            <div class="row q-col-gutter-sm">
                                <div class="col-12 col-md-6">
                                    <BaseInput
                                        v-model="userData.username"
                                        label="Usuario"
                                        readonly
                                    />
                                </div>
                                <div class="col-12 col-md-6">
                                    <BaseInput
                                        v-model="userData.email"
                                        label="Email"
                                        lazy-rules
                                        :rules="[rules.required, rules.email]"
                                    />
                                </div>
                                <div class="col-12 col-md-6">
                                    <BaseInput
                                        v-model="userData.firstName"
                                        label="Nombre"
                                        lazy-rules
                                        :rules="[rules.required]"
                                    />
                                </div>
                                <div class="col-12 col-md-6">
                                    <BaseInput
                                        v-model="userData.lastName"
                                        label="Apellido"
                                    />
                                </div>
                                <div class="col-12 col-md-6">
                                    <BaseInput
                                        v-model="userData.newPassword"
                                        label="Nueva Contraseña"
                                        type="password"
                                        lazy-rules
                                        :rules="[rules.minLength(6)]"
                                    />
                                </div>
                            </div>

                            <div
                                v-if="errorMessage"
                                class="q-pa-md bg-negative text-white rounded-borders"
                            >
                                {{ errorMessage }}
                            </div>

                            <div class="row justify-end q-mt-md">
                                <q-btn
                                    label="Actualizar Perfil"
                                    type="submit"
                                    color="primary"
                                    :loading="loading"
                                />
                            </div>
                        </q-form>
                    </q-card-section>
                </q-card>
            </div>
        </div>
    </q-page>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from "vue";
import { useQuasar } from "quasar";
import { userService } from "@/services/user.service";
import type { User } from "@/types";
import { useAuthStore } from "@/stores/auth";
import BaseInput from "@/components/base/BaseInput.vue";
import { rules } from "@/utils/validations";

const $q = useQuasar();
const authStore = useAuthStore();
const loading = ref(false);
const errorMessage = ref("");

const userData = reactive<User & { newPassword?: string }>({
    username: "",
    email: "",
    firstName: "",
    lastName: "",
    role: 0,
});

const fetchProfile = async () => {
    try {
        const response = await userService.getProfile();
        Object.assign(userData, response.data);
    } catch (error: any) {
        errorMessage.value = "Error al cargar el perfil";
    }
};

const onUpdate = async () => {
    loading.value = true;
    errorMessage.value = "";
    try {
        const data: any = {
            email: userData.email,
            firstName: userData.firstName,
            lastName: userData.lastName,
        };
        if (userData.newPassword) {
            data.newPassword = userData.newPassword;
        }
        await userService.updateProfile(data);
        $q.notify({
            color: "positive",
            message: "Perfil actualizado con éxito",
            icon: "check_circle",
        });
        userData.newPassword = "";
        if (userData.username) authStore.username = userData.username;
    } catch (error: any) {
        errorMessage.value =
            error.response?.data?.message ||
            error.response?.data ||
            "Error al actualizar el perfil";
    } finally {
        loading.value = false;
    }
};

onMounted(fetchProfile);
</script>
