import axios from 'axios'
import { defineStore } from 'pinia'

export const useTableStore = defineStore('table', () => {
  const tables = ref<Array<any>>([])
  const baseUrl = useRuntimeConfig().public.apiBase
  const fetchTables = async () => {
    try {
      const response = await axios.get(`${baseUrl}/table`)
      tables.value = response.data
      console.log('Masalar başarıyla alındı:', tables.value)
    }
    catch (error) {
      console.error('Masalar alınırken bir hata oluştu:', error)
    }
  }

  return { tables, fetchTables }
})
