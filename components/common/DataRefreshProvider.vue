<script setup lang="ts">
import { provide } from 'vue'
import { useAutoRefresh, type RefreshOptions } from '@/composables/useAutoRefresh'
import AutoRefreshStatus from '@/components/ui/AutoRefreshStatus.vue'

interface Props {
  refreshCallback: () => Promise<void> | void
  options?: RefreshOptions
  showStatus?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  showStatus: true,
  options: () => ({
    interval: 10000,
    immediate: true,
    onVisibilityChange: true
  })
})

// Auto refresh composable'ı kullan
const autoRefresh = useAutoRefresh(props.refreshCallback, props.options)

// Child component'lara provide et
provide('autoRefresh', autoRefresh)
</script>

<template>
  <div class="space-y-4">

    <AutoRefreshStatus
      v-if="showStatus"
      :is-refreshing="autoRefresh.isRefreshing.value"
      :is-active="autoRefresh.isActive.value"
      :last-refresh="autoRefresh.lastRefresh.value"
      :error-count="autoRefresh.errorCount.value"
      @refresh="autoRefresh.refresh"
      @toggle="autoRefresh.toggle"
    />
    
    <slot 
      :refresh="autoRefresh.refresh"
      :is-refreshing="autoRefresh.isRefreshing.value"
      :is-active="autoRefresh.isActive.value"
      :last-refresh="autoRefresh.lastRefresh.value"
    />
  </div>
</template>