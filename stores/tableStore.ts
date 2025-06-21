import axios from 'axios'
import { defineStore } from 'pinia'

export const useTableStore = defineStore('table', () => {
  const tables = ref<Array<any>>([])
  const emptyTables = ref<Array<any>>([])
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

  const fetchEmptyTables = async () => {
    try {
      const response = await axios.get(`${baseUrl}/Table/empty`)
      emptyTables.value = response.data
      console.log('Boş masalar başarıyla alındı:', emptyTables.value)
    }
    catch (error) {
      console.error('Boş masalar alınırken bir hata oluştu:', error)
    }
  }

  return { tables, fetchTables,fetchEmptyTables, emptyTables  }
})
