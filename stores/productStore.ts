import axios from 'axios'
import { defineStore } from 'pinia'

export const useProductStore = defineStore('product', () => {
  const products = ref<Array<any>>([]) // Ürünleri saklamak için bir reaktif değişken
  const baseUrl = useRuntimeConfig().public.apiBase
  const fetchProducts = async () => {
    try {
      const response = await axios.get(`${baseUrl}/products`) // API'den ürünleri çek
      products.value = response.data.map((product: { no: string }) => ({
        ...product,
        image: `/img/products/${product.no}.png` || `/img/products/default.png`,

      }))
      console.log('Ürünler başarıyla alındı:', products.value)
    }
    catch (error) {
      console.error('Ürünler alınırken bir hata oluştu:', error)
    }
  }

  const filterProductsBySpec1 = (spec1: string) => {
    return products.value.filter(product => product.spec1 === spec1) // spec1'e göre filtrele
  }

  return { products, fetchProducts, filterProductsBySpec1 } // Değişkenleri ve fonksiyonları döndür
})
