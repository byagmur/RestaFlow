<script setup lang="ts">
import { useAuthStore } from '~/stores/auth/authStore'

definePageMeta({
  middleware: ['auth'],
})

const authStore = useAuthStore()

async function onSubmit() {
  await authStore.register()
  const success = authStore.successMessage || authStore.errorMessage
  if (success) {
    alert('Kayıt başarılı! Lütfen giriş yapın.')
  }
  else {
    alert('Kayıt başarısız! Lütfen bilgilerinizi kontrol edin.')
  }
}

watch(() => authStore.errorMessage, (val) => {
  if (val) { /* empty */ }
})

watch(() => authStore.successMessage, (val) => {
  if (val) {
    alert('Kayıt başarılı! Lütfen giriş yapın.')
  }
})
</script>

<template>
  <div v-if="authStore.isLoading" class="flex items-center justify-center h-screen">
    <Loader />
  </div>
  <div v-else class="min-h-screen flex items-center justify-center bg-gray-50">
    <div class="w-full max-w-6xl h-[650px] rounded-xl shadow-xl flex overflow-hidden">
      <!-- Sol Panel - Dekoratif Alan -->
      <div class="w-1/2 bg-gray-50 relative overflow-hidden">
        <NuxtImg
          src="../public/img/baseImg.png"
          alt="RestaFlow Dekoratif Görseli"
          class="pt-20"
          width="640"
          height="400"
          loading="lazy"
        />
        <div class="absolute bottom-10 w-full text-center">
          <h3 class="text-lg font-medium text-gray-600">
            <!-- Lezzet ve Verimlilik Bir Arada -->
          </h3>
        </div>
      </div>

      <!-- Sağ Panel - Kayıt Formu -->
      <div class="w-1/2 bg-gray-50 p-10 flex flex-col">
        <div class="text-center mb-6">
          <h1 class="text-3xl font-bold text-blue-700 mb-2">
            Hesap Oluştur
          </h1>
          <p class="text-gray-600">
            RestaFlow Restoran yönetim sistemine kayıt olun
          </p>
        </div>

        <UForm
          :state="authStore.form"
          class="space-y-4 w-full max-w-md mx-auto"
          @submit="onSubmit"
        >
          <div>
            <label class="block text-gray-600 text-sm mb-1">Kullanıcı Adı</label>
            <UInput
              v-model="authStore.form.username"
              type="text"
              placeholder="Kullanıcı adınızı girin"
              variant="none"
              class="!custom-input w-full border-b !text-gray-800 !border-gray-300 transition-colors py-2"
              required
            />
          </div>

          <div>
            <label class="block text-gray-600 text-sm mb-1">E-posta</label>
            <UInput
              v-model="authStore.form.email"
              type="email"
              placeholder="Email adresinizi girin"
              variant="none"
              class="!custom-input w-full border-b !text-gray-800 !border-gray-300 transition-colors py-2"
              required
            />
          </div>

          <div>
            <label class="block text-gray-600 text-sm mb-1">Şifre</label>
            <UInput
              v-model="authStore.form.password"
              type="password"
              placeholder="Şifrenizi girin"
              variant="none"
              class="!custom-input w-full border-b !text-gray-800 !border-gray-300 transition-colors py-2 "
              required
            />
          </div>

          <div>
            <UInput
              v-model="authStore.form.passwordConfirm"
              type="password"
              placeholder="Şifrenizi tekrar girin"
              variant="none"
              class="!custom-input w-full border-b !text-gray-800 !border-gray-300 transition-colors py-2 "
              required
            />
          </div>

          <div class="mt-6">
            <UButton
              type="submit"
              variant="solid"
              size="lg"
              block
              class="h-12 !text-white bg-blue-600 hover:bg-blue-700 transition-all rounded-md"
              :loading="authStore.isLoading"
            >
              KAYIT OL
            </UButton>
          </div>

          <div class="mt-4 text-center">
            <p class="text-sm text-gray-600">
              Zaten hesabınız var mı?
              <NuxtLink to="/login" class="text-blue-600 hover:text-blue-800 ml-1">
                Giriş Yap
              </NuxtLink>
            </p>
          </div>
        </UForm>

        <div class="mt-auto flex items-center justify-center">
          <div class="h-px bg-gray-300 w-1/4" />
          <div class="mx-4 text-gray-500 text-xs" />
          <div class="h-px bg-gray-300 w-1/4" />
        </div>

        <div class="text-center text-xs text-gray-500 mt-6">
          © 2025  RestaFlow Ltd. Şti. Tüm hakları saklıdır.
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.bg-blue-700 {
  background-color: steelblue;
}
.hover\:bg-blue-800:hover {
  background-color: #3a6d8c;
}
.text-blue-700 {
  color: steelblue;
}
.text-blue-600 {
  color: #4991c2;
}
.hover\:text-blue-800:hover {
  color: #3a6d8c;
}

.custom-input {
  background-color: rgba(238, 238, 238, 0.7) !important;
  border-bottom: 2px solid #838383 !important;
  color: #333 !important;
  border-radius: 4px 4px 0 0 !important;
}

.custom-input:focus {
  border-bottom: 2px solid #4991c2 !important;
  box-shadow: 0 1px 0 0 rgba(73, 145, 194, 0.5) !important;
}

.custom-input::placeholder {
  color: #777 !important;
}
</style>
