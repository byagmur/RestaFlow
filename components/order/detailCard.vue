<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { OrderItem } from '~/types'

const props = defineProps<{ order: any }>()
defineEmits(['close'])

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
          <button
            class="text-gray-400 hover:text-red-500 !text-4xl font-bold"
            aria-label="Kapat"
            @click="$emit('close')"
          >
            &times;
          </button>
        </div>
      </template>

      <div class="px-6 py-4 space-y-3 text-gray-700 text-[17px] leading-relaxed">
        <div><strong>Masa:</strong> {{ order.tableId }}</div>
        <div><strong>Tarih:</strong> {{ formatDate(order.orderDate) }}</div>
        <div><strong>Durum:</strong> {{ order.status }}</div>
        <div><strong>Tutar:</strong> {{ order.totalPrice }}₺</div>
        <div v-if="order.note">
          <strong>Not:</strong> {{ order.note }}
        </div>
      </div>

      <div class="px-6 pb-6 pt-2">
        <div v-if="order.items && order.items.length">
          <UTable
            :data="uniqueItems"
            :columns="columns"
            class="mt-4 border border-gray-200 rounded-xl overflow-hidden shadow-sm"
            :ui="{
              base: 'text-[15px]',
              th: 'bg-gray-100 text-gray-600 font-semibold tracking-wide uppercase text-sm px-4 py-2 border-b border-gray-200',
              td: 'px-4 py-3 border-b border-gray-100 group-hover:bg-gray-50 transition',
              tr: 'hover:bg-gray-50',
            }"
          />
        </div>
        <div v-else class="text-gray-500 text-center mt-6">
          Ürün bilgisi bulunmamaktadır.
        </div>
      </div>
    </UCard>
  </div>
</template>
