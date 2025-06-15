<script setup lang="ts">
defineProps<{
  product: {
    id: number
    name: string
    description: string
    salePrice: number
    image: string
  }
}>()
</script>

<template>
  <div class="inter-tight flex shadow-md rounded-3xl overflow-hidden p-6 bg-white">
    <img :src="product.image" :alt="product.name" class="w-42 h-37 mr-4">
    <div class="flex-1">
      <h3 class="text-xl  mb-2">
        {{ product.name }}
        {{ product.id }}
      </h3>
      <p class="text-gray-600 mb-2">
        {{ product.description }}
      </p>
      <p class="text-lg font-bold">
        {{ product.salePrice }} TL
      </p>
    </div>
  </div>
</template>

<style scoped>
/* Tailwind CSS kullanıldığı için özel CSS gerekmez */
</style>
