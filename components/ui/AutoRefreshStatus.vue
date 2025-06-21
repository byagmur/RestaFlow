<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  isRefreshing: boolean
  isActive: boolean
  lastRefresh: Date | null
  errorCount?: number
  showLastRefresh?: boolean
  showControls?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  errorCount: 0,
  showLastRefresh: true,
  showControls: true
})

const emit = defineEmits<{
  refresh: []
  toggle: []
}>()

const statusText = computed(() => {
  if (props.isRefreshing) return 'Güncelleniyor...'
  if (!props.isActive) return 'Otomatik güncelleme kapalı'
  if (props.errorCount > 0) return `Hata oluştu (${props.errorCount})`
  return 'Otomatik güncelleme aktif'
})

const statusIcon = computed(() => {
  if (props.isRefreshing) return 'eos-icons:loading'
  if (!props.isActive) return 'material-symbols:pause-circle'
  if (props.errorCount > 0) return 'material-symbols:error'
  return 'material-symbols:refresh'
})

const statusColor = computed(() => {
  if (props.isRefreshing) return 'text-blue-600'
  if (!props.isActive) return 'text-gray-500'
  if (props.errorCount > 0) return 'text-red-600'
  return 'text-green-600'
})

const bgColor = computed(() => {
  if (props.errorCount > 0) return 'bg-red-50 border-red-200'
  if (!props.isActive) return 'bg-gray-50 border-gray-200'
  return 'bg-blue-50 border-blue-200'
})
</script>

<template>
  <div class="flex justify-between items-center p-3 rounded-lg border transition-colors" :class="bgColor">
    <div class="flex items-center gap-2">
      <Icon 
        :name="statusIcon"
        :class="['w-5 h-5', statusColor, { 'animate-spin': isRefreshing }]"
      />
      <span class="text-sm font-medium" :class="statusColor">
        {{ statusText }}
      </span>
      <span 
        v-if="showLastRefresh && lastRefresh" 
        class="text-xs text-gray-500"
      >
        ({{ lastRefresh.toLocaleTimeString() }})
      </span>
    </div>
    
    <div v-if="showControls" class="flex gap-2">
      <button
        class="px-3 py-1 text-xs bg-white border rounded-md hover:bg-gray-50 transition"
        :class="isActive ? 'text-red-600 border-red-300' : 'text-green-600 border-green-300'"
        @click="emit('toggle')"
      >
        {{ isActive ? 'Durdur' : 'Başlat' }}
      </button>
      <button
        class="px-3 py-1 text-xs bg-blue-600 text-white rounded-md hover:bg-blue-700 transition disabled:opacity-50"
        :disabled="isRefreshing"
        @click="emit('refresh')"
      >
        Manuel Yenile
      </button>
    </div>
  </div>
</template>