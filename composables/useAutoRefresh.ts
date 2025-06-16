import { isClient } from '@vueuse/core'
import { ref, onUnmounted, readonly } from 'vue'

export interface RefreshOptions {
  interval?: number // Milisaniye cinsinden
  immediate?: boolean // Hemen başlasın mı
  onVisibilityChange?: boolean // Sayfa focus durumunu takip et
}

export const useAutoRefresh = (
  refreshCallback: () => Promise<void> | void,
  options: RefreshOptions = {}
) => {
  const {
    interval = 1000, 
    immediate = true,
    onVisibilityChange = true
  } = options

  const isRefreshing = ref(false)
  const isActive = ref(false)
  const lastRefresh = ref<Date | null>(null)
  const refreshInterval = ref<NodeJS.Timeout | null>(null)
  const errorCount = ref(0)
  const maxErrors = 3


  const refresh = async (silent = false) => {
    if (isRefreshing.value) return

    isRefreshing.value = true
    
    try {
      await refreshCallback()
      lastRefresh.value = new Date()
      errorCount.value = 0 // Başarılı olunca error count'u sıfırla
      
      if (!silent) {
        console.log('✅ Veriler güncellendi:', lastRefresh.value.toLocaleTimeString())
      }
    } catch (error) {
      errorCount.value++
      console.error(`❌ Yenileme hatası (${errorCount.value}/${maxErrors}):`, error)
      
      // Çok fazla hata varsa otomatik yenilemeyi durdur
      if (errorCount.value >= maxErrors) {
        console.warn('⚠️ Çok fazla hata nedeniyle otomatik yenileme durduruluyor')
        stop()
      }
    } finally {
      isRefreshing.value = false
    }
  }


  const start = () => {
    if (refreshInterval.value) return 

    isActive.value = true
    refreshInterval.value = setInterval(() => {
      refresh(true) // Silent refresh
    }, interval)

    console.log(`🔄 Otomatik yenileme başlatıldı (${interval/1000}s)`)
  }


  const stop = () => {
    if (refreshInterval.value) {
      clearInterval(refreshInterval.value)
      refreshInterval.value = null
    }
    isActive.value = false
    console.log('⏹️ Otomatik yenileme durduruldu')
  }


  if (isClient && onVisibilityChange) {
    const handleVisibilityChange = () => {
      if (document.hidden) {
        stop()
      } else if (immediate) {
        start()
        refresh(true) // Sayfa tekrar aktif olunca hemen yenile
      }
    }

    document.addEventListener('visibilitychange', handleVisibilityChange)
    
    onUnmounted(() => {
      document.removeEventListener('visibilitychange', handleVisibilityChange)
    })
  }

  // Component unmount olunca temizle
  onUnmounted(() => {
    stop()
  })

  // İlk başlatma
  if (immediate && process.client) {
    start()
  }

  return {
    // State
    isRefreshing: readonly(isRefreshing),
    isActive: readonly(isActive),
    lastRefresh: readonly(lastRefresh),
    errorCount: readonly(errorCount),
    
    // Actions
    start,
    stop,
    refresh: () => refresh(false), // Public manual refresh
    
    // Utils
    toggle: () => isActive.value ? stop() : start(),
    getStatus: () => ({
      isRefreshing: isRefreshing.value,
      isActive: isActive.value,
      lastRefresh: lastRefresh.value,
      errorCount: errorCount.value,
      hasErrors: errorCount.value > 0
    })
  }
}