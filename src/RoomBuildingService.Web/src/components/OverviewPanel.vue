<template>
  <div class="space-y-8">
    <!-- Stats Cards -->
    <section class="grid grid-cols-1 md:grid-cols-4 gap-6">
      <article v-for="card in localOverviewCards" :key="card.label" class="bg-surface-container-lowest border border-outline-variant p-6 rounded-xl hover:border-primary transition-all shadow-sm group">
        <div class="text-[10px] font-bold text-on-surface-variant uppercase tracking-widest mb-3 opacity-60 group-hover:opacity-100 transition-opacity">{{ card.label }}</div>
        <div class="flex justify-between items-end">
          <h3 class="font-display text-4xl font-bold leading-none tracking-tight">{{ card.value }}</h3>
          <div class="text-[10px] text-on-surface-variant opacity-70 font-medium">{{ card.hint }}</div>
        </div>
      </article>
    </section>

    <!-- Main Overview Content -->
    <article class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <div class="bg-surface-container-lowest border border-outline-variant rounded-xl p-6 shadow-sm">
        <h4 class="font-bold text-sm mb-4 flex items-center gap-2">
          <span class="material-symbols-outlined font-bold text-primary">pie_chart</span>
          Tỉ lệ lấp đầy Phòng
        </h4>
        <div class="aspect-square max-h-64 mx-auto relative flex items-center justify-center">
          <canvas id="roomStatusChart"></canvas>
          <div v-if="!rooms.length" class="absolute inset-0 flex items-center justify-center bg-surface/50 backdrop-blur-[2px] rounded-lg">
             <p class="text-xs font-bold opacity-50">Chưa có dữ liệu phòng</p>
          </div>
        </div>
        <div class="mt-6 grid grid-cols-3 gap-2">
           <div v-for="s in roomStats" :key="s.label" class="text-center p-2 rounded-lg bg-surface-container-low border border-outline-variant/30">
              <div class="text-[10px] font-bold opacity-50 uppercase">{{ s.label }}</div>
              <div class="text-sm font-bold text-primary">{{ s.count }}</div>
           </div>
        </div>
      </div>

      <div class="bg-surface-container-lowest border border-outline-variant rounded-xl p-6 shadow-sm flex flex-col">
        <h4 class="font-bold text-sm mb-4 flex items-center gap-2">
          <span class="material-symbols-outlined text-primary">analytics</span>
          Thống kê hạ tầng
        </h4>
        <div class="flex-1 space-y-3">
          <div v-for="stat in infrastructureStats" :key="stat.l" class="flex items-center justify-between p-4 bg-surface-container-low border border-outline-variant/20 rounded-xl hover:bg-surface-container-high transition-colors">
            <div class="flex items-center gap-4">
              <div class="w-10 h-10 rounded-full bg-surface-container-highest flex items-center justify-center text-primary">
                <span class="material-symbols-outlined">{{ stat.i }}</span>
              </div>
              <span class="text-xs font-bold text-on-surface-variant uppercase tracking-wider">{{ stat.l }}</span>
            </div>
            <strong class="font-label-mono text-xl">{{ stat.v }}</strong>
          </div>
        </div>
        <div class="mt-6 p-4 bg-secondary-container/10 border border-secondary/20 rounded-xl">
           <p class="text-[10px] text-on-surface-variant opacity-70 leading-relaxed italic">
             * Hệ thống đang kết nối trực tiếp với Registry Service. Các chỉ số được cập nhật theo thời gian thực (Real-time synchronization).
           </p>
        </div>
      </div>
    </article>
  </div>
</template>

<script setup>
import { computed, onMounted, onUnmounted, watch } from 'vue'

const props = defineProps({
  buildings: { type: Array, required: true },
  roomTypes: { type: Array, required: true },
  rooms: { type: Array, required: true },
  equipments: { type: Array, required: true }
})

let chartInstance = null

const localOverviewCards = computed(() => [
  { label: 'Tòa nhà', value: props.buildings.length, hint: 'Quy mô hạ tầng' },
  { label: 'Mẫu phòng', value: props.roomTypes.length, hint: 'Cấu hình đa dạng' },
  { label: 'Tổng Phòng', value: props.rooms.length, hint: 'Sức chứa hệ thống' },
  { label: 'Thiết bị', value: props.equipments.length, hint: 'Tài sản cố định' }
])

const infrastructureStats = computed(() => [
  { l: 'Tổng tòa nhà', v: props.buildings.length, i: 'apartment' },
  { l: 'Tổng số phòng', v: props.rooms.length, i: 'meeting_room' },
  { l: 'Mẫu cấu hình', v: props.roomTypes.length, i: 'category' }
])

const roomStats = computed(() => {
  const available = props.rooms.filter(r => r.status === 'AVAILABLE').length
  const full = props.rooms.filter(r => r.status === 'FULL').length
  const maintenance = props.rooms.filter(r => r.status === 'UNDER_MAINTENANCE').length
  return [
    { label: 'Trống', count: available, color: '#1a8a3b' },
    { label: 'Đã đầy', count: full, color: '#565e74' },
    { label: 'Bảo trì', count: maintenance, color: '#ba1a1a' }
  ]
})

function initChart() {
  const ctx = document.getElementById('roomStatusChart')
  if (!ctx) return
  if (chartInstance) chartInstance.destroy()

  const stats = roomStats.value
  const hasData = stats.some(s => s.count > 0)

  if (typeof Chart === 'undefined') return

  // @ts-ignore
  chartInstance = new Chart(ctx, {
    type: 'doughnut',
    data: {
      labels: stats.map(s => s.label),
      datasets: [{
        data: hasData ? stats.map(s => s.count) : [1],
        backgroundColor: hasData ? stats.map(s => s.color) : ['#e6e8ea'],
        borderWidth: 0,
        hoverOffset: 10
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      cutout: '75%',
      plugins: {
        legend: { display: false },
        tooltip: { enabled: hasData }
      }
    }
  })
}

watch(() => props.rooms, () => {
  initChart()
}, { deep: true })

onMounted(() => {
  initChart()
})

onUnmounted(() => {
  if (chartInstance) chartInstance.destroy()
})
</script>
