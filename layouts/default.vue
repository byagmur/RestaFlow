<script setup>
import { onMounted, ref } from 'vue'
import { useAuthStore } from '~/stores/auth/authStore'

const pageTitle = ref('')
const isLoading = ref(true)
const authStore = useAuthStore()

async function loadUser() {
  isLoading.value = true
  await authStore.fetchUserInfo?.() // fetchUserInfo fonksiyonun varsa çağır
  isLoading.value = false
}

onMounted(() => {
  loadUser()
})
</script>

<template>
  <div v-if="!isLoading" class="min-h-screen bg-gray-50 flex">
    <div
      class="!sticky top-0 w-24 bg-white/90 backdrop-blur-sm shadow-md flex flex-col py-6 h-screen border-r border-gray-200"
    >
      <nav class="flex flex-col justify-between flex-grow space-y-6">
        <div class="flex justify-center mb-4">
          <img src="/public/img/restaFlowIcon.png" alt="logo" class="w-24 h-24 p-2 rounded-xl">
        </div>

        <div class="flex flex-col space-y-6 opacity-0 animate-fade-in p-2" style="animation-delay: 0.1s">
          <menu_item to="/" title="Anasayfa" icon="heroicons:home-20-solid" />
          <menu_item to="/order/add_order" title="Sipariş Oluştur" icon="heroicons:clipboard-document-list-20-solid" />
          <!-- <menu_item to="/order/orders" title="Siparişleri Yönet" icon="mingcute:edit-4-line" /> -->
          <!-- <menu_item to="/dashboard" title="Dashboard" icon="heroicons:chart-bar-20-solid" /> -->
          <!-- <menu_item to="/settings" title="Ayarlar" icon="heroicons:cog-6-tooth-20-solid" /> -->
        </div>

        <div class="mt-auto opacity-0 animate-fade-in" style="animation-delay: 0.2s">
          <UButton
            variant="none" to="/login"
            class="p-2 rounded-xl hover:bg-gray-100 mx-auto flex justify-center transition-colors duration-200"
            @click="authStore.logout()"
          >
            <Icon name="heroicons:arrow-right-on-rectangle-20-solid" class="!w-7 !h-7 text-gray-600" />
          </UButton>
        </div>
      </nav>
    </div>

    <div class="flex-1 p-8 inter-tight">
      <div class="flex justify-between items-center mb-8 opacity-0 animate-fade-in" style="animation-delay: 0.3s">
        <h2
          :ui="{ base: '!text-left' }"
          class="flex mt-8 ml-4 text-2xl font-bold text-gray-800 mb-6 opacity-0 animate-fade-in justify-start  "
          style="animation-delay: 0.2s"
        >
          {{ 'RestaFlow' }} {{ authStore.userInfo.role === 'Yönetici' ? 'Yönetici Paneline' : 'Garson Paneline' }}
          Hoşgeldiniz!
        </h2>
        <h1 class="text-3xl font-bold text-gray-800">
          {{ pageTitle }}
        </h1>

        <!-- Garson Profil -->
        <div class="flex items-center space-x-4 bg-white shadow-sm p-3 rounded-xl border border-gray-200">
          <div class="text-right">
            <p v-if="authStore.userInfo" class="font-semibold text-gray-800">
              {{ authStore.userInfo.firstName || '' }} {{ }} {{ authStore.userInfo.lastName || '' }}
            </p>
            <p class="text-sm text-gray-500">
              {{ authStore.userInfo.role }}
            </p>
          </div>
          <div class="relative">
            <div class="w-10 h-10 bg-blue-50 rounded-xl flex items-center justify-center">
              <Icon name="mingcute:user-3-fill" size="25" class=" text-blue-600" />
            </div>
            <span class="absolute bottom-0 right-0 h-3 w-3 bg-emerald-500 rounded-full border-2 border-white" />
          </div>
        </div>
      </div>

      <!-- Sayfa İçeriği -->
      <slot name="content" />
    </div>
  </div>
  <div v-else class="flex items-center justify-center min-h-screen pb-20 flex-col">
    <Loader />
    <p class="text-center mt-5 ml-4 text-gray-500">
      Kullanıcı verileri yükleniyor...
    </p>
  </div>
</template>

<style>
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.animate-fade-in {
  animation: fadeIn 0.5s ease-out forwards;
}

::-webkit-scrollbar {
  height: 6px;
  width: 6px;
}

::-webkit-scrollbar-track {
  background: rgba(230, 230, 230, 0.5);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb {
  background: rgba(180, 180, 180, 0.5);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: rgba(150, 150, 150, 0.7);
}
</style>
