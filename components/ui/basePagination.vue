<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  modelValue: number
  total: number
  pageSize: number
}>()
const emit = defineEmits(['update:modelValue'])

const pageCount = computed(() => Math.ceil(props.total / props.pageSize))

function setPage(page: number) {
  if (page >= 1 && page <= pageCount.value) {
    emit('update:modelValue', page)
  }
}
</script>

<template>
  <div
    v-if="pageCount > 1"
    class="flex items-center justify-center gap-2 mt-4 p-2"
  >
    <UButton
      :disabled="props.modelValue === 1"
      class="px-3 py-2 rounded-lg border border-gray-300 bg-gray-100 text-gray-700 hover:bg-gray-200 transition"
      icon="solar:skip-previous-linear"
      @click="setPage(props.modelValue - 1)"
    />

    <button
      v-for="n in pageCount"
      :key="n"
      class="w-9 h-9 text-sm font-medium flex items-center justify-center border rounded-lg transition-all duration-200"
      :class="[
        n === props.modelValue
          ? 'bg-blue-600 text-white border-blue-600 shadow-md scale-105'
          : 'bg-gray-100 text-gray-700 border-gray-300 hover:bg-gray-200',
      ]"
      @click="setPage(n)"
    >
      {{ n }}
    </button>

    <UButton
      :disabled="props.modelValue === pageCount"
      class="px-3 py-2 rounded-lg border border-gray-300 bg-gray-100 text-gray-700 hover:bg-gray-200 transition"
      icon="solar:skip-next-outline"
      @click="setPage(props.modelValue + 1)"
    />
  </div>
</template>
