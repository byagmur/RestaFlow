<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useTableStore } from '@/stores/tableStore'

interface Props {
  show: boolean
  orderId: string | number
  currentTableId: string | number
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
})

const emit = defineEmits<{
  close: []
  confirm: [newTableId: string | number]
}>()
const tableStore = useTableStore()
const loadingTables = ref(false)

const selectedTableId = ref('')
const localError = ref('')

const availableTables = computed(() => {
  return tableStore.emptyTables.filter(table => table.uid !== props.currentTableId)
})

// Modal açıldığında formu temizle ve masaları yükle
watch(() => props.show, async (newVal) => {
  if (newVal) {
    selectedTableId.value = ''
    localError.value = ''

    // Masaları yükle
    loadingTables.value = true
    try {
      await tableStore.fetchEmptyTables()
console.log('Boş masalar yüklendi:', tableStore.emptyTables)
      if (tableStore.emptyTables.length === 0) {
        localError.value = 'Şu anda boş masa bulunmuyor.'
      }
    }
    catch (error) {
      console.error('Masalar yüklenirken hata:', error)
    }
    finally {
      loadingTables.value = false
    }
  }
})

function handleConfirm() {
  if (!selectedTableId.value || selectedTableId.value === props.currentTableId.toString()) {
    localError.value = 'Lütfen farklı bir masa seçin!'
    return
  }

  localError.value = ''
  emit('confirm', selectedTableId.value)
}

function handleClose() {
  if (!props.loading) {
    emit('close')
  }
}

function handleKeyDown(event: KeyboardEvent) {
  if (event.key === 'Escape' && !props.loading) {
    handleClose()
  }
}
</script>

<template>
  <!-- Modal Backdrop -->
  <Teleport to="body">
    <div
      v-if="show"
      class="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 backdrop-blur-sm"
      tabindex="-1"
      @click.self="handleClose"
      @keydown="handleKeyDown"
    >
      <div
        class="bg-white rounded-2xl shadow-2xl w-full max-w-md mx-4 p-6 transform transition-all duration-200"
        :class="show ? 'scale-100 opacity-100' : 'scale-95 opacity-0'"
      >
        <!-- Header -->
        <div class="text-center mb-6">
          <div class="mx-auto w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mb-4">
            <svg class="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4" />
            </svg>
          </div>
          <h3 class="text-xl font-bold text-gray-900 mb-2">
            Masayı Taşı
          </h3>
          <p class="text-gray-600">
            #{{ orderId }} numaralı siparişi Masa {{ currentTableId }}'den boş bir masaya taşıyın.
          </p>
        </div>

        <!-- Masa seçimi -->
        <div class="mb-6">
          <label class="block text-sm font-medium text-gray-700 mb-2">
            Hedef Masa (Sadece Boş Masalar) <span class="text-red-500">*</span>
          </label>

          <!-- Loading durumu -->
          <div v-if="loadingTables" class="w-full px-3 py-2 border border-gray-300 rounded-md bg-gray-50 text-center">
            <div class="inline-flex items-center">
              <svg class="animate-spin -ml-1 mr-2 h-4 w-4 text-gray-500" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
              </svg>
              <span class="text-sm text-gray-500">Boş masalar yükleniyor...</span>
            </div>
          </div>

          <!-- Masa seçim combo'su -->
          <select
            v-else
            v-model="selectedTableId"
            :disabled="loading || availableTables.length === 0"
            class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-green-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
          >
            <option value="">
              {{ availableTables.length === 0 ? 'Boş masa bulunamadı' : 'Boş masa seçin...' }}
            </option>
            <option
              v-for="table in availableTables"
              :key="table.uid"
              :value="table.uid"
              class="text-green-600"
            >
              {{ table.tableName }} (Boş)
            </option>
          </select>

          <p class="mt-1 text-xs text-gray-500">
            {{ availableTables.length === 0 && !loadingTables
              ? 'Şu anda boş masa bulunmuyor.'
              : 'Sadece boş masalar gösteriliyor.'
            }}
          </p>
        </div>

        <!-- Hata mesajı -->
        <div v-if="localError" class="mb-4 p-3 bg-red-50 border border-red-200 rounded-md">
          <div class="flex items-center">
            <svg class="w-4 h-4 text-red-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01" />
            </svg>
            <p class="text-sm text-red-600">
              {{ localError }}
            </p>
          </div>
        </div>

        <!-- Boş masa yoksa uyarı -->
        <div v-if="!loadingTables && availableTables.length === 0" class="mb-4 p-3 bg-yellow-50 border border-yellow-200 rounded-md">
          <div class="flex items-center">
            <svg class="w-4 h-4 text-yellow-600 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>
            <p class="text-sm text-yellow-700">
              Şu anda tüm masalar dolu. Masa taşıma işlemi yapılamaz.
            </p>
          </div>
        </div>

        <!-- Butonlar -->
        <div class="flex gap-3">
          <UButton
            type="button"
            :disabled="loading"
            class="!text-center flex-1 px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 disabled:opacity-50 transition font-medium"
            @click="handleClose"
          >
            Vazgeç
          </UButton>
          <UButton
            type="button"
            :disabled="loading || !selectedTableId || loadingTables || availableTables.length === 0"
            class="text-center flex-1 px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 focus:ring-offset-2 disabled:opacity-50 transition font-medium"
            @click="handleConfirm"
          >
            <span v-if="loading" class="inline-flex items-center">
              <svg class="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
              </svg>
              Taşınıyor...
            </span>
            <span v-else>
              Masayı Taşı
            </span>
          </UButton>
        </div>
      </div>
    </div>
  </Teleport>
</template>
