import axios from 'axios'
import { defineStore } from 'pinia'
import { useAuthStore } from '@/stores/authStore'

export const useOrderStore = defineStore('order', () => {
  const baseUrl = useRuntimeConfig().public.apiBase
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const orders = ref([])

  // Sipariş ve ürün detaylarını birlikte oluşturur
  async function createOrderWithDetails(orderData: any, items: any[]) {
    console.log('createOrderWithDetails ÇAĞRILDI', orderData, items)
    isLoading.value = true
    error.value = null
    try {
      if (!orderData.tableID) {
        throw new Error('Masa seçilmedi! (tableID eksik)')
      }

      const authStore = useAuthStore()
      const waiterID = authStore.userInfo.id

      // 1. Siparişi oluştur
      const orderRes = await axios.post(`${baseUrl}/Orders`, orderData)
      console.log('Sipariş oluşturuldu:', orderRes.data)
      const ConstorderId = Number(orderRes.data.orderId)
      console.log('Sipariş oluşturuldu, orderId:', ConstorderId)

      // 2. Her ürün için detay kaydı oluştur
      console.log('Detay ekleme başlıyor, items:', items, 'length:', items.length)
      if (!items || items.length === 0) {
        console.warn('UYARI: items dizisi BOŞ! Sipariş detayları eklenmeyecek.')
      }

      for (const food of items) {
        const detayPayload = {
          orderId: ConstorderId,
          productId: food.no,
          quantity: food._orderCount, // seçili miktarı gönder
          insertDate: new Date().toISOString(),
        }
        console.log('Detay için gönderilen veri:', detayPayload)
        try {
          await axios.post(`${baseUrl}/Orders/detail`, detayPayload)
        }
        catch (err: any) {
          console.error('DETAY KAYDI HATASI:', err?.response?.data || err)
          throw err // döngüyü durdurmak için
        }
      }

      console.log('Tüm detaylar eklendi.')

      // 3. Sipariş hareketi kaydı oluştur
      await axios.post(`${baseUrl}/Orders/movement`, {
        orderId: ConstorderId,
        orderStatus: orderData.status,
        employeeId: Number(waiterID),
        movementDate: new Date().toISOString(),
      })
      console.log('Hareket kaydı eklendi.')

      isLoading.value = false
      return ConstorderId
    }
    catch (err: any) {
      console.error('HATA:', err?.response?.data || err)
      error.value = err?.response?.data?.message || 'Sipariş oluşturulamadı.'
      isLoading.value = false
      throw error.value
    }
  }

  async function fetchOrders({ start = 0, limit = 7 } = {}) {
    isLoading.value = true
    error.value = null
    try {
      const res = await axios.get(`${baseUrl}/Orders`, {
        params: { start, limit },
      })
      orders.value = res.data.data || []

      console.log('Siparişler başarıyla alındı:', orders.value)
      // Bunu ekle:
      return { totalCount: res.data.totalCount }
    }
    catch (err: any) {
      error.value = err?.response?.data || err.message
    }
    finally {
      isLoading.value = false
    }
  }

  return { fetchOrders, orders, createOrderWithDetails, isLoading, error }
})
