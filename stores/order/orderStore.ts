import type { Order, MoveOrderRequest, MoveOrderResponse } from '~/types'
import axios from 'axios'
import { defineStore } from 'pinia'
import { useAuthStore } from '~/stores/auth/authStore'

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
    targetTableId, // Yeni parametre eklendi
    //
  }: {
    orderId: number
    orderStatus: string
    employeeId: number
    movementDate?: string
    targetTableId?: number // Yeni parametre eklendi
  }) {
    await axios.post(`${baseUrl}/Orders/movement`, {
      orderId,
      orderStatus,
      employeeId,
      movementDate: movementDate || new Date().toISOString(),
      targetTableId, // Yeni parametre eklendi
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
      // console.log('Sipariş oluşturuldu:', orderRes.data)
      const ConstorderId = Number(orderRes.data.orderId)
      // console.log('Sipariş oluşturuldu, orderId:', ConstorderId)

      // 2. Her ürün için detay kaydı oluştur
      // console.log('Detay ekleme başlıyor, items:', items, 'length:', items.length)
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
        // console.log('Detay için gönderilen veri:', detayPayload)
        try {
          await axios.post(`${baseUrl}/Orders/detail`, detayPayload)
        }
        catch (err: any) {
          console.error('DETAY KAYDI HATASI:', err?.response?.data || err)
          throw err
        }
      }

      // console.log('Tüm detaylar eklendi.')

      // 3. Hareket kaydı ekle
      await addOrderMovement({
        orderId: ConstorderId,
        orderStatus: orderData.status,
        employeeId: Number(waiterID),
        movementDate: new Date().toISOString(),
        targetTableId: orderData.tableID ? Number(orderData.tableID) : undefined,
      })
      // console.log('Hareket kaydı eklendi.')

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
      // console.log('fetchOrders response:', res)
      return { totalCount: res.totalCount }
    }
    catch (err: any) {
      error.value = err?.response?.data || err.message
      // console.error('fetchOrders error:', err)
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
      // console.log('siparişin ürünleri', order.value.items)
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

      // console.log('fetchLastOrdersPerTable params:', params)

      const res = await $fetch(`${baseUrl}/Orders/last-orders-per-table`, {
        params,
      }) as { data: any[], totalCount: number }

      // console.log('fetchLastOrdersPerTable response:', res)

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

      // console.log('Set totalOrderCount to:', totalOrderCount.value)
      // console.log('pagedOrders length:', pagedOrders.value.length)

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
        targetTableId: order.value?.tableId ? Number(order.value.tableId) : undefined,
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

  /**
   * Sipariş ürünlerini günceller (Geliştirilmiş hata yakalama ile)
   */
  async function updateOrderItems(orderId: string | number, newItems: any[], originalItems: any[] = []) {
    isLoading.value = true
    error.value = null

    try {
      console.log('🔍 updateOrderItems başlıyor:', { orderId, newItems, originalItems })

      // Veri validasyonu
      if (!orderId) {
        throw new Error('Order ID gerekli')
      }

      if (!Array.isArray(newItems)) {
        throw new TypeError('Yeni ürünler array olmalı')
      }

      if (!Array.isArray(originalItems)) {
        throw new TypeError('Orijinal ürünler array olmalı')
      }

      // baseUrl kontrolü
      if (!baseUrl) {
        throw new Error('API base URL tanımlı değil')
      }

      console.log('✅ Validasyonlar başarılı, API işlemleri başlıyor...')

      // Eski ürünlerin listesi
      const originalMap = new Map()
      originalItems.forEach((item) => {
        const key = item.id || item.productId
        if (key) {
          originalMap.set(key, item)
        }
      })

      const newMap = new Map()
      newItems.forEach((item) => {
        const key = item.id || item.productId
        if (key) {
          newMap.set(key, item)
        }
      })

      console.log('🔍 Maps oluşturuldu:', {
        originalMap: Array.from(originalMap.entries()),
        newMap: Array.from(newMap.entries()),
      })

      // 1. Silinen ürünleri bul ve sil
      for (const [productId] of originalMap) {
        if (!newMap.has(productId)) {
          console.log('🗑️ Ürün siliniyor:', { orderId, productId })

          try {
            const deleteResponse = await $fetch(`${baseUrl}/Orders/detail`, {
              method: 'DELETE',
              body: {
                orderId: Number(orderId),
                productId: Number(productId),
              },
            })
            console.log('✅ Ürün silindi:', { productId, response: deleteResponse })
          }
          catch (deleteError: any) {
            console.error('❌ Ürün silme hatası:', deleteError)
            throw new Error(`Ürün silinirken hata: ${deleteError.message}`)
          }
        }
      }

      // 2. Yeni eklenen veya güncellenen ürünleri işle
      for (const newItem of newItems) {
        const productId = newItem.id || newItem.productId
        const originalItem = originalMap.get(productId)

        if (!originalItem) {
        // Yeni ürün ekleme
          console.log('➕ Yeni ürün ekleniyor:', newItem)

          try {
            const addResponse = await $fetch(`${baseUrl}/Orders/detail`, {
              method: 'POST',
              body: {
                orderId: Number(orderId),
                productId: String(productId),
                quantity: newItem.quantity,
                insertDate: new Date().toISOString(),
              },
            })
            console.log('✅ Yeni ürün eklendi:', { productId, response: addResponse })
          }
          catch (addError: any) {
            console.error('❌ Ürün ekleme hatası:', addError)
            throw new Error(`Ürün eklenirken hata: ${addError.message}`)
          }
        }
        else if (originalItem.quantity !== newItem.quantity) {
        // Mevcut ürün miktarı güncelleme
          console.log('📝 Ürün miktarı güncelleniyor:', {
            productId,
            eskiMiktar: originalItem.quantity,
            yeniMiktar: newItem.quantity,
          })

          try {
            const updateResponse = await $fetch(`${baseUrl}/Orders/detail`, {
              method: 'PUT',
              body: {
                orderId: Number(orderId),
                productId: String(productId),
                quantity: newItem.quantity,
              },
            })
            console.log('✅ Ürün miktarı güncellendi:', { productId, response: updateResponse })
          }
          catch (updateError: any) {
            console.error('❌ Ürün güncelleme hatası:', updateError)
            throw new Error(`Ürün güncellenirken hata: ${updateError.message}`)
          }
        }
      }

      // 3. Sipariş detayını yeniden yükle
      console.log('🔄 Sipariş detayı yeniden yükleniyor...')
      await fetchOrderDetail(orderId)

      console.log('✅ Tüm sipariş ürün işlemleri başarıyla tamamlandı')
    }
    catch (err: any) {
      console.error('❌ updateOrderItems genel hatası:', err)
      const errorMessage = err?.data?.message || err?.response?.data?.message || err.message || 'Bilinmeyen hata'
      error.value = errorMessage
      throw new Error(errorMessage)
    }
    finally {
      isLoading.value = false
    }
  }

  /**
   * Siparişi başka masaya taşır - MoveOrderRequest interface kullanarak
   */
  async function moveOrderToTable(orderId: string | number, targetTableId: string | number): Promise<MoveOrderResponse> {
    isLoading.value = true
    error.value = null
    
    try {
      console.log('Masa taşıma işlemi başlıyor:', { orderId, targetTableId })
      
      const authStore = useAuthStore()
      
      // MoveOrderRequest interface'ini kullan
      const requestData: MoveOrderRequest = {
        orderId: Number(orderId),
        targetTableId: Number(targetTableId),
        employeeId: Number(authStore.userInfo.id)
      }

      console.log("API'ye gönderilen veri:", requestData)

      // Backend API endpoint'ini düzelt - /Orders/move-order olmalı
      const response = await axios.post<MoveOrderResponse>(`${baseUrl}/Table/move-order`, requestData)

      console.log('Masa taşıma başarılı:', response.data)

      // Hareket kaydı ekle - targetTableId ile birlikte
      await addOrderMovement({
        orderId: Number(orderId),
        orderStatus: 'Masa Değiştirildi',
        employeeId: Number(authStore.userInfo.id),
        targetTableId: Number(targetTableId)
      })

      // Siparişleri yeniden yükle
      await fetchOrders()

      return response.data
    }
    catch (err: any) {
      const errorMessage = err?.response?.data?.message || 
                          err?.response?.data || 
                          err?.message || 
                          'Masa taşıma işlemi başarısız oldu.'
      error.value = errorMessage
      console.error('Masa taşıma hatası:', err)
      throw new Error(errorMessage)
    }
    finally {
      isLoading.value = false
    }
  }

  return {
    fetchOrders,
    moveOrderToTable,
    fetchLastOrdersPerTable,
    orders,
    pagedOrders,
    totalOrderCount,
    createOrderWithDetails,
    isLoading,
    error,
    fetchOrderDetail,
    order,
    updateOrderStatus,
    addOrderMovement,
    updateOrderItems,
  }
})
