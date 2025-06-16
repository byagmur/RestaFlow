import { inject } from 'vue'

export const useRefreshContext = () => {
  const autoRefresh = inject<ReturnType<typeof useAutoRefresh>>('autoRefresh')
  
  if (!autoRefresh) {
    throw new Error('useRefreshContext must be used within DataRefreshProvider')
  }
  
  return autoRefresh
}