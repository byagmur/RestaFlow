<script setup lang="ts">
import type { Order } from '@/types'
import { computed, onMounted, ref, watch } from 'vue'
import WeeklyOrderChart from '@/components/charts/WeeklyOrderChart.vue'
import OrderDetailCard from '@/components/order/detailCard.vue'
import { useOrderStore } from '@/stores/orderStore'
import TotalEarningsProgress from '~/components/charts/TotalEarningsProgress.vue'
import BasePagination from '~/components/ui/BasePagination.vue'
import { useAuthStore } from '~/stores/authStore'

const page = ref<number>(1)
const pageSize = ref(5)
const authStore = useAuthStore()
const orderStore = useOrderStore()
const Loader = defineAsyncComponent(() => import('~/components/ui/loader.vue'))
const weeklyTarget = ref(10000)

// Filtre state'leri
const currentFilters = ref({
  orderNo: '',
  tableId: '',
  status: '',
  todayOnly: false
})

// Bugünün tarihini kontrol eden fonksiyon
function isToday(dateString: string): boolean {
  if (!dateString) return false
  
  const orderDate = new Date(dateString)
  const today = new Date()
  
  return orderDate.getFullYear() === today.getFullYear() &&
         orderDate.getMonth() === today.getMonth() &&
         orderDate.getDate() === today.getDate()
}

// Filtrelenmiş siparişler (tüm backend verisi üzerinde)
const filteredOrders = computed(() => {
  if (!orderStore.pagedOrders || !Array.isArray(orderStore.pagedOrders)) {
    return []
  }

  return orderStore.pagedOrders.filter((order) => {
    if (!order) return false

    // Sipariş no filtresi
    const matchesOrderNo = currentFilters.value.orderNo
      ? String(order.uid || '').toLowerCase().includes(currentFilters.value.orderNo.toLowerCase())
      : true
    
    // Masa no filtresi
    const matchesTableId = currentFilters.value.tableId
      ? String(order.tableId || '').includes(currentFilters.value.tableId)
      : true
    
    // Durum filtresi
    const matchesStatus = currentFilters.value.status
      ? order.status === currentFilters.value.status
      : true
    
    // Bugün filtresi
    const matchesToday = currentFilters.value.todayOnly
      ? isToday(order.orderDate)
      : true

    return matchesOrderNo && matchesTableId && matchesStatus && matchesToday
  })
})

// Sayfalı siparişler (client-side pagination)
const pagedFilteredOrders = computed(() => {
  const start = (page.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredOrders.value.slice(start, end)
})

// Toplam filtrelenmiş kayıt sayısı
const totalFilteredOrders = computed(() => filteredOrders.value.length)

async function loadOrders() {
  const params: any = { start: 0, limit: 50 } // Backend'den 50 kayıt çek
  
  if (authStore.userInfo.role === 'Garson') {
    params.waiterID = authStore.userInfo.id
  }
  
  await orderStore.fetchLastOrdersPerTable(params)
}

onMounted(async () => {
  authStore.loadUserInfo()
  const fetchParams: any = {}
  if (authStore.userInfo.role === 'Garson') {
    fetchParams.waiterID = authStore.userInfo.id
  }
  await orderStore.fetchOrders(fetchParams)
  await loadOrders()
})

// Filtre değiştiğinde sadece sayfa 1'e dön (veri yeniden çekilmez)
function handleFilterChange(filters: any) {
  currentFilters.value = filters
  page.value = 1 // Filtreleme sonrası 1. sayfaya dön
  // console.log('Filtre uygulandı:', filters)
  // console.log('Filtrelenmiş sipariş sayısı:', filteredOrders.value.length)
}

// Sayfa değiştiğinde veri yeniden çekilmez, sadece client-side pagination
watch(page, () => {
  // console.log('Sayfa değişti:', page.value)
  // console.log('Gösterilen siparişler:', pagedFilteredOrders.value.length)
})

definePageMeta({
  middleware: 'auth',
  ssr: false,
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

// console.log('Backend\'den gelen siparişler:', orderStore.pagedOrders?.length)
// console.log('Filtrelenmiş siparişler:', filteredOrders.value?.length)
// console.log('Sayfalı siparişler:', pagedFilteredOrders.value?.length)
// console.log('Toplam filtrelenmiş:', totalFilteredOrders.value)
</script>

<template defer="true">
  <NuxtLayout>
    <template #content>
      <div v-if="isUserLoaded" class="space-y-6 px-4 sm:px-0">
        <div class="grid grid-cols-1 lg:grid-cols-5 gap-6 w-full">
          <!-- Sol taraf: Siparişler (4/5 genişlik) -->
          <div class="lg:col-span-3 flex flex-col w-full h-full">
            <RecentOrdersCard
              :orders="pagedFilteredOrders"
              :user-role="authStore.userInfo.role"
              @order-click="openOrderDetail"
              @filter-change="handleFilterChange"
            />
            <BasePagination
              v-if="totalFilteredOrders > pageSize"
              v-model="page"
              :total="totalFilteredOrders"
              :page-size="pageSize"
            />
            <p v-else class="text-gray-500 text-sm mt-2 text-center">
              {{ totalFilteredOrders }} sipariş bulundu ({{ pageSize }}'den az)
            </p>
          </div>

          <!-- Sağ taraf: Grafikler (1/5 genişlik) -->
          <div class="lg:col-span-2 flex flex-col gap-6 w-full h-full">
            <div
              class="bg-white shadow-md rounded-2xl p-4 sm:p-6 opacity-0 animate-fade-in w-full"
              style="animation-delay: 0.5s"
            >
              <h2 class="text-lg sm:text-lg font-bold text-gray-800 mb-4 sm:mb-6">
                Haftalık Sipariş Sayısı
              </h2>
              <WeeklyOrderChart :orders="orderStore.orders" />
            </div>
            <TotalEarningsProgress
              :orders="orderStore.orders" 
              :weekly-target="weeklyTarget"
              class="w-full" 
              @update:weekly-target="weeklyTarget = $event"
            />
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
