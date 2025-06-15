<script setup lang="ts">
import type { Order } from '@/types'
import { computed, onMounted, ref, watch } from 'vue'
import WeeklyOrderChart from '@/components/charts/WeeklyOrderChart.vue'
import { useOrderStore } from '@/stores/orderStore'
import TotalEarningsProgress from '~/components/charts/TotalEarningsProgress.vue'
import { useAuthStore } from '~/stores/authStore'
import OrderDetailCard from '@/components/order/detailCard.vue'

const page = ref(1)
const pageSize = 7

const authStore = useAuthStore()
const orderStore = useOrderStore()
const Loader = defineAsyncComponent(() => import('~/components/ui/loader.vue'))
const weeklyTarget = ref(10000)

const totalOrders = ref(0) // Toplam sipariş sayısı (backend'den dönmeli!)

const start = computed(() => (page.value - 1) * pageSize)

async function loadOrders() {
  const params: any = { start: start.value, limit: pageSize }
  if (authStore.userInfo.role === 'Garson') {
    params.waiterID = Number(authStore.userInfo.id)
  }
  const result = await orderStore.fetchOrders(params)
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
  return (orderStore.orders as Order[])
    .slice()
    .sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime())
})

const isUserLoaded = computed(() => !!authStore.userInfo.role)

const showOrderDetail = ref(false)
const selectedOrder = ref<any>(null)
const isOrderDetailLoading = ref(false)

async function openOrderDetail(order: any) {
  isOrderDetailLoading.value = true
  showOrderDetail.value = true
  await orderStore.fetchOrderDetail(order.uid)
  selectedOrder.value = { ...orderStore.order }
  isOrderDetailLoading.value = false
}
function closeOrderDetail() {
  showOrderDetail.value = false
  selectedOrder.value = null
}

console.log('garson id', authStore.userInfo.id)
console.log('filtrelenmiş siparişler---:', filteredOrders.value)
</script>

<template defer="true">
  <NuxtLayout>
    <template #content>
      <div v-if="isUserLoaded" class="space-y-6 px-4 sm:px-0">

        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 w-full">

          <div class="flex flex-col w-full h-full">
            <RecentOrdersCard :orders="filteredOrders" :total-orders="totalOrders" :page="page" :page-size="pageSize"
              @update:page="val => page = val" @order-click="openOrderDetail" />
          </div>

          <div class="flex flex-col gap-6 w-full h-full">
            <div class="bg-white shadow-md rounded-2xl p-4 sm:p-6 opacity-0 animate-fade-in w-full"
              style="animation-delay: 0.5s">
              <h2 class="text-lg sm:text-lg font-bold text-gray-800 mb-4 sm:mb-6">
                Haftalık Sipariş Sayısı
              </h2>
              <WeeklyOrderChart :orders="filteredOrders" />
            </div>
            <TotalEarningsProgress :orders="filteredOrders" :weekly-target="weeklyTarget"
              @update:weeklyTarget="weeklyTarget = $event" class="w-full" />
          </div>
        </div>
      </div>
      <div v-else class="flex items-center justify-center min-h-screen pb-60 flex-col">
        <Loader />
        <p class="text-center mt-5 ml-4 text-gray-500">
          Kullanıcı verileri yükleniyor...
        </p>
      </div>
      <OrderDetailCard
        v-if="showOrderDetail && selectedOrder"
        :order="selectedOrder"
        @close="closeOrderDetail"
      />
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
