<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '../stores/authStore'

const authStore = useAuthStore()
const router = useRouter()
const isRedirecting = ref(false)

const Loader = defineAsyncComponent(() => import('~/components/ui/loader.vue'))

definePageMeta({
  layoutTransition: {
    name: 'fade',
    mode: 'out-in',
  },
  layout: 'login',
})

async function onSubmit(event: Event) {
  event.preventDefault?.()
  await authStore.login()
  if (authStore.isAuthenticated) {
    isRedirecting.value = true
    await router.replace('/')
    isRedirecting.value = false
  }
  else {
    // alert('Giriş başarısız! Lütfen kullanıcı adı ve şifrenizi kontrol edin.')
  }
}
</script>

<template>
  <div v-if="authStore.isLoading || isRedirecting" class="flex items-center justify-center h-screen">
    <Loader />
  </div>
  <div v-else class="min-h-screen flex items-center justify-center bg-gray-50">
    <div class="w-full max-w-6xl h-[600px] rounded-xl shadow-xl flex overflow-hidden">
      <!-- Sol Panel - Dekoratif Alan -->
      <div class="w-1/2 bg-gray-50 relative overflow-hidden">
        <NuxtImg
          src="../public/img/baseImg.png"
          alt="RestaFlow Dekoratif Görseli"
          fetchpriority="high"
          width="640"
          height="400"
          preload
        />
        <div class="absolute bottom-10 w-full text-center">
          <h3 class="text-lg font-medium text-gray-600">
            RestaFlow ile Lezzet ve Verimlilik Bir Arada
          </h3>
        </div>
      </div>

      <!-- Sağ Panel - Giriş Formu -->
      <div class="w-1/2 bg-gray-50 p-10 flex flex-col">
        <div class="text-center mb-8">
          <h1 class="text-3xl font-bold text-blue-700 mb-2">
            Hoşgeldiniz
          </h1>
          <p class="text-gray-600">
            Restoran yönetim sistemine giriş yapın
          </p>
        </div>

        <UForm
          :state="authStore.form"
          class="space-y-6 w-full max-w-md mx-auto mt-12"
          @submit="onSubmit"
        >
          <div>
            <UInput
              v-model="authStore.form.username"
              type="text"
              placeholder="Kullanıcı adınızı girin"
              variant="none"
              class="!custom-input  w-full border-b !border-gray-300 transition-colors py-2 !text-gray-950 !text-lg"
              required
            />
          </div>

          <div>
            <UInput
              v-model="authStore.form.password"
              type="password"
              placeholder="Şifrenizi girin"
              variant="none"
              class="!custom-input w-full border-b !border-gray-300 transition-colors py-2 !text-gray-950"
              required
            />
          </div>

          <div>
            <UButton
              type="submit"
              variant="solid"
              size="lg"
              block
              class="h-12 !text-white bg-blue-500 hover:bg-blue-600 transition-all rounded-md mt-7"
              :loading="authStore.isLoading"
            >
              GİRİŞ YAP
            </UButton>
          </div>

          <div class="mt-4 text-center">
            <p class="text-sm text-gray-600">
              Hesabınız yok mu?
              <NuxtLink to="/register" class="text-blue-600 hover:text-blue-800 ml-1">
                Kayıt Ol
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
          © 2025 RestaFlow Ltd. Şti. Tüm hakları saklıdır.
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
.hover\:text-blue-800:hover {
  color: #3a6d8c;
}
.custom-input {
  background-color: rgba(238, 238, 238, 0.7) !important;
  border-bottom:
    2px solid,
    #838383 !important;
  color: #e5e5e5 !important;
  border-radius: 4px 4px 0 0 !important;
}

.custom-input::placeholder {
  color: #ababab !important;
}
</style>
