<script setup lang="ts">
interface Option {
  label: string
  value: string
}

interface Props {
  modelValue: string
  options: Option[]
  placeholder?: string
  disabled?: boolean
  label?: string
  required?: boolean
  error?: string
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: 'Seçin...',
  disabled: false,
  required: false
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const updateValue = (event: Event) => {
  const target = event.target as HTMLSelectElement
  emit('update:modelValue', target.value)
}
</script>

<template>
  <div class="w-full">
    <!-- Label -->
    <label v-if="label" class="block text-sm font-medium text-gray-700 mb-1">
      {{ label }}
      <span v-if="required" class="text-red-500 ml-1">*</span>
    </label>
    
    <!-- Select -->
    <select 
      :value="modelValue"
      :disabled="disabled"
      @change="updateValue"
      class="w-full px-3 py-2 border rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors"
      :class="[
        error 
          ? 'border-red-300 focus:ring-red-500 focus:border-red-500' 
          : 'border-gray-300',
        disabled 
          ? 'bg-gray-100 cursor-not-allowed text-gray-500' 
          : 'bg-white hover:border-gray-400'
      ]"
    >
      <option value="">{{ placeholder }}</option>
      <option 
        v-for="option in options" 
        :key="option.value" 
        :value="option.value"
      >
        {{ option.label }}
      </option>
    </select>
    
    <!-- Error message -->
    <p v-if="error" class="mt-1 text-sm text-red-600">
      {{ error }}
    </p>
    
    <!-- Helper text -->
    <p v-else-if="$slots.help" class="mt-1 text-sm text-gray-500">
      <slot name="help" />
    </p>
  </div>
</template>