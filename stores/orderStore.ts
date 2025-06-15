import axios from 'axios'
import { defineStore } from 'pinia'
import { useAuthStore } from '@/stores/authStore'
import type { Order } from '~/types'

export const useOrderStore = defineStore('order', () => {
  const baseUrl = useRuntimeConfig().public.apiBase
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const orders = ref<Order[]>([])
  const order = ref<Order | null>(null)

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

  async function fetchOrders({ start = 0, limit = 20, waiterID }: { start?: number, limit?: number, waiterID?: number } = {}) {
  isLoading.value = true
  error.value = null
  try {
    const params: any = { start, limit }
    if (waiterID) params.waiterID = waiterID
    const res = await $fetch(`${baseUrl}/Orders`, {
      params
    }) as { data: any[]; totalCount: number }
    orders.value = (res.data || []).map((order: any) => ({
      uid: order.uid,
      tableId: order.tableID,
      orderDate: order.orderDate,
      totalPrice: order.totalPrice,
      status: order.status,
      waiterId: order.waiterID
    })) as Order[]
    return { totalCount: res.totalCount }
  } catch (err: any) {
    error.value = err?.response?.data || err.message
  } finally {
    isLoading.value = false
  }
}

  async function fetchOrderDetail(orderId: string | number) {
    isLoading.value = true
    error.value = null
    try {
      const res = await $fetch<any>(`${baseUrl}/Orders/${orderId}`)
      order.value = {
        uid: res.uid,
        tableId: res.tableId ?? res.tableID,
        orderDate: res.orderDate,
        totalPrice: res.totalPrice,
        status: res.status,
        waiterId: res.waiterId ?? res.waiterID,
        note: res.note,
        waiterName: res.waiterName,
        items: Array.isArray(res.items)
          ? res.items.map((item: any) => ({
              id: item.id,
              name: item.name,
              quantity: item.quantity,
              price: item.price
            }))
          : []
      }
      console.log('siparişin ürünleri', order.value.items)
    } catch (err: any) {
      error.value = err?.response?.data || err.message
      throw error.value
    } finally {
      isLoading.value = false
    }
  }

  return { fetchOrders, orders, createOrderWithDetails, isLoading, error, fetchOrderDetail, order}
})
