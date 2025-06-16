<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { OrderItem } from '~/types'
import { ref } from 'vue'
import { useOrderStore } from '@/stores/orderStore'

const props = defineProps<{ order: any }>()
defineEmits(['close'])

const orderStore = useOrderStore()
const loading = ref(false)
const success = ref(false)
const error = ref('')

function formatDate(date: string | Date): string {
  const d = new Date(date)
  return d.toLocaleDateString('tr-TR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const columns: TableColumn<OrderItem>[] = [
  { accessorKey: 'name', header: 'Ürün Adı', cell: info => info.getValue() },
  { accessorKey: 'quantity', header: 'Adet', cell: info => info.getValue() },
  { accessorKey: 'price', header: 'Fiyat', cell: info => `${info.getValue()}₺` },
]

const uniqueItems = computed(() => {
  if (!props.order?.items)
    return []
  const seen = new Set()
  return props.order.items.filter((item: OrderItem) => {
    if (seen.has(item.name))
      return false
    seen.add(item.name)
    return true
  })
})

async function handleServeOrder() {
  loading.value = true
  error.value = ''
  success.value = false
  try {
    await orderStore.updateOrderStatus(Number(props.order.uid), 'Servis Edildi')
    success.value = true
    // props.order.status = 'Servis Edildi' // Gerekirse local güncelleme
  }
  catch (e: any) {
    error.value = e || 'Bir hata oluştu'
  }
  finally {
    loading.value = false
  }
}
</script>

<template>
  <div
    v-if="order"
    class="fixed inset-0 z-50 flex items-center justify-center bg-gray-100/90 backdrop-blur-sm"
  >
    <UCard
      class="relative w-full max-w-4xl mx-4 rounded-2xl shadow-xl border border-gray-200 bg-white animate-fade-in"
    >
      <template #header>
        <div class="flex items-center justify-between border-b border-gray-200 px-6 py-4">
          <h2 class="text-2xl font-bold text-gray-800">
            #{{ order.uid }} Sipariş Detayları
          </h2>
          <div class="flex items-center gap-2">
            <!-- SAĞ ÜSTTE SERVİS ET BUTONU HER ZAMAN GÖRÜNÜR -->
            <UButton
              :color="order.status === 'Tamamlandı' ? 'primary' : 'gray'"
              class="!text-white text-lg rounded-lg px-4 py-2 transition-colors" :class="[
                order.status === 'Tamamlandı'
                  ? '!bg-orange-400 hover:!bg-orange-500 cursor-pointer'
                  : '!bg-gray-300 cursor-not-allowed !text-gray-500',
              ]"
              :disabled="order.status !== 'Tamamlandı'"
              :loading="loading"
              @click="order.status === 'Tamamlandı' && handleServeOrder()"
            >
              Servis Et
            </UButton>
            <button
              class="text-gray-400 hover:text-red-500 !text-4xl font-bold ml-2"
              aria-label="Kapat"
              @click="$emit('close')"
            >
              &times;
            </button>
          </div>
        </div>

        <div v-if="order.status === 'Tamamlandı' && (success || error)" class="flex justify-end px-6 pt-1">
          <h1 v-if="success" icon="cuida:info-outline" class="absolute z-20 ml-3 text-green-600">Sipariş durumu güncellendi!</h1>
          <p v-if="error" class="ml-3 text-red-600">{{ error }}</p>
        </div>
      </template>

      <div class="px-6 pb-4 space-y-3 text-gray-700 text-lg leading-relaxed">
        <div><strong>Masa:</strong> {{ order.tableId }}</div>
        <div><strong>Tarih:</strong> {{ formatDate(order.orderDate) }}</div>
        <div><strong>Durum:</strong> {{ order.status }}</div>
        <div><strong>Tutar:</strong> {{ order.totalPrice }}₺</div>
        <div v-if="order.note">
          <strong>Not:</strong> {{ order.note }}
        </div>
      </div>

      <div
        v-if="order.items && order.items.length"
        class="mt-4 px-4 pb-6 overflow-auto"
      >
        <div class="border border-gray-200 rounded-lg overflow-hidden">
          <UTable
            :data="uniqueItems"
            :columns="columns"
            :ui="{
              base: 'min-w-full text-gray-600 text-base',
              th: 'bg-gray-100 text-gray-600 uppercase text-sm tracking-wider px-4 py-2 border-b border-gray-100',
              td: 'px-4 py-3 border-b border-gray-200',
              tr: 'hover:bg-gray-50 transition',
            }"
          />
        </div>
      </div>

      <div v-else class="space-y-3 sm:space-y-4">
        <div
          v-for="(order, index) in filteredOrders"
          :key="order.uid"
          class="bg-gray-50 shadow-sm shadow-gray-300 p-3 sm:p-4 rounded-xl opacity-0 animate-fade-in cursor-pointer"
          :style="{ animationDelay: `${0.5 + index * 0.1}s` }"
          @click="openOrderDetail(order)"
        >
          <div class="flex justify-between items-center">
            <div>
              <div class="font-bold text-gray-700">
                #{{ order.uid }}
              </div>
              <div class="text-gray-500 text-sm">
                Masa: {{ order.tableId }}
              </div>
            </div>
            <div class="text-xs px-2 py-1 rounded" :class="getOrderStatusClass(order.status)">
              {{ order.status }}
            </div>
          </div>
          <div class="text-gray-400 text-xs mt-1">
            {{ order.orderDate }}
          </div>
        </div>
      </div>
    </UCard>
  </div>
</template>
