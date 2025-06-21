<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useOrderStore } from '~/stores/order/orderStore'
import { useProductStore } from '~/stores/productStore'
import { useTableStore } from '~/stores/tableStore'
import { useAuthStore } from '~/stores/auth/authStore'

definePageMeta({
  middleware: 'auth',
  ssr: false,
})

const categories = ref([
  { name: 'Çorba', icon: 'lsicon:soup-filled' },
  { name: 'Ana Yemek', icon: 'fluent:food-16-filled' },
  { name: 'İçecek', icon: 'tdesign:drink-filled' },
  { name: 'Tatlı', icon: 'ep:dessert' },
])

const router = useRouter()
const authStore = useAuthStore()
const productStore = useProductStore()
const tableStore = useTableStore()
const orderStore = useOrderStore()
const selectedCategory = ref<string | null>(null)
const selectedFoods = ref<any[]>([])
const selectedTable = ref<number | null>(null)
const waiterID = authStore.userInfo.id // veya backend'in beklediği id alanı

onMounted(() => {
  productStore.fetchProducts()
  tableStore.fetchTables()
})

const filteredFoods = computed(() => {
  if (!selectedCategory.value)
    return []
  return productStore.products.filter(
    food => food.spec1 === selectedCategory.value,
  )
})

const totalPrice = computed(() => {
  return selectedFoods.value.reduce(
    (sum, food) => sum + (food.salePrice || 0) * (food._orderCount || 1),
    0,
  )
})

function addFoodToOrder(food: any) {
  const existing = selectedFoods.value.find(f => f.no === food.no)
  if (existing) {
    if (existing.quantity < food.quantity) { // stok sınırı
      existing._orderCount = (existing._orderCount || 1) + 1
    }
  }
  else {
    selectedFoods.value.push({ ...food, _orderCount: 1 })
  }
}

function removeFoodFromOrder(food: any) {
  selectedFoods.value = selectedFoods.value.filter(item => item !== food)
}

function increaseFoodCount(food: any) {
  if (food._orderCount < food.quantity) {
    food._orderCount++
  }
}

function decreaseFoodCount(food: any) {
  if (food._orderCount > 1) {
    food._orderCount--
  }
  else {
    removeFoodFromOrder(food)
  }
}

async function placeOrder() {
  console.log('placeOrder ÇAĞRILDI')
  if (!selectedTable.value || selectedFoods.value.length === 0) {
    alert('Lütfen masa ve en az bir ürün seçin.')
    return
  }

  const orderData = {
    status: 'Açıldı', // veya backend'in beklediği başka bir değer
    tableID: selectedTable.value,
    waiterID, // giriş yapan kullanıcının id'si
    orderDate: new Date().toISOString(),
    totalPrice: totalPrice.value,
    note: '',
  }

  try {
    await orderStore.createOrderWithDetails(orderData, selectedFoods.value)
    console.log('Sipariş ve ürünler başarıyla oluşturuldu!')
    selectedTable.value = null
    selectedFoods.value = []
    selectedCategory.value = null
  }
  catch (err) {
    console.error('Sipariş oluşturulamadı:', orderData)
    console.error('Seçilen Yemekler:', selectedFoods.value)
    console.error('Hata:', err)
    alert(err || 'Sipariş oluşturulamadı.')
  }
}

function selectCategory(categoryName: string) {
  selectedCategory.value = categoryName
}
</script>

<template>
  <NuxtLayout>
    <template #content>
      <div class="flex h-screen">
        <!-- Masa Seçim Card'ı -->
        <div
          v-if="!selectedTable"
          class="fixed inset-0 flex items-center justify-center bg-gray-300 bg-opacity-30 z-50"
        >
          <div class="bg-white rounded-xl shadow-xl p-8 min-w-[350px]">
            <UButton
              class="text-3xl top-4 left-4 text-gray-500 hover:text-gray-900 !bg-transparent transition-colors duration-200"
              icon="tabler:arrow-back-up"
              title="Geri"
              @click="router.back()"
            />  

            <h2 class="text-xl font-semibold text-gray-800 mb-4 text-center">
              Masa Seçiniz
            </h2>
            <div class="grid grid-cols-4 gap-6">
              <UCard
                v-for="table in tableStore.tables"
                :key="table.uid"
                :ui="{
                  body: { padding: 'p-0' },
                  rounded: 'rounded-xl',
                }"
                class="w-28 h-28 flex flex-col items-center justify-center cursor-pointer transition-all duration-200 font-semibold select-none"
                :class="{
                  'bg-green-500 text-white hover:scale-105': !table.status,
                  'bg-gray-300 text-gray-500 cursor-not-allowed': table.status === 'Dolu',
                  'bg-red-500 text-white cursor-not-allowed': table.status === 'İptal Edildi',
                  'bg-yellow-400 text-white cursor-not-allowed': table.status === 'Servis Edildi',
                  'bg-orange-400 text-white cursor-not-allowed': table.status === 'Tamamlandı',
                }"
                :disabled="['Dolu', 'İptal Edildi', 'Servis Edildi', 'Tamamlandı'].includes(table.status)"
                @click="!['Dolu', 'İptal Edildi', 'Servis Edildi', 'Tamamlandı'].includes(table.status) && (selectedTable = table.uid)"
              >
                <div class="flex flex-col items-center justify-center h-full w-full">
                  <span class="text-lg font-bold">{{ table.tableName }}</span>
                  <span v-if="table.status" class="block text-xs mt-1">{{ table.status }}</span>
                  <span v-else class="block text-xs mt-1">Boş</span>
                </div>
              </UCard>
            </div>
          </div>
        </div>

        <!-- Sipariş Ekleme ve Adisyon Özeti -->
        <div v-else class="flex w-full">
          <!-- Sol Panel: Sipariş Ekleme -->
          <div class="w-2/3 bg-white shadow-md p-6 rounded-l-3xl overflow-y-auto">
            <h2 class="text-2xl font-semibold text-gray-800 mb-4">
              Sipariş Ekle
            </h2>
            <!-- Kategoriler -->
            <div class="mb-6">
              <h3 class="text-lg text-gray-800 font-medium mb-4">
                Kategori Seç
              </h3>
              <div class="flex space-x-4 overflow-x-auto pb-4">
                <button
                  v-for="category in categories"
                  :key="category.name"
                  :class="{
                    'bg-blue-600 text-white': selectedCategory === category.name,
                    'bg-gray-100 text-gray-700': selectedCategory !== category.name,
                  }"
                  class="p-3 rounded-full flex flex-col items-center space-y-2 transition-all duration-300 transform hover:scale-105"
                  @click="selectCategory(category.name)"
                >
                  <Icon :name="category.icon" class="text-2xl" />
                  <span class="text-xs sm:text-md pb-1">{{ category.name }}</span>
                </button>
              </div>
            </div>
            <!-- Kategoriye ait Yemekler -->
            <div v-if="selectedCategory" class="mb-6">
              <h3 class="text-lg text-gray-800 font-medium mb-4">
                Yemekler
              </h3>
              <div class="grid grid-cols-2 gap-3">
                <div
                  v-for="food in filteredFoods"
                  :key="food.id"
                  class="bg-gray-50 rounded-lg p-3 flex justify-between items-center border border-gray-100"
                >
                  <span
                    class="text-sm"
                    :class="[food.quantity === 0 ? '!text-red-600 font-bold' : 'text-gray-800']"
                  >
                    {{ food.name }}
                  </span>
                  <span class="text-gray-800 text-sm ml-2">{{ food.salePrice }} ₺</span>
                  <button
                    class="px-4 py-2 rounded-lg font-semibold text-md"
                    :class="food.quantity === 0 ? 'bg-red-500 text-red-100 cursor-not-allowed' : 'bg-blue-600 text-white hover:bg-blue-700'"
                    :disabled="food.quantity === 0"
                    @click="addFoodToOrder(food)"
                  >
                    Ekle
                  </button>
                </div>
              </div>
            </div>
            <!-- Seçilen Yemekler -->
            <div class="mb-6">
              <h3 class="text-lg text-gray-800 font-medium mb-4">
                Seçilen Yemekler
              </h3>
              <ul class="space-y-2">
                <li
                  v-for="(food, index) in selectedFoods"
                  :key="index"
                  class="bg-gray-50 rounded-lg p-3 flex justify-between items-center border border-gray-100"
                >
                  <span class="text-gray-800 text-sm">{{ food.name }}</span>
                  <div class="flex items-center space-x-2">
                    <button
                      class="px-2 py-1 bg-gray-200 rounded text-lg font-bold"
                      @click="decreaseFoodCount(food)"
                    >
                      -
                    </button>
                    <span class="text-gray-800">{{ food._orderCount }}</span>
                    <button
                      class="px-2 py-1 bg-gray-200 rounded text-lg font-bold"
                      :disabled="food._orderCount >= food.quantity"
                      @click="increaseFoodCount(food)"
                    >
                      +
                    </button>
                  </div>
                  <button
                    class="text-red-500 hover:text-red-700 font-semibold ml-2"
                    @click="removeFoodFromOrder(food)"
                  >
                    Kaldır
                  </button>
                </li>
              </ul>
            </div>
            <!-- Sipariş Ver Butonu -->
            <div class="text-center">
              <button
                class="text-md bg-blue-600 text-white font-semibold py-3 px-8 rounded-lg hover:bg-blue-700 transition duration-200"
                @click="placeOrder"
              >
                Siparişi Ver
              </button>
            </div>
          </div>
          <!-- Sağ Panel: Adisyon Özeti -->
          <div class="w-1/3 bg-gray-100 p-6 rounded-r-3xl overflow-y-auto">
            <h2 class="text-2xl font-semibold text-gray-800 mb-4">
              Adisyon Özeti
            </h2>
            <div class="mb-4">
              <h3 class="text-gray-800 text-lg">
                Masa: {{ tableStore.tables.find(t => t.uid === selectedTable)?.tableName || selectedTable }}
              </h3>
            </div>
            <div class="space-y-4">
              <div v-for="(food, index) in selectedFoods" :key="index" class="flex justify-between items-center bg-white p-3 rounded-lg shadow-sm">
                <span class="text-gray-800 text-sm">{{ food.name }}</span>
                <span class="text-gray-800 text-sm">{{ food.salePrice }} ₺</span>
              </div>
            </div>
            <div class="mt-4 border-t border-gray-300 pt-4">
              <div class="flex justify-between items-center">
                <span class="text-gray-800 font-semibold">Toplam:</span>
                <span class="text-blue-600 font-semibold">{{ totalPrice }} ₺</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </NuxtLayout>
</template>

<style scoped>
/* Açık tema renk özellikleri */
.bg-primary {
  background-color: #4991c2; /* Açık mavi - Steel Blue */
}

.bg-primary-dark {
  background-color: #3a6d8c; /* Koyu mavi - Steel Blue'nun koyu versiyonu */
}

/* Yazı renkleri */
.text-gray-800 {
  color: #1f2937;
}

.text-gray-700 {
  color: #374151;
}

.text-gray-600 {
  color: #4b5563;
}

.text-blue-600 {
  color: #4991c2; /* Steel Blue - Koyu temanızdaki yeşil yerine */
}

.text-sm {
  font-size: 0.875rem;
}
</style>
