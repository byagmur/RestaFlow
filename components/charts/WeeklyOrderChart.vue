<script setup lang="ts">
import { Chart, registerables } from 'chart.js'
import { computed } from 'vue'
import { Bar } from 'vue-chartjs'

const props = defineProps<{ orders: any[] }>()

Chart.register(...registerables)

// Son 7 günün tarihlerini ve sipariş sayılarını hazırla
const days = Array.from({ length: 7 }).map((_, i) => {
  const d = new Date()
  d.setDate(d.getDate() - (6 - i))
  return d.toISOString().slice(0, 10)
})

const chartData = computed(() => {
  const counts = days.map(day =>
    props.orders.filter(order => order.orderDate && order.orderDate.startsWith(day)).length,
  )
  return {
    labels: days.map(d => d.slice(5)), // '06-03' gibi
    datasets: [
      {
        label: 'Sipariş Sayısı',
        backgroundColor: '#2563eb',
        data: counts,
        borderRadius: 6,
      },
    ],
  }
})

const chartOptions = {
  responsive: true,
  plugins: {
    legend: { display: false },
  },
  scales: {
    y: { beginAtZero: true, ticks: { stepSize: 1, precision: 0 } },
  },
}
</script>

<template>
  <Bar :data="chartData" :options="chartOptions" />
</template>
