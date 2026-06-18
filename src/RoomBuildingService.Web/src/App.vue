<template>
  <div class="flex h-screen bg-surface font-body-md text-on-surface overflow-hidden">
    <!-- Sidebar -->
    <aside class="w-64 bg-surface-container-low border-r border-outline-variant flex flex-col py-6 z-50">
      <div class="px-6 mb-8 text-center md:text-left">
        <h1 class="font-display text-3xl font-bold text-primary tracking-tight">Facility OS</h1>
        <p class="text-on-surface-variant text-xs opacity-70">Quản trị viên Hệ thống</p>
      </div>
      <nav class="flex-1 px-3 space-y-1 overflow-y-auto">
        <button
          v-for="item in navItems" :key="item.key"
          @click="activeSection = item.key"
          class="w-full flex items-center justify-between px-4 py-3 rounded-lg transition-all duration-200 cursor-pointer active:scale-95"
          :class="activeSection === item.key ? 'bg-secondary-container text-primary font-bold' : 'text-on-surface-variant hover:bg-surface-container-high'"
        >
          <div class="flex items-center gap-3">
            <span class="material-symbols-outlined">{{ item.icon }}</span>
            <span class="font-body-md">{{ item.label }}</span>
          </div>
          <span v-if="item.count" class="text-[10px] font-bold opacity-60 bg-surface-container-highest px-1.5 rounded-full">{{ item.count }}</span>
        </button>
      </nav>
      <div class="mt-auto px-3 pt-6 border-t border-outline-variant space-y-1 text-on-surface-variant">
        <button @click="refreshAll" :disabled="loading" class="w-full flex items-center gap-3 px-4 py-3 rounded-lg hover:bg-surface-container-high transition-all active:scale-95">
          <span class="material-symbols-outlined" :class="{ 'animate-spin': loading }">refresh</span>
          <span class="text-sm font-medium">{{ loading ? 'Cơ sở dữ liệu...' : 'Làm mới dữ liệu' }}</span>
        </button>
        <div class="flex items-center gap-3 px-4 py-4 mt-2">
          <div class="w-10 h-10 rounded-full bg-secondary-container flex items-center justify-center text-primary font-bold text-sm shadow-sm border border-primary/10">A</div>
          <div class="overflow-hidden">
            <p class="font-bold text-sm truncate">Admin</p>
            <p class="text-[10px] opacity-60 truncate flex items-center gap-1">
               <span class="w-1.5 h-1.5 rounded-full bg-green-500 animate-pulse"></span>
               Live Connect
            </p>
          </div>
        </div>
      </div>
    </aside>

    <main class="flex-1 flex flex-col h-screen overflow-hidden">
      <!-- Topbar -->
      <header class="bg-surface-container-lowest h-14 w-full sticky top-0 z-40 border-b border-outline-variant flex items-center px-6 overflow-hidden">
        <!-- Full-width Marquee Style -->
        <div class="flex-1 overflow-hidden relative">
          <div class="flex items-center gap-12 animate-marquee whitespace-nowrap">
            <span class="text-[10px] font-bold text-primary uppercase tracking-[0.4em] opacity-60">FACILITY OS — NÂNG TẦM KHÔNG GIAN SỐNG TRỰC TUYẾN</span>
            <span class="text-[10px] font-bold text-on-surface-variant/20 uppercase tracking-[0.4em]">//</span>
            <span class="text-[10px] font-bold text-on-surface-variant uppercase tracking-[0.4em] opacity-40 text-glow">KIẾN TẠO GIÁ TRỊ • KHẲNG ĐỊNH ĐẲNG CẤP QUẢN TRỊ</span>
            <span class="text-[10px] font-bold text-on-surface-variant/20 uppercase tracking-[0.4em]">//</span>
            <span class="text-[10px] font-bold text-on-surface-variant uppercase tracking-[0.4em] opacity-40">TỐI GIẢN TRONG THIẾT KẾ • ĐỈNH CAO TRONG CÔNG NGHỆ</span>
            <span class="text-[10px] font-bold text-on-surface-variant/20 uppercase tracking-[0.4em]">//</span>
            <span class="text-[10px] font-bold text-primary uppercase tracking-[0.4em] opacity-60">MANAGEMENT REIMAGINED — VERSION 4.0 STABLE</span>
            
            <!-- Duplicate for infinite loop -->
            <span class="text-[10px] font-bold text-on-surface-variant/20 uppercase tracking-[0.4em]">//</span>
            <span class="text-[10px] font-bold text-primary uppercase tracking-[0.4em] opacity-60">FACILITY OS — NÂNG TẦM KHÔNG GIAN SỐNG TRỰC TUYẾN</span>
          </div>
        </div>
      </header>

      <!-- Content Area -->
      <div class="flex-1 overflow-y-auto p-6 space-y-8">
        <section class="flex flex-col md:flex-row md:items-end justify-between gap-4 border-b border-outline-variant/30 pb-6">
          <div>
            <h2 class="font-headline-lg text-2xl font-bold text-primary leading-tight">{{ currentSectionTitle }}</h2>
          </div>
          <div class="flex gap-2">
            <span class="px-3 py-1 bg-primary text-on-primary rounded-full text-[10px] font-bold uppercase tracking-wider">{{ activeSectionLabel }}</span>
          </div>
        </section>

        <!-- Dynamic Panels -->
        <section class="min-h-[500px]">
           <div v-if="loading && !loadedOnce" class="flex flex-col items-center justify-center py-32 text-on-surface-variant animate-pulse space-y-4">
             <span class="material-symbols-outlined text-5xl animate-spin text-primary">sync</span>
             <p class="text-[10px] font-bold tracking-[0.2em] uppercase opacity-50">Đang đồng bộ dữ liệu...</p>
           </div>
           <div v-else>
              <transition name="fade" mode="out-in">
                 <div :key="activeSection">
                    <OverviewPanel v-if="activeSection === 'overview'" :buildings="buildings" :room-types="roomTypes" :rooms="rooms" :equipments="equipments" />
                    <BuildingsPanel v-else-if="activeSection === 'buildings'" ref="buildingsRef" :buildings="buildings" :busy-key="busyKey" @save="saveBuilding" @delete="deleteBuilding" />
                    <RoomTypesPanel v-else-if="activeSection === 'room-types'" ref="roomTypesRef" :room-types="roomTypes" :busy-key="busyKey" @save="saveRoomType" @delete="deleteRoomType" />
                    <RoomsPanel v-else-if="activeSection === 'rooms'" ref="roomsRef" :rooms="rooms" :buildings="buildings" :room-types="roomTypes" :busy-key="busyKey" @save="saveRoom" @delete="deleteRoom" @update-status="updateRoomStatus" />
                    <BedsPanel v-else-if="activeSection === 'beds'" ref="bedsRef" :beds="beds" :rooms="rooms" :selected-room-id="selectedRoomId" @save="saveBed" @delete="deleteBed" @change-room="loadRoomDetails" />
                    <EquipmentsPanel v-else-if="activeSection === 'equipments'" ref="equipmentsRef" :equipments="equipments" :rooms="rooms" :selected-room-id="selectedRoomId" :selected-id="selectedEquipment?.id" :selected-equipment="selectedEquipment" @save="saveEquipment" @delete="deleteEquipment" @change-room="loadRoomDetails" @update-status="updateEquipmentStatus" @select="eq => selectedEquipment = eq" />
                 </div>
              </transition>
           </div>
        </section>
      </div>
    </main>

    <!-- Global UI helpers (Toasts, Modals) -->
    <transition name="fade">
      <div v-if="toast.show" class="fixed bottom-8 left-1/2 -translate-x-1/2 z-[200] flex items-center gap-4 px-8 py-4 rounded-3xl bg-on-surface text-surface shadow-2xl transition-all duration-500 border border-surface/10">
        <span class="material-symbols-outlined" :class="toast.type === 'success' ? 'text-green-400' : 'text-red-400'">{{ toast.type === 'success' ? 'verified' : 'error' }}</span>
        <div class="text-xs">
          <strong class="uppercase tracking-widest opacity-60 text-[9px] block mb-0.5">{{ toast.title }}</strong>
          <span class="font-bold text-[13px]">{{ toast.message }}</span>
        </div>
      </div>
    </transition>

    <transition name="fade">
      <div v-if="confirmModal.show" class="fixed inset-0 z-[210] flex items-center justify-center p-4 backdrop-blur-sm">
        <div class="absolute inset-0 bg-on-surface/60" @click="confirmModal.show = false"></div>
        <div class="relative bg-surface p-10 rounded-[3rem] shadow-2xl max-w-sm w-full animate-in zoom-in-95 duration-300">
          <div class="w-16 h-16 bg-error-container/20 text-error rounded-3xl flex items-center justify-center mb-6"><span class="material-symbols-outlined text-3xl font-bold">delete_forever</span></div>
          <h3 class="font-headline-lg text-2xl font-bold mb-3">Xác nhận xóa?</h3>
          <p class="text-on-surface-variant text-sm mb-8 leading-relaxed opacity-80">{{ confirmModal.message }}</p>
          <div class="flex gap-4">
            <button @click="confirmModal.show = false" class="flex-1 px-6 py-4 rounded-3xl bg-surface-container-high font-bold text-sm active:scale-95 transition-all">Hủy</button>
            <button @click="executeConfirmedAction" class="flex-1 px-6 py-4 rounded-3xl bg-error text-on-error font-bold text-sm active:scale-95 transition-all shadow-lg shadow-error/20">Xác nhận</button>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { useManagement } from './composables/useManagement'

// Import Components
import OverviewPanel from './components/OverviewPanel.vue'
import BuildingsPanel from './components/BuildingsPanel.vue'
import RoomTypesPanel from './components/RoomTypesPanel.vue'
import RoomsPanel from './components/RoomsPanel.vue'
import BedsPanel from './components/BedsPanel.vue'
import EquipmentsPanel from './components/EquipmentsPanel.vue'

// UI state
const activeSection = ref('overview')
const loadedOnce = ref(false)
const toast = reactive({ show: false, type: 'success', title: '', message: '' })
const confirmModal = reactive({ show: false, message: '', onConfirm: null })
const selectedEquipment = ref(null)

// UI Refs
const buildingsRef = ref(null); const roomsRef = ref(null); const roomTypesRef = ref(null); const bedsRef = ref(null); const equipmentsRef = ref(null);

// UI Helpers
const showToast = (type, title, message) => { Object.assign(toast, { show: true, type, title, message }); setTimeout(() => toast.show = false, 3000) }
const openConfirm = (message, onConfirm) => Object.assign(confirmModal, { show: true, message, onConfirm })
const executeConfirmedAction = () => { if (confirmModal.onConfirm) confirmModal.onConfirm(); confirmModal.show = false }

// Inject Management Logic
const {
  loading, busyKey, buildings, roomTypes, rooms, beds, equipments, selectedRoomId,
  refreshAll, saveBuilding, deleteBuilding, saveRoomType, deleteRoomType,
  saveRoom, updateRoomStatus, deleteRoom, loadRoomDetails,
  saveBed, deleteBed, saveEquipment, updateEquipmentStatus, deleteEquipment
} = useManagement(showToast, openConfirm)

const navItems = computed(() => [
  { key: 'overview', label: 'Bàn làm việc', icon: 'dashboard' },
  { key: 'buildings', label: 'Tòa nhà', icon: 'apartment', count: buildings.value.length },
  { key: 'room-types', label: 'Loại phòng', icon: 'category', count: roomTypes.value.length },
  { key: 'rooms', label: 'Phòng ở', icon: 'meeting_room', count: rooms.value.length },
  { key: 'beds', label: 'Giường ngủ', icon: 'bed' },
  { key: 'equipments', label: 'Thiết bị', icon: 'construction' }
])

const sectionMeta = computed(() => {
  const map = {
    overview: { title: 'Dashboard Hệ thống', label: 'Bàn làm việc' },
    buildings: { title: 'Quản lý Tòa nhà', label: 'Tòa nhà' },
    'room-types': { title: 'Cấu hình Loại phòng', label: 'Loại mẫu' },
    rooms: { title: 'Dữ liệu Phòng ở', label: 'Phòng' },
    beds: { title: 'Danh mục Giường', label: 'Giường' },
    equipments: { title: 'Trang thiết bị', label: 'Thiết bị' }
  }
  return map[activeSection.value] || map.overview
})

const currentSectionTitle = computed(() => sectionMeta.value.title)
const activeSectionLabel = computed(() => sectionMeta.value.label)

onMounted(async () => {
  await refreshAll()
  loadedOnce.value = true
})
</script>

<style>
.fade-enter-active, .fade-leave-active { transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); }
.fade-enter-from, .fade-leave-to { opacity: 0; transform: translateY(8px); }

@keyframes marquee {
  0% { transform: translateX(0); }
  100% { transform: translateX(-50%); }
}

.animate-marquee {
  display: inline-flex;
  animation: marquee 30s linear infinite;
}

.animate-marquee:hover {
  animation-play-state: paused;
}
</style>
