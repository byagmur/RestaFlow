<script setup lang="ts">
import { defineEmits, defineProps } from 'vue'

const props = defineProps<{
  orders: any[]
  totalOrders: number
  page: number
  pageSize: number
}>()

const emit = defineEmits(['update:page'])

function onPageChange(newPage: number) {
  emit('update:page', newPage)
}

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
</script>

<template>
  <div class="bg-white shadow-md rounded-2xl p-4 sm:p-6 opacity-0 animate-fade-in w-full h-full" style="animation-delay: 0.4s">
    <h2 class="text-lg sm:text-xl font-bold text-gray-800 mb-4 sm:mb-6">
      Son Siparişler
    </h2>
    <div v-if="props.orders.length === 0" class="text-gray-500 text-center py-8">
      Gösterilecek sipariş yok.
    </div>
    <div v-else class="space-y-3 sm:space-y-4">
      <div
        v-for="(order, index) in props.orders"
        :key="order.uid"
        class="bg-gray-50 p-3 sm:p-4 rounded-xl opacity-0 animate-fade-in"
        :style="{ animationDelay: `${0.5 + index * 0.1}s` }"
      >
        <div class="flex justify-between items-start mb-2">
          <div>
            <p class="font-semibold text-gray-800 text-sm sm:text-base">
              Masa {{ order.tableID }}
            </p>
            <p class="text-xs sm:text-sm text-gray-500">
              {{ order.orderDate ? order.orderDate.substring(11, 16) : '' }}
            </p>
          </div>
          <span class="text-base sm:text-lg font-semibold text-blue-600">{{ order.totalPrice }}₺</span>
        </div>
        <span
          class="px-2 sm:px-3 py-1 rounded-full text-xs sm:text-sm font-medium inline-block"
          :class="getOrderStatusClass(order.status)"
        >
          {{ order.status }}
        </span>
      </div>
      <UPagination
        v-if="props.totalOrders > props.pageSize"
        :model-value="props.page"

        :total="props.totalOrders"
        class="mt-4"
        @update:model-value="onPageChange"
      />
    </div>
  </div>
</template>
