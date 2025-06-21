<script setup lang="ts">
import { ref, watch } from 'vue'

const props = defineProps<{
  modelValue: string
}>()
const emit = defineEmits(['update:modelValue'])

const options = [
  { label: 'Tümü', value: '' },
  { label: 'Hazırlanıyor', value: 'Hazırlanıyor' },
  { label: 'Servis Edildi', value: 'Servis Edildi' },
  { label: 'İptal Edildi', value: 'İptal Edildi' },
  { label: 'Tamamlandı', value: 'Tamamlandı' },
  { label: 'Ödendi', value: 'Ödendi' },
  { label: 'Açıldı', value: 'Açıldı' },
]

const selected = ref(props.modelValue)
watch(() => props.modelValue, v => selected.value = v)
watch(selected, v => emit('update:modelValue', v))
</script>

<template>
  <select
    :value="modelValue"
    @change="emit('update:modelValue', $event.target.value)"
    class="bg-gray-100 shadow-lg rounded-2xl px-3 py-2 text-md min-w-[120px]"
  >
    <option
      v-for="option in options"
      :key="option.value"
      :value="option.value"
      class="!text-gray-700  "
    >
      {{ option.label }}
    </option>
  </select>
</template>