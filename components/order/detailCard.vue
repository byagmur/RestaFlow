<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { OrderItem } from '~/types'
import { computed, ref, watch } from 'vue'
import ProductSelector from '@/components/ui/selector/ProductSelector.vue'
import { useOrderStore } from '@/stores/order/orderStore'

const props = defineProps<{ order: any }>()
const emit = defineEmits(['close', 'updated'])

const orderStore = useOrderStore()
const loading = ref(false)
const success = ref(false)
const error = ref('')
const isEditing = ref(false)
const editableItems = ref<OrderItem[]>([])
const originalItems = ref<OrderItem[]>([]) // Orijinal ürünleri sakla

watch(() => isEditing.value, (newVal) => {
  if (newVal && props.order?.items) {
    editableItems.value = JSON.parse(JSON.stringify(props.order.items))
    originalItems.value = JSON.parse(JSON.stringify(props.order.items)) // Orijinal verileri sakla
  }
})

function formatDate(date: string | Date): string {
  const d = new Date(date)
  return d.toLocaleDateString('tr-TR', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

const columns: TableColumn<OrderItem>[] = [
  { accessorKey: 'name', header: 'Ürün Adı', cell: info => info.getValue() },
  { accessorKey: 'quantity', header: 'Adet', cell: info => info.getValue() },
  { accessorKey: 'price', header: 'Fiyat', cell: info => `${info.getValue()}₺` },
  {
    accessorKey: 'actions',
    header: 'İşlemler',
    cell: () => '',
  },
]

// Düzenleme modu için farklı sütunlar
const editColumns: TableColumn<OrderItem>[] = [
  { accessorKey: 'name', header: 'Ürün Adı', cell: info => info.getValue() },
  { accessorKey: 'quantity', header: 'Adet', cell: info => info.getValue() },
  { accessorKey: 'price', header: 'Fiyat', cell: info => `${info.getValue()}₺` },
  {
    accessorKey: 'total',
    header: 'Toplam',
    cell: (info) => {
      const row = info.row.original
      return `${(row.quantity * row.price).toFixed(2)}₺`
    },
  },
  {
    accessorKey: 'actions',
    header: 'İşlemler',
    cell: () => '',
  },
]

const uniqueItems = computed(() => {
  if (!props.order?.items)
    return []
  const seen = new Set()
  return props.order.items.filter((item: OrderItem) => {
    if (seen.has(item.name))
      return false
    seen.add(item.name)
    return true
  })
})

// Düzenlenmiş toplam tutar
const editedTotal = computed(() => {
  return editableItems.value.reduce((total, item) => {
    return total + (item.quantity * item.price)
  }, 0)
})

// Ürün miktarını artır
function increaseQuantity(index: number) {
  editableItems.value[index].quantity += 1
}

// Ürün miktarını azalt
function decreaseQuantity(index: number) {
  if (editableItems.value[index].quantity > 1) {
    editableItems.value[index].quantity -= 1
  }
}

// Ürünü sil (UI'dan kaldır)
function removeItem(index: number) {
  if (editableItems.value.length > 1) {
    editableItems.value.splice(index, 1)
  }
  else {
    error.value = 'Sipariş en az bir ürün içermelidir!'
    setTimeout(() => error.value = '', 3000)
  }
}

// Değişiklikleri kaydet
async function saveChanges() {
  loading.value = true
  error.value = ''
  success.value = false

  try {
    console.log('Kaydedilecek veriler:', {
      orderId: props.order.uid,
      originalItems: originalItems.value,
      editableItems: editableItems.value,
    })

    // Orijinal ve yeni items'ı gönder
    await orderStore.updateOrderItems(props.order.uid, editableItems.value, originalItems.value)

    success.value = true
    isEditing.value = false
    emit('updated')
    setTimeout(() => success.value = false, 3000)
  }
  catch (e: any) {
    console.error('saveChanges hatası:', e)
    error.value = e.message || 'Sipariş güncellenirken hata oluştu'
  }
  finally {
    loading.value = false
  }
}

// Değişiklikleri iptal et
function cancelEdit() {
  isEditing.value = false
  editableItems.value = []
  error.value = ''
}

async function handleServeOrder() {
  loading.value = true
  error.value = ''
  success.value = false
  try {
    await orderStore.updateOrderStatus(Number(props.order.uid), 'Servis Edildi')
    success.value = true
    emit('updated')
  }
  catch (e: any) {
    error.value = e || 'Bir hata oluştu'
  }
  finally {
    loading.value = false
  }
}

// Yeni ürün ekleme
function addNewProduct(productData: any) {
  console.log('Yeni ürün ekleniyor:', productData)

  const existingIndex = editableItems.value.findIndex(
    item => (item.id || item.productId || item.no) === productData.id,
  )

  if (existingIndex !== -1) {
    editableItems.value[existingIndex].quantity += productData.quantity
    console.log('Mevcut ürün miktarı artırıldı:', editableItems.value[existingIndex])
  }
  else {
    // editableItems.value.push({
    //   id: productData.id,
    //   productId: productData.id,
    //   no: productData.no,
    //   name: productData.name,
    //   price: productData.price,
    //   quantity: productData.quantity,
    // })

    const newOrderItem: OrderItem = {
      id: productData.id,
      productId: productData.id,
      no: productData.no,
      name: productData.name,
      price: productData.price,
      quantity: productData.quantity,
      total: productData.price * productData.quantity,
    }
    editableItems.value.push(newOrderItem)
    console.log('Yeni ürün eklendi:', productData)
  }
}
</script>

<template>
  <div
    v-if="order"
    class="fixed inset-0 z-50 flex items-center justify-center bg-gray-100/90 backdrop-blur-sm"
  >
    <UCard
      class="relative w-full max-w-6xl mx-4 rounded-2xl shadow-xl border border-gray-200 bg-white animate-fade-in max-h-[90vh] overflow-y-auto"
    >
      <template #header>
        <div class="flex items-center justify-between border-b border-gray-200 px-6 py-4">
          <h2 class="text-2xl font-bold text-gray-800">
            #{{ order.uid }} Sipariş Detayları
          </h2>
          <div class="flex items-center gap-2">
            <UButton
              v-if="!isEditing && ['Açıldı', 'Hazırlanıyor', 'Beklemede'].includes(order.status)"
              color="blue"
              variant="outline"
              class="!text-blue-600 text-lg rounded-lg px-4 py-2 transition-colors hover:!bg-blue-200 bg-blue-100"
              @click="isEditing = true"
            >
              <Icon name="heroicons:pencil-square" class="w-4 h-4 mr-1" />
              Düzenle
            </UButton>

            <!-- Servis et butonu -->
            <UButton
              v-if="!isEditing"
              :color="order.status === 'Tamamlandı' ? 'primary' : 'gray'"
              class="!text-white text-lg rounded-lg px-4 py-2 transition-colors"
              :class=" [
                order.status === 'Tamamlandı'
                  ? '!bg-orange-400 hover:!bg-orange-500 cursor-pointer'
                  : '!bg-gray-300 cursor-not-allowed !text-gray-500',
              ]"
              :disabled="order.status !== 'Tamamlandı'"
              :loading="loading"
              @click="order.status === 'Tamamlandı' && handleServeOrder()"
            >
              Servis Et
            </UButton>

            <!-- Düzenleme modu butonları -->
            <template v-if="isEditing">
              <UButton
                color="green"
                :loading="loading"
                class="!text-white text-lg rounded-lg px-4 py-2 transition-colors hover:!bg-green-600 bg-green-500"
                @click="saveChanges"
              >
                <Icon name="heroicons:check" class="w-4 h-4 mr-1" />
                Kaydet
              </UButton>
              <UButton
                color="gray"
                variant="outline"
                class="!text-white text-lg rounded-lg px-4 py-2 transition-colors hover:!bg-red-500 bg-red-400"
                @click="cancelEdit"
              >
                <Icon name="heroicons:x-mark" class="w-4 h-4 mr-1" />
                İptal
              </UButton>
            </template>

            <button
              class="text-gray-400 hover:text-red-500 !text-4xl font-bold ml-2"
              aria-label="Kapat"
              @click="$emit('close')"
            >
              &times;
            </button>
          </div>
        </div>

        <!-- Bildirim mesajları -->
        <div v-if="success || error" class="flex justify-center px-6 pt-2">
          <div v-if="success" class="text-green-600 bg-green-50 px-3 py-1 rounded">
            {{ isEditing ? 'Sipariş başarıyla güncellendi!' : 'Sipariş durumu güncellendi!' }}
          </div>
          <div v-if="error" class="text-red-600 bg-red-50 px-3 py-1 rounded">
            {{ error }}
          </div>
        </div>
      </template>

      <div class="px-6 pb-4 space-y-3 text-gray-700 text-lg leading-relaxed">
        <div>Masa: {{ order.tableId }}</div>
        <div>Tarih: {{ formatDate(order.orderDate) }}</div>
        <div>Durum: {{ order.status }}</div>
        <div>
          Tutar:
          {{ isEditing ? editedTotal.toFixed(2) : order.totalPrice }}₺
        </div>
        <div v-if="order.note">
          Not: {{ order.note }}
        </div>
      </div>

      <!-- Ürünler tablosu -->
      <div v-if="order.items && order.items.length" class="mt-4 px-4 pb-6">
        <!-- Yeni Ürün Ekleme (Sadece düzenleme modunda) -->
        <div v-if="isEditing" class="mb-6">
          <ProductSelector @product-selected="addNewProduct" />
        </div>

        <div class="border border-gray-200 rounded-lg overflow-hidden">
          <!-- Normal görünüm tablosu -->
          <UTable
            v-if="!isEditing"
            :data="uniqueItems"
            :columns="columns"
            :ui="{
              base: 'min-w-full text-gray-600 text-base',
              th: 'bg-gray-100 text-gray-600 uppercase text-sm tracking-wider px-4 py-2 border-b border-gray-100',
              td: 'px-4 py-3 border-b border-gray-200',
              tr: 'hover:bg-gray-50 transition',
            }"
          />

          <!-- Düzenleme görünümü tablosu -->
          <div v-else class="overflow-x-auto">
            <table class="min-w-full text-gray-600 text-base">
              <thead class="bg-gray-100">
                <tr>
                  <th class="text-gray-600 uppercase text-sm tracking-wider px-4 py-2 border-b border-gray-100 text-left">
                    Ürün Adı
                  </th>
                  <th class="text-gray-600 uppercase text-sm tracking-wider px-4 py-2 border-b border-gray-100 text-center">
                    Adet
                  </th>
                  <th class="text-gray-600 uppercase text-sm tracking-wider px-4 py-2 border-b border-gray-100 text-right">
                    Fiyat
                  </th>
                  <th class="text-gray-600 uppercase text-sm tracking-wider px-4 py-2 border-b border-gray-100 text-right">
                    Toplam
                  </th>
                  <th class="text-gray-600 uppercase text-sm tracking-wider px-4 py-2 border-b border-gray-100 text-center">
                    İşlemler
                  </th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(item, index) in editableItems" :key="index" class="hover:bg-gray-50 transition">
                  <td class="px-4 py-3 border-b border-gray-200">
                    <div class="flex items-center gap-2">
                      <span>{{ item.name }}</span>
                      <span
                        v-if="!originalItems.find(orig => (orig.id || orig.productId || orig.no) === (item.id || item.productId || item.no))"
                        class="px-2 py-1 text-xs bg-green-100 text-green-700 rounded-full"
                      >
                        Yeni
                      </span>
                    </div>
                  </td>
                  <td class="px-4 py-3 border-b border-gray-200 text-center">
                    <div class="flex items-center justify-center gap-2">
                      <button
                        class="w-8 h-8 bg-red-100 text-red-600 rounded-full hover:bg-red-200 transition disabled:opacity-50"
                        :disabled="item.quantity <= 1"
                        @click="decreaseQuantity(index)"
                      >
                        <Icon name="heroicons:minus" class="w-4 h-4 mx-auto" />
                      </button>
                      <span class="w-8 text-center font-medium">{{ item.quantity }}</span>
                      <button
                        class="w-8 h-8 bg-green-100 text-green-600 rounded-full hover:bg-green-200 transition"
                        @click="increaseQuantity(index)"
                      >
                        <Icon name="heroicons:plus" class="w-4 h-4 mx-auto" />
                      </button>
                    </div>
                  </td>
                  <td class="px-4 py-3 border-b border-gray-200 text-right">
                    {{ item.price }}₺
                  </td>
                  <td class="px-4 py-3 border-b border-gray-200 text-right font-medium">
                    {{ (item.quantity * item.price).toFixed(2) }}₺
                  </td>
                  <td class="px-4 py-3 border-b border-gray-200 text-center">
                    <button
                      class="p-2 text-red-500 hover:text-red-700 hover:bg-red-50 rounded transition"
                      :disabled="editableItems.length <= 1"
                      :title="editableItems.length <= 1 ? 'Sipariş en az bir ürün içermelidir' : 'Ürünü sil'"
                      @click="removeItem(index)"
                    >
                      <Icon name="heroicons:trash" class="w-4 h-4" />
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Düzenleme modu toplam bilgisi -->
        <div v-if="isEditing" class="mt-4 p-4 bg-blue-50 rounded-lg">
          <div class="flex justify-between items-center">
            <span class="text-sm text-gray-600">Toplam Tutar:</span>
            <span class="text-lg font-bold text-blue-600">{{ editedTotal.toFixed(2) }}₺</span>
          </div>
          <div v-if="editedTotal !== order.totalPrice" class="flex justify-between items-center mt-1">
            <span class="text-xs text-gray-500">Fark:</span>
            <span class="text-sm" :class="editedTotal > order.totalPrice ? 'text-red-600' : 'text-green-600'">
              {{ (editedTotal - order.totalPrice > 0 ? '+' : '') }}{{ (editedTotal - order.totalPrice).toFixed(2) }}₺
            </span>
          </div>
        </div>
      </div>
    </UCard>
  </div>
</template>
