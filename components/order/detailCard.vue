<script setup lang="ts">
defineProps<{ order: any }>();
defineEmits(['close']);

// Simple date formatting function
function formatDate(date: string | Date): string {
  const d = new Date(date);
  return d.toLocaleDateString('tr-TR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  });
}

</script>

<template>
  <div
    v-if="order"
    class="fixed inset-0 z-50 flex items-center justify-center"
    style="background: rgba(249,250,251,0.8);"
  >
    <UCard class="max-w-md shadow-lg rounded-xl !w-4xl h-auto relative animate-fade-in"
      :ui="{ base: 'max-w-4xl w-full' }">
      <template #header>
        <button
          @click="$emit('close')"
          class="absolute top-2 right-2 text-gray-400 hover:text-gray-700 text-3xl mr-2 font-bold"
        >&times;</button>
        <h3 class="text-3xl font-bold text-gray-800">Sipariş Detayları</h3>
      </template>
      <div class="space-y-2 text-gray-700 text-xl">
        <div><b>Masa: </b> {{ order.tableId }}</div>
        <div><b>Tarih: </b> {{ formatDate(order.orderDate) }}</div>
        <div><b>Durum: </b> {{ order.status }}</div>
        <div><b>Tutar: </b> {{ order.totalPrice }}₺</div>
        <div v-if="order.note"><b>Not: </b> {{ order.note }}</div>
        <div><b>Sipariş No: </b> {{ order.uid }}</div>
      </div>
      <!-- Ürünler -->
      <div v-if="order.items && order.items.length" class="mt-6">
        <h4 class="text-lg font-bold mb-2 text-gray-800">Ürünler</h4>
        <table class="w-full text-base text-gray-700 border">
          <thead>
            <tr class="border-b">
              <th class="py-1 text-left">Ürün</th>
              <th class="py-1 text-right">Adet</th>
              <th class="py-1 text-right">Fiyat</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in order.items" :key="item.id || item.name" class="border-b">
              <td class="py-1">{{ item.name }}</td>
              <td class="py-1 text-right">{{ item.quantity }}</td>
              <td class="py-1 text-right">{{ item.price }}₺</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-else class="mt-6 text-gray-400 text-base">Ürün bilgisi yok.</div>
    </UCard>
  </div>
</template>

