<script setup lang="ts">
import type { Order } from '@/types'
import { computed, onMounted, ref, watch } from 'vue'
import WeeklyOrderChart from '@/components/charts/WeeklyOrderChart.vue'
import { useOrderStore } from '@/stores/orderStore'
import TotalEarningsProgress from '~/components/charts/TotalEarningsProgress.vue'
import { useAuthStore } from '~/stores/authStore'

const page = ref(1)
const pageSize = 7

const authStore = useAuthStore()
const orderStore = useOrderStore()
const Loader = defineAsyncComponent(() => import('~/components/ui/loader.vue'))
const weeklyTarget = 10000

const totalOrders = ref(0) // Toplam sipariş sayısı (backend'den dönmeli!)

const start = computed(() => (page.value - 1) * pageSize)

async function loadOrders() {
  const result = await orderStore.fetchOrders({ start: start.value, limit: pageSize })
  if (result && typeof result.totalCount === 'number') {
    totalOrders.value = result.totalCount
  }
}

onMounted(async () => {
  authStore.loadUserInfo()
  loadOrders()
})

watch(page, async () => {
  await loadOrders()
})

definePageMeta({
  middleware: 'auth',
  ssr: false, // Sadece istemci tarafında render edilir
})

// const activeOrders = computed<Order[]>(() =>
//   (orderStore.orders as Order[])
//     .slice()
//     .sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime()),
// )

// Kullanıcının rolüne göre filtrele
const filteredOrders = computed<Order[]>(() => {
  const allOrders = (orderStore.orders as Order[])
    .slice()
    .sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime())

  // Yönetici ise tüm siparişler, garson ise sadece kendi siparişleri
  if (authStore.userInfo.role === 'Yönetici') {
    return allOrders
  }
  else if (authStore.userInfo.role === 'Garson') {
    return allOrders.filter(order => order.waiterId === authStore.userInfo.id)
  }
  return []
})

const isUserLoaded = computed(() => !!authStore.userInfo.role)

console.log('userInfo:', authStore.userInfo)
console.log('filteredOrders:', filteredOrders.value)
</script>

<template defer="true">
  <NuxtLayout>
    <template #content>
      <div v-if="isUserLoaded" class="space-y-6 px-4 sm:px-0">
        <h2
          class="text-2xl text-left font-bold p-5 text-gray-800 transition-all duration-500 ease-in-out opacity-0 animate-fade-in"
          style="animation-delay: 0.2s"
        >
          {{ 'RestaFlow' }} {{ authStore.userInfo.role === 'Yönetici' ? 'Yönetici Paneline' : 'Garson Paneline' }}
          Hoşgeldiniz!
        </h2>
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 w-full">
          <!-- Son Siparişler: 1. sütunun tamamını kaplar -->
          <div class="flex flex-col w-full h-full">
            <RecentOrdersCard
              :orders="filteredOrders"
              :total-orders="totalOrders"
              :page="page"
              :page-size="pageSize"
              @update:page="val => page = val"
            />
          </div>
          <!-- Sağda: Haftalık Sipariş Sayısı ve Toplam Kazanç alt alta -->
          <div class="flex flex-col gap-6 w-full h-full">
            <div class="bg-white shadow-md rounded-2xl p-4 sm:p-6 opacity-0 animate-fade-in w-full" style="animation-delay: 0.5s">
              <h2 class="text-lg sm:text-lg font-bold text-gray-800 mb-4 sm:mb-6">
                Haftalık Sipariş Sayısı
              </h2>
              <WeeklyOrderChart :orders="filteredOrders" />
            </div>
            <TotalEarningsProgress :orders="filteredOrders" :weekly-target="weeklyTarget" class="w-full" />
          </div>
        </div>
      </div>
      <div v-else class="flex items-center justify-center min-h-screen pb-60 flex-col">
        <Loader />
        <p class="text-center mt-5 ml-4 text-gray-500">
          Kullanıcı verileri yükleniyor...
        </p>
      </div>
    </template>
  </NuxtLayout>
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
