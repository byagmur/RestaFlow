<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import SelectBox from '@/components/ui/combobox/SelectBox.vue'
import { useProductStore } from '@/stores/productStore'

const emit = defineEmits<{
  productSelected: [product: any]
}>()

const productStore = useProductStore()
const selectedSpec1 = ref('')
const selectedGroup1 = ref('')
const selectedGroup2 = ref('')
const selectedProduct = ref('')
const quantity = ref(1)

// Kategoriler
const spec1Options = computed(() => {
  const specs = [...new Set(productStore.products.map(p => p.spec1).filter(Boolean))]
  return specs.map(spec => ({ label: spec, value: spec }))
})

// Group1 seçenekleri (Spec1'e göre filtrelenir)
const group1Options = computed(() => {
  if (!selectedSpec1.value)
    return []

  const groups = [...new Set(
    productStore.products
      .filter(p => p.spec1 === selectedSpec1.value)
      .map(p => p.group1)
      .filter(Boolean),
  )]

  return groups.map(group => ({ label: group, value: group }))
})

// Group2 seçenekleri (Spec1 ve Group1'e göre filtrelenir)
const group2Options = computed(() => {
  if (!selectedSpec1.value || !selectedGroup1.value)
    return []

  const groups = [...new Set(
    productStore.products
      .filter(p => p.spec1 === selectedSpec1.value && p.group1 === selectedGroup1.value)
      .map(p => p.group2)
      .filter(Boolean),
  )]

  return groups.map(group => ({ label: group, value: group }))
})

// Ürün seçenekleri (tüm filtrelere göre)
const productOptions = computed(() => {
  const filteredProducts = productStore.products.filter((p) => {
    if (selectedSpec1.value && p.spec1 !== selectedSpec1.value)
      return false
    if (selectedGroup1.value && p.group1 !== selectedGroup1.value)
      return false
    if (selectedGroup2.value && p.group2 !== selectedGroup2.value)
      return false
    return true
  })

  return filteredProducts.map(product => ({
    label: `${product.name} - ${product.salePrice}₺`,
    value: product.no,
    product,
  }))
})

// Seçenekler değiştiğinde alt seçenekleri sıfırla
watch(() => selectedSpec1.value, () => {
  selectedGroup1.value = ''
  selectedGroup2.value = ''
  selectedProduct.value = ''
})

watch(() => selectedGroup1.value, () => {
  selectedGroup2.value = ''
  selectedProduct.value = ''
})

watch(() => selectedGroup2.value, () => {
  selectedProduct.value = ''
})

// Ürün ekle
function addProduct() {
  if (!selectedProduct.value || quantity.value <= 0)
    return

  const productOption = productOptions.value.find(opt => opt.value === selectedProduct.value)
  if (!productOption)
    return

  const productData = {
    id: productOption.product.no,
    productId: productOption.product.no,
    no: productOption.product.no,
    name: productOption.product.name,
    price: productOption.product.salePrice,
    quantity: quantity.value,
  }

  console.log('Ürün ekleniyor:', productData)
  emit('productSelected', productData)

  // Formu sıfırla
  resetForm()
}

function resetForm() {
  selectedSpec1.value = ''
  selectedGroup1.value = ''
  selectedGroup2.value = ''
  selectedProduct.value = ''
  quantity.value = 1
}

// Component mount olduğunda ürünleri yükle
onMounted(async () => {
  console.log('ProductSelector mount oldu')
  if (productStore.products.length === 0) {
    console.log('Ürünler yükleniyor...')
    await productStore.fetchProducts()
    console.log('Ürünler yüklendi:', productStore.products.length)
  }
})
</script>

<template>
  <div class="space-y-4 p-4 border border-gray-200 rounded-lg bg-gray-50">
    <h4 class="font-medium text-gray-800">
      Yeni Ürün Ekle
    </h4>

    <!-- Loading state -->
    <div v-if="productStore.isLoading" class="text-center py-4">
      <div class="inline-flex items-center gap-2">
        <svg class="animate-spin h-4 w-4 text-blue-600" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
        </svg>
        <span class="text-sm text-gray-600">Ürünler yükleniyor...</span>
      </div>
    </div>

    <!-- Form (sadece ürünler yüklendiyse göster) -->
    <div v-else-if="productStore.products.length > 0">
      <!-- Spec1 (Ana Kategori) -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Kategori</label>
          <select
            v-model="selectedSpec1"
            class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">
              Kategori seçin...
            </option>
            <option v-for="option in spec1Options" :key="option.value" :value="option.value">
              {{ option.label }}
            </option>
          </select>
        </div>

        <SelectBox
          v-model="selectedGroup1"
          :options="group1Options"
          label="Alt Kategori"
          placeholder="Alt kategori seçin..."
          :disabled="!selectedSpec1"
        />
        <SelectBox
          v-model="selectedGroup2"
          :options="group2Options"
          label="Grup"
          placeholder="Grup seçin..."
          :disabled="!selectedGroup1"
        />
        <SelectBox
          v-model="selectedProduct"
          :options="productOptions"
          label="Ürün"
          placeholder="Ürün seçin..."
          :disabled="productOptions.length === 0"
          required
        />
      </div>

      <!-- Miktar ve Ekle Butonu -->
      <div class="flex items-end gap-3">
        <div class="flex-shrink-0">
          <label class="block text-sm font-medium text-gray-700 mb-1">Adet</label>
          <div class="flex items-center gap-1">
            <button
              type="button"
              class="w-8 h-8 bg-red-100 text-red-600 rounded-md hover:bg-red-200 transition disabled:opacity-50"
              :disabled="quantity <= 1"
              @click="quantity = Math.max(1, quantity - 1)"
            >
              -
            </button>
            <input
              v-model.number="quantity"
              type="number"
              min="1"
              max="99"
              class="w-16 text-center px-2 py-1 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
            <button
              type="button"
              class="w-8 h-8 bg-green-100 text-green-600 rounded-md hover:bg-green-200 transition"
              @click="quantity += 1"
            >
              +
            </button>
          </div>
        </div>

        <button
          type="button"
          :disabled="!selectedProduct || quantity <= 0"
          class="flex-shrink-0 inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition"
          @click="addProduct"
        >
          <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
          </svg>
          Ürün Ekle
        </button>

        <button
          type="button"
          class="flex-shrink-0 inline-flex items-center px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 transition"
          @click="resetForm"
        >
          <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Temizle
        </button>
      </div>
    </div>

    <!-- Ürün yok durumu -->
    <div v-else-if="!productStore.isLoading" class="text-center py-4">
      <p class="text-gray-500">
        Ürün bulunamadı
      </p>
      <button
        class="mt-2 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
        @click="productStore.fetchProducts()"
      >
        Tekrar Yükle
      </button>
    </div>
  </div>
</template>
