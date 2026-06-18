<template>
  <article class="grid grid-cols-1 lg:grid-cols-12 gap-6">
    <div class="lg:col-span-8 bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden shadow-sm">
      <div class="px-6 py-4 border-b border-outline-variant bg-surface-container-low flex items-center justify-between flex-wrap gap-4">
        <div class="flex items-center gap-3">
          <h3 class="font-bold text-sm">Trang thiết bị tại</h3>
          <select v-model="localRoomId" class="px-3 py-1 bg-white border border-outline-variant rounded text-xs font-bold outline-none focus:ring-1 focus:ring-primary">
            <option value="" disabled>--- Chọn phòng ---</option>
            <option v-for="r in rooms" :key="r.id" :value="r.id">Phòng {{ r.roomNumber }}</option>
          </select>
        </div>
        <input 
          v-if="localRoomId"
          v-model="localSearch" 
          class="px-2 py-1 text-[10px] bg-white border border-outline-variant rounded outline-none w-32" 
          placeholder="Lọc thiết bị..." 
        />
      </div>

      <div v-if="!localRoomId" class="flex flex-col items-center justify-center py-20 opacity-30">
        <span class="material-symbols-outlined text-6xl mb-4">construction</span>
        <p class="text-sm font-bold">Chọn phòng để quản lý thiết bị</p>
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead class="bg-surface-container-low/50 text-[10px] text-on-surface-variant uppercase font-bold border-b border-outline-variant">
            <tr>
              <th class="px-6 py-4">Tên thiết bị</th>
              <th class="px-6 py-4 text-center">Trạng thái</th>
              <th class="px-6 py-4 text-right">Thao tác</th>
            </tr>
          </thead>
          <transition-group name="stagger" tag="tbody" class="divide-y divide-outline-variant">
            <tr 
              v-for="eq in filteredEquipments" :key="eq.id" @click="$emit('select', eq)"
              class="hover:bg-surface-container-low/40 cursor-pointer transition-all"
              :class="{ 'bg-secondary-container/20': selectedId === eq.id }"
            >
              <td class="px-6 py-4 flex items-center gap-3">
                <div class="w-10 h-10 rounded bg-surface-container-high flex items-center justify-center text-on-surface-variant">
                  <span class="material-symbols-outlined text-base">home_repair_service</span>
                </div>
                <div>
                  <p class="font-bold text-base">{{ eq.equipmentName }}</p>
                  <p class="text-[10px] opacity-40 font-mono">SN: {{ eq.id.slice(-12).toUpperCase() }}</p>
                </div>
              </td>
              <td class="px-6 py-4 text-center">
                <span :class="getBadgeClass(equipmentTone(eq.status))" class="px-2 py-1 rounded text-[10px] font-bold uppercase tracking-wider">
                  {{ translateStatus(eq.status) }}
                </span>
              </td>
              <td class="px-6 py-4 text-right" @click.stop>
                <button @click="$emit('delete', eq.id)" class="p-1.5 hover:bg-error-container hover:text-error rounded transition-all">
                  <span class="material-symbols-outlined text-sm">delete</span>
                </button>
              </td>
            </tr>
          </transition-group>
        </table>
        <div v-if="filteredEquipments.length === 0" class="py-12 text-center opacity-40 italic text-sm">
           Chưa có thiết bị nào được ghi nhận.
        </div>
      </div>
    </div>

    <div class="lg:col-span-4 space-y-6">
      <div class="bg-surface-container-lowest border border-outline-variant rounded-xl p-6 shadow-sm self-start">
        <h4 class="font-bold text-sm mb-4">Thêm thiết bị mới</h4>
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div class="space-y-1">
            <label class="text-[10px] font-bold opacity-50 uppercase tracking-widest">Tên thiết bị</label>
            <input v-model.trim="form.equipmentName" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary" placeholder="VD: Điều hòa LG, Quạt trần..." required />
          </div>
          <button 
            type="submit" 
            :disabled="!localRoomId"
            class="w-full bg-primary text-on-primary font-bold py-3 rounded-lg text-sm active:scale-95 transition-all disabled:opacity-50"
          >
            Thêm vào phòng
          </button>
        </form>
      </div>

      <div v-if="selectedEquipment" class="bg-surface-container-lowest border border-outline-variant rounded-xl p-6 shadow-sm animate-in fade-in slide-in-from-bottom-4 duration-300">
        <h4 class="font-bold text-sm mb-4 italic text-primary">Cập nhật nhanh trạng thái</h4>
        <div class="p-3 bg-secondary-container/10 border border-secondary/20 rounded-lg mb-4">
          <p class="text-[10px] font-bold opacity-70">SẢN PHẨM:</p>
          <p class="font-bold text-sm">{{ selectedEquipment.equipmentName }}</p>
        </div>
        <form @submit.prevent="handleStatusUpdate" class="space-y-4">
          <div class="space-y-1">
            <label class="text-[10px] font-bold opacity-50 uppercase tracking-widest">Tình trạng hiện tại</label>
            <select v-model="statusForm.status" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm font-bold">
              <option value="ACTIVE">ACTIVE (Hoạt động tốt)</option>
              <option value="UNDER_MAINTENANCE">MAINTENANCE (Bảo trì)</option>
              <option value="BROKEN">BROKEN (Hư hỏng)</option>
              <option value="RETIRED">RETIRED (Thanh lý)</option>
            </select>
          </div>
          <button type="submit" class="w-full bg-secondary-container text-primary font-bold py-3 rounded-lg text-sm active:scale-95 transition-all">Lưu thay đổi</button>
        </form>
      </div>
    </div>
  </article>
</template>

<script setup>
import { ref, reactive, watch, computed } from 'vue'

const props = defineProps({
  equipments: { type: Array, required: true },
  rooms: { type: Array, required: true },
  selectedId: { type: String, default: '' },
  selectedRoomId: { type: String, default: '' },
  selectedEquipment: { type: Object, default: null }
})

const emit = defineEmits(['save', 'delete', 'select', 'change-room', 'update-status'])

const localRoomId = ref(props.selectedRoomId)
const localSearch = ref('')

const form = reactive({
  equipmentName: ''
})

const statusForm = reactive({
  status: 'ACTIVE'
})

watch(() => props.selectedRoomId, (val) => {
  localRoomId.value = val
})

watch(localRoomId, (val) => {
  emit('change-room', val)
})

watch(() => props.selectedEquipment, (val) => {
  if (val) statusForm.status = val.status || 'ACTIVE'
}, { immediate: true })

const filteredEquipments = computed(() => {
  const q = localSearch.value.trim().toLowerCase()
  return props.equipments.filter(e => !q || e.equipmentName.toLowerCase().includes(q))
})

function handleSubmit() {
  emit('save', { roomId: localRoomId.value, payload: { ...form } })
  form.equipmentName = ''
}

function handleStatusUpdate() {
  emit('update-status', { id: props.selectedId, payload: { ...statusForm } })
}

function translateStatus(s) {
  const map = { ACTIVE: 'Tốt', UNDER_MAINTENANCE: 'Bảo trì', BROKEN: 'Hỏng', RETIRED: 'Thanh lý' }
  return map[s] || s
}

function equipmentTone(s) {
  if (s === 'ACTIVE') return 'success'
  if (s === 'UNDER_MAINTENANCE' || s === 'BROKEN') return 'warning'
  return 'neutral'
}

function getBadgeClass(tone) {
  if (tone === 'success') return 'bg-green-100 text-green-700'
  if (tone === 'warning') return 'bg-orange-100 text-orange-700'
  return 'bg-gray-100 text-gray-700'
}
</script>
