<script setup lang="ts">
import { computed, defineEmits, defineProps, ref, watch } from 'vue'
import OrderStatusFilter from '@/components/ui/filter/orderStatusFilter.vue'
import CustomInput from '@/components/ui/input/index.vue'

const props = defineProps<{
  orders: any[]
  userRole?: string
}>()

const emit = defineEmits(['order-click', 'filter-change'])

const filterOrderNo = ref('')
const filterTableId = ref('')
const filterStatus = ref('')
const showTodayOnly = ref(false) // Yeni: bugünkü siparişler filtresi

// Admin kontrolü
const isAdmin = computed(() => props.userRole === 'Yönetici')

function getOrderStatusClass(status: string) {
  switch (status) {
    case 'Hazırlanıyor':
      return 'bg-blue-50 text-blue-600'
    case 'Beklemede':
      return 'bg-amber-50 text-amber-600'
    case 'Servis Edildi':
      return 'bg-orange-50 text-orange-600'
    case 'İptal Edildi':
      return 'bg-red-50 text-red-600'
    case 'Tamamlandı':
      return 'bg-yellow-50 text-yellow-600'
    case 'Ödendi':
      return 'bg-purple-50 text-purple-600'
    case 'Açıldı':
      return 'bg-green-50 text-green-600'
    default:
      return 'bg-slate-50 text-slate-600'
  }
}

function openOrderDetail(order: any) {
  emit('order-click', order)
}

// Bugünün tarihini kontrol etmek için yardımcı fonksiyon
function isToday(dateString: string): boolean {
  if (!dateString)
    return false

  const orderDate = new Date(dateString)
  const today = new Date()

  return orderDate.getFullYear() === today.getFullYear()
    && orderDate.getMonth() === today.getMonth()
    && orderDate.getDate() === today.getDate()
}

// Bugün filtresini toggle et
function toggleTodayFilter() {
  showTodayOnly.value = !showTodayOnly.value
}

const filteredOrders = computed(() => {
  // Props kontrolü
  if (!props.orders || !Array.isArray(props.orders)) {
    return []
  }

  return props.orders.filter((order) => {
    if (!order)
      return false

    // Sipariş no filtresi
    const matchesOrderNo = filterOrderNo.value
      ? String(order.uid || '').toLowerCase().includes(filterOrderNo.value.toLowerCase())
      : true

    // Masa no filtresi
    const matchesTableId = filterTableId.value
      ? String(order.tableId || '').includes(filterTableId.value)
      : true

    // Durum filtresi
    const matchesStatus = filterStatus.value
      ? order.status === filterStatus.value
      : true

    // Bugün filtresi
    const matchesToday = showTodayOnly.value
      ? isToday(order.orderDate)
      : true

    return matchesOrderNo && matchesTableId && matchesStatus && matchesToday
  })
})

// Filter değişikliklerini parent'a bildir
watch([filterOrderNo, filterTableId, filterStatus, showTodayOnly], () => {
  emit('filter-change', {
    orderNo: filterOrderNo.value,
    tableId: filterTableId.value,
    status: filterStatus.value,
    todayOnly: showTodayOnly.value, // Yeni filtre parametresi
  })
}, { deep: true })
</script>

<template>
  <div
    class="bg-white shadow-md rounded-2xl p-4 sm:p-6 opacity-0 animate-fade-in w-full h-full"
    style="animation-delay: 0.4s"
  >
    <h2 class="text-lg sm:text-xl font-bold text-gray-800 mb-4 sm:mb-6">
      Son Siparişler
      <span v-if="isAdmin" class="text-sm text-gray-600">(Tüm siparişler)</span>
      <span v-if="showTodayOnly" class="text-sm text-blue-600 ml-2">• Bugün</span>
    </h2>

    <!-- Filtreler -->
    <div class="flex flex-col gap-2 mb-4">
      <!-- Üst satır: Arama filtreleri -->
      <div class="flex flex-col sm:flex-row gap-2">
        <CustomInput
          v-model="filterOrderNo"
          icon="material-symbols:search"
          placeholder="Sipariş No ile ara"
        />
        <CustomInput
          v-model="filterTableId"
          icon="material-symbols:search"
          placeholder="Masa No ile ara"
        />
        
        <OrderStatusFilter v-model="filterStatus" />
           <button
          class="px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200 flex items-center gap-2"
          :class="[
            showTodayOnly
              ? 'bg-blue-600 text-white shadow-md !rounded-full'
              : ' text-white hover:bg-gray-200 bg-green-400 !rounded-full',
          ]"
          @click="toggleTodayFilter"
        >
          <Icon
            :name="showTodayOnly ? 'material-symbols:check-circle' : 'material-symbols:today'"
            class="w-4 h-4"
          />
          {{ showTodayOnly ? 'Bugünkü Siparişler' : 'Sadece Bugün' }}
        </button>
      </div>

      <!-- Alt satır: Bugün butonu -->

    </div>

    <!-- Sipariş listesi -->
    <div v-if="!props.orders || props.orders.length === 0" class="text-gray-500 text-center py-8">
      Henüz sipariş yok.
    </div>
    <div v-else-if="filteredOrders.length === 0" class="text-gray-500 text-center py-8">
      <div v-if="showTodayOnly">
        Bugün henüz sipariş yok.
      </div>
      <div v-else>
        Filtre kriterlerine uygun sipariş bulunamadı.
      </div>
    </div>
    <div v-else class="space-y-3 sm:space-y-4">
      <div
        v-for="(order, index) in filteredOrders"
        :key="order?.uid || index"
        class="bg-gray-50 shadow-sm shadow-gray-300 p-3 sm:p-4 rounded-xl opacity-0 animate-fade-in cursor-pointer"
        :style="{ animationDelay: `${0.5 + index * 0.1}s` }"
        @click="openOrderDetail(order)"
      >
        <div class="flex justify-between items-start mb-2">
          <div>
            <p class="font-semibold text-gray-800 text-sm sm:text-base">
              Masa {{ order?.tableId || 'N/A' }}
            </p>
            <p class="text-xs sm:text-sm text-gray-500">
              {{ order?.orderDate ? order.orderDate.substring(11, 16) : '' }}
              <span v-if="showTodayOnly" class="text-blue-600 ml-1">• Bugün</span>
            </p>
          </div>
          <span class="text-base sm:text-lg font-semibold text-blue-600">
            {{ order?.totalPrice || 0 }}₺
          </span>
        </div>
        <span
          class="px-2 sm:px-3 py-1 rounded-full text-xs sm:text-sm font-medium inline-block"
          :class="getOrderStatusClass(order?.status || '')"
        >
          {{ order?.status || 'Bilinmiyor' }}
        </span>
      </div>
    </div>
  </div>
</template>
