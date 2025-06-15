<!-- CustomInput.vue -->
<script setup>
defineProps({
  modelValue: {
    type: [String, Number],
    default: '',
  },
  type: {
    type: String,
    default: 'text',
  },
  placeholder: {
    type: String,
    default: '',
  },
  variant: {
    type: String,
    default: 'none',
  },
  required: {
    type: Boolean,
    default: false,
  },
  // Ek proplar ekleyebilirsiniz
  class: {
    type: String,
    default: '',
  },
})

const emit = defineEmits(['update:modelValue'])
</script>

<template>
  <UInput
    :value="modelValue"
    :type="type"
    :placeholder="placeholder"
    :variant="variant"
    :class="['!custom-input w-full border-b !border-gray-300 transition-colors py-2', class]"
    :required="required"
    @input="emit('update:modelValue', $event.target.value)"
  />
</template>
