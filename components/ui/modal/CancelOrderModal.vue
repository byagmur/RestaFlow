<script setup lang="ts">
import { ref, watch } from 'vue'

interface Props {
  show: boolean
  orderId: string | number
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
})

const emit = defineEmits<{
  close: []
  confirm: [reason: string]
}>()

// const cancelReason = ref('')
const localError = ref('')

// Modal açıldığında formu temizle
watch(() => props.show, (newVal) => {
  if (newVal) {
    // cancelReason.value = ''
    localError.value = ''
  }
})

function handleConfirm() {
  // if (!cancelReason.value.trim()) {
  //   localError.value = 'İptal nedeni belirtmelisiniz!'
  //   return
  // }

  localError.value = ''
  emit('confirm', '') // Boş string gönder
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
          <div class="mx-auto w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mb-4">
            <svg class="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
            </svg>
          </div>
          <h3 class="text-xl font-bold text-gray-900 mb-2">
            Siparişi İptal Et
          </h3>
          <p class="text-gray-600">
            #{{ orderId }} numaralı siparişi iptal etmek istediğinizden emin misiniz?
          </p>
        </div>

        <!-- <div class="mb-6">
          <label class="block text-sm font-medium text-gray-700 mb-2">
            İptal Nedeni <span class="text-red-500">*</span>
          </label>
          <textarea
            v-model="cancelReason"
            rows="3"
            placeholder="İptal nedenini açıklayın..."
            :disabled="loading"
            class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 resize-none disabled:bg-gray-100 disabled:cursor-not-allowed"
            @keydown.enter.ctrl="handleConfirm"
          />
          <p class="mt-1 text-xs text-gray-500">
            Bu bilgi raporlama için kullanılacaktır.
          </p>
        </div> -->

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

        <div class="flex gap-3">
          <UButton
            type="button"
            :disabled="loading"
            class="!text-center  flex-1 px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 focus:ring-offset-2 disabled:opacity-50 transition font-medium"
            @click="handleClose"
          >
            Vazgeç
          </UButton>
          <UButton
            type="button"
            :disabled="loading"
            class=" text-center flex-1 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 disabled:opacity-50 transition font-medium"
            @click="handleConfirm"
          >
            <span v-if="loading" class="inline-flex items-center">
              <svg class="animate-spin -ml-1 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
              </svg>
              İptal ediliyor...
            </span>
            <span v-else>
              Siparişi İptal Et
            </span>
          </UButton>
        </div>
      </div>
    </div>
  </Teleport>
</template>
