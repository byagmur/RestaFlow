<script setup lang="ts">
import { Icon } from '@iconify/vue'

const props = defineProps<{
  modelValue: string
  placeholder?: string
  icon?: string // Örn: 'mdi:table'
}>()
const emit = defineEmits(['update:modelValue'])
</script>

<template>
  <div class="relative flex items-center w-45">
    <span v-if="icon" class="absolute left-3 flex items-center pointer-events-none text-gray-400">
      <Icon :icon="icon" class="w-5 h-5" />
    </span>
    <input
      :value="props.modelValue"
      @input="emit('update:modelValue', $event.target.value)"
      :placeholder="props.placeholder || 'Arama...'"
      type="text"
      class="bg-gray-100 shadow-lg rounded-2xl px-3 py-2 text-md focus:outline-none focus:ring-2 focus:ring-gray-300 w-full"
      :class="{ 'pl-10': props.icon }"
    >
  </div>
</template>