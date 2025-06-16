import type { Order } from '~/types'
import axios from 'axios'
import { defineStore } from 'pinia'
import { useAuthStore } from '@/stores/authStore'

export const useOrderStore = defineStore('order', () => {
  const baseUrl = useRuntimeConfig().public.apiBase
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  const orders = ref<Order[]>([])
  const order = ref<Order | null>(null)
  const pagedOrders = ref<Order[]>([])
  const totalOrderCount = ref(0) // Bu satır eklendi!

  /**
   * Sipariş hareketi ekler
   */
  async function addOrderMovement({
    orderId,
    orderStatus,
    employeeId,
    movementDate,
  }: {
    orderId: number
    orderStatus: string
    employeeId: number
    movementDate?: string
  }) {
    await axios.post(`${baseUrl}/Orders/movement`, {
      orderId,
      orderStatus,
      employeeId,
      movementDate: movementDate || new Date().toISOString(),
    })
  }

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
          throw err
        }
      }

      console.log('Tüm detaylar eklendi.')

      // 3. Hareket kaydı ekle
      await addOrderMovement({
        orderId: ConstorderId,
        orderStatus: orderData.status,
        employeeId: Number(waiterID),
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
      if (waiterID)
        params.waiterID = waiterID
      const res = await $fetch(`${baseUrl}/Orders`, {
        params,
      }) as { data: any[], totalCount: number }
      orders.value = res.data
      console.log('fetchOrders response:', res) // Debug için
      return { totalCount: res.totalCount }
    }
    catch (err: any) {
      error.value = err?.response?.data || err.message
      console.error('fetchOrders error:', err) // Debug için
    }
    finally {
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
              price: item.price,
            }))
          : [],
      }
      console.log('siparişin ürünleri', order.value.items)
    }
    catch (err: any) {
      error.value = err?.response?.data || err.message
      throw error.value
    }
    finally {
      isLoading.value = false
    }
  }

  async function fetchLastOrdersPerTable({ start = 0, limit = 5, waiterID }: { start?: number, limit?: number, waiterID?: number } = {}) {
    isLoading.value = true
    error.value = null
    try {
      const params: any = { start, limit }
      if (waiterID)
        params.waiterID = waiterID

      console.log('fetchLastOrdersPerTable params:', params) // Debug için

      const res = await $fetch(`${baseUrl}/Orders/last-orders-per-table`, {
        params,
      }) as { data: any[], totalCount: number }

      console.log('fetchLastOrdersPerTable response:', res) // Debug için

      pagedOrders.value = (res.data || []).map((order: any) => ({
        uid: order.uid,
        tableId: order.tableID,
        orderDate: order.orderDate,
        totalPrice: order.totalPrice,
        status: order.status,
        waiterId: order.waiterID,
        note: order.note,
        items: Array.isArray(order.items)
          ? order.items.map((item: any) => ({
              id: item.id,
              name: item.name,
              quantity: item.quantity,
              price: item.price,
            }))
          : [],
      })) as Order[]

      // TotalOrderCount'u güncelle - Bu satır eklendi!
      totalOrderCount.value = res.totalCount || 0

      console.log('Set totalOrderCount to:', totalOrderCount.value) // Debug için
      console.log('pagedOrders length:', pagedOrders.value.length) // Debug için

      return { totalCount: res.totalCount }
    }
    catch (err: any) {
      console.error('fetchLastOrdersPerTable error:', err) // Debug için
      error.value = err?.response?.data || err.message
    }
    finally {
      isLoading.value = false
    }
  }

  async function updateOrderStatus(orderId: number, status: string) {
    isLoading.value = true
    error.value = null
    try {
      await $fetch(`${baseUrl}/Orders/status`, {
        method: 'PUT',
        body: {
          orderId,
          status,
        },
      })
      if (
        order.value
        && (Number(order.value.uid) === orderId)
      ) {
        order.value.status = status
      }

      // sipariş durumunu sipariş hareketleri tablosnuna ekle
      addOrderMovement({
        orderId,
        orderStatus: status,
        employeeId: useAuthStore().userInfo.id,
        movementDate: new Date().toISOString(),
      })
    }
    catch (err: any) {
      error.value = err?.response?.data?.message || err.message || 'Durum güncellenemedi.'
      throw error.value
    }
    finally {
      isLoading.value = false
    }
  }

  return {
    fetchOrders,
    fetchLastOrdersPerTable,
    orders,
    pagedOrders,
    totalOrderCount, // Bu satır eklendi!
    createOrderWithDetails,
    isLoading,
    error,
    fetchOrderDetail,
    order,
    updateOrderStatus,
    addOrderMovement,
  }
})
