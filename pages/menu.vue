<script setup lang="ts">
import { Icon } from '@iconify/vue'
import { computed, onMounted, ref } from 'vue'
import ProductCard from '~/components/ui/product_card.vue'
import { useProductStore } from '@/stores/productStore'

const categories = [
  { key: 'Pizza', icon: 'ph:pizza-bold', label: 'Pizzalar', style: 'color: #d26b13' },
  { key: 'Hamburger', icon: 'lucide:hamburger', label: 'Hamburgerler' },
  { key: 'Kızartma', icon: 'hugeicons:french-fries-02', label: 'Aperatifler' },
  { key: 'Salata', icon: 'lucide:salad', label: 'Salatalar' },
  { key: 'İçecek', icon: 'cil:drink', label: 'İçecekler' },
  { key: 'Tatlı', icon: 'ep:dessert', label: 'Tatlılar' },
]

const selectedCategory = ref(categories[0].key)

const productStore = useProductStore()
const { products, fetchProducts } = productStore

onMounted(() => {
  fetchProducts()
})

const filteredProducts = computed(() =>
  products.filter(p =>
    // Burada kategoriye göre filtreleme yapılıyor, kendi veri yapına göre düzenle
    selectedCategory.value === 'Pizza'
      ? p.group1 === 'Pizza'
      : selectedCategory.value === 'Hamburger'
        ? p.group1 === 'Hamburger'
        : selectedCategory.value === 'Kızartma'
          ? p.group1 === 'Kızartma'
          : selectedCategory.value === 'Salata'
            ? p.group1 === 'Salata'
            : selectedCategory.value === 'İçecek'
              ? p.spec1 === 'İçecek'
              : selectedCategory.value === 'Tatlı'
                ? p.spec1 === 'Tatlı'

                : true, // Diğer kategori için tüm ürünler

  ),
)
</script>

<template>
  <div class=" mx-auto rounded-3xl p-8">
    <h3 class="text-orange-600 font-serif text-center text-5xl font-bold mb-8 tracking-tight">
      MENÜ
    </h3>
    <!-- Kategori Sekmeleri -->
    <div class="flex justify-center gap-8 mb-8">
      <button
        v-for="cat in categories"
        :key="cat.key"
        class="flex flex-col items-center px-4 py-2 border-b-2 transition-colors duration-200"
        :class="selectedCategory === cat.key
          ? 'text-orange-600 border-orange-600 font-semibold'
          : 'text-gray-500 border-transparent hover:text-orange-500'"
        @click="selectedCategory = cat.key"
      >
        <Icon :icon="cat.icon" class="text-2xl mb-1" /> <!-- Burası değişti -->
        <span class="text-base">{{ cat.label }}</span>
      </button>
    </div>

    <h2 class="text-center text-3xl font-bold text-orange-600 mb-6">
      {{ categories.find(c => c.key === selectedCategory)?.label }}
    </h2>

    <div class="flex flex-col gap-6 max-w-4xl mx-auto">
      <ProductCard
        v-for="product in filteredProducts"
        :key="product.uid"
        :product="product"
      />
    </div>
  </div>
</template>
