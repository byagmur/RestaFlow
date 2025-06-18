<script setup lang="ts">
import type { Order } from '@/types'
import { computed, ref, watch } from 'vue'
import { useAuthStore } from '@/stores/auth/authStore'

const props = defineProps<{
  orders: Order[]
  weeklyTarget?: number
}>()

const emit = defineEmits(['update:weeklyTarget'])
const authStore = useAuthStore()
const showTargetInput = ref(false)
const newTarget = ref(props.weeklyTarget ?? 10000)

watch(() => props.weeklyTarget, (val) => {
  newTarget.value = val ?? 10000
})

const totalEarnings = computed(() => {
  return (
    `₺${
      props.orders.reduce((sum, order) => {
        const t = typeof order.totalPrice === 'string' ? Number(order.totalPrice.replace(/\D/g, '')) : order.totalPrice
        return sum + (t || 0)
      }, 0).toLocaleString('tr-TR')}`
  )
})

const progressPercent = computed(() => {
  const total = props.orders.reduce((sum, order) => {
    const t = typeof order.totalPrice === 'string' ? Number(order.totalPrice.replace(/\D/g, '')) : order.totalPrice
    return sum + (t || 0)
  }, 0)
  const target = props.weeklyTarget ?? 10000
  return Math.min(100, Math.round((total / target) * 100))
})

function saveTarget() {
  emit('update:weeklyTarget', Number(newTarget.value))
  showTargetInput.value = false
}
</script>

<template>
  <div class="bg-white shadow-md rounded-2xl p-4 sm:p-6 opacity-0 animate-fade-in relative" style="animation-delay: 0.3s">
    <div class="flex items-center justify-between mb-6 sm:mb-8">
      <h2 class="text-lg sm:text-md xs:text-sm font-bold text-gray-800">
        Toplam Kazanç
      </h2>
      <span class="text-gray-500 text-sm sm:text-base">Haftalık Hedef: ₺{{ props.weeklyTarget }}</span>
      <div v-if="authStore.userInfo.role == 'Yönetici'" class="relative flex items-center">
        <UButton
          class="!text-sm sm:!text-base xs:!text-xs rounded-full shadow-sm mr-2"
          @click="showTargetInput = !showTargetInput"
        >
          Haftalık Hedef Belirle
        </UButton>
        <div v-if="showTargetInput" class="w-45 h-15  absolute left-full ml-2 flex items-center bg-white border-none shadow-md rounded px-2 py-1 z-10">
          <input
            v-model="newTarget"
            type="number"
            min="0"
            class="w-24 px-2 py-1 border rounded text-gray-800 text-sm mr-2"
            placeholder="Hedef ₺"
          />
          <UButton size="xs" @click="saveTarget" class="!text-xs !px-2">Kaydet</UButton>
        </div>
      </div>
      <span class="text-blue-600 text-xl sm:text-2xl font-bold">{{ totalEarnings }}</span>
    </div>

    <div class="w-24 h-24 sm:w-32 sm:h-32 mx-auto relative">
      <svg class="w-full h-full" viewBox="0 0 100 100">
        <circle
          class="text-gray-200 stroke-current"
          stroke-width="10"
          fill="transparent"
          r="45"
          cx="50"
          cy="50"
        />
        <circle
          class="text-blue-600 stroke-current"
          stroke-width="10"
          stroke-linecap="round"
          fill="transparent"
          r="45"
          cx="50"
          cy="50"
          :style="{
            strokeDasharray: 283,
            strokeDashoffset: 283 - (progressPercent * 2.83),
          }"
        />
      </svg>
      <div class="absolute inset-0 flex items-center justify-center">
        <span class="text-gray-800 text-base sm:text-lg font-bold">{{ progressPercent }}%</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
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
</style>
