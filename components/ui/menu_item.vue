<script setup>
defineProps({
  to: { type: String, required: true },
  title: { type: String, required: true },
  icon: { type: String, required: true },
})
</script>

<template>
  <NuxtLink
    :to="to"
    class="p-2 rounded-xl hover:bg-neutral-300/50 mx-auto transition-colors duration-200 flex flex-col items-center"
  >
    <Icon :name="icon" class="!w-7 !h-7 text-neutral-400" />
    <h1 class=" text-center p-1 text-xs text-neutral-400">
      {{ title }}
    </h1>
  </NuxtLink>
</template>
