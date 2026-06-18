<template>
  <article class="space-y-6">
    <div class="bg-surface-container-lowest border border-outline-variant rounded-xl p-4 flex flex-wrap gap-4 items-end shadow-sm">
      <div class="flex-1 grid grid-cols-1 md:grid-cols-4 gap-4 min-w-[300px]">
        <div class="space-y-1">
          <label class="text-[9px] font-bold opacity-50 uppercase">Tòa nhà</label>
          <select v-model="filters.buildingId" class="w-full px-3 py-1.5 bg-surface-container-low border border-outline-variant rounded text-xs outline-none focus:ring-1 focus:ring-primary">
            <option value="">Tất cả tòa nhà</option>
            <option v-for="b in buildings" :key="b.id" :value="b.id">{{ b.name }}</option>
          </select>
        </div>
        <div class="space-y-1">
          <label class="text-[9px] font-bold opacity-50 uppercase">Tầng</label>
          <input v-model.number="filters.floor" type="number" class="w-full px-3 py-1.5 bg-surface-container-low border border-outline-variant rounded text-xs outline-none" placeholder="Mọi tầng" />
        </div>
        <div class="space-y-1">
          <label class="text-[9px] font-bold opacity-50 uppercase">Trạng thái</label>
          <select v-model="filters.status" class="w-full px-3 py-1.5 bg-surface-container-low border border-outline-variant rounded text-xs outline-none">
            <option value="">Mọi trạng thái</option>
            <option value="AVAILABLE">Trống</option>
            <option value="FULL">Đầy</option>
            <option value="UNDER_MAINTENANCE">Bảo trì</option>
          </select>
        </div>
        <div class="space-y-1">
          <label class="text-[9px] font-bold opacity-50 uppercase">Số phòng</label>
          <input v-model="filters.q" class="w-full px-3 py-1.5 bg-surface-container-low border border-outline-variant rounded text-xs outline-none" placeholder="Tìm số phòng..." />
        </div>
      </div>
      <div class="flex gap-2">
        <button @click="resetFilters" class="px-4 py-1.5 text-xs font-bold hover:bg-surface-container-high rounded transition-all">Đặt lại</button>
        <button @click="currentPage = 1" class="bg-primary text-on-primary px-6 py-1.5 rounded font-bold text-xs active:scale-95 transition-all">Lọc dữ liệu</button>
      </div>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
      <div class="lg:col-span-8 bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden shadow-sm">
        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead class="bg-surface-container-low/50 text-[10px] text-on-surface-variant uppercase font-bold border-b border-outline-variant">
              <tr>
                <th class="px-6 py-4">Phòng</th>
                <th class="px-6 py-4">Vị trí</th>
                <th class="px-6 py-4">Mẫu loại</th>
                <th class="px-6 py-4 text-center">Trạng thái</th>
                <th class="px-6 py-4 text-right"></th>
              </tr>
            </thead>
            <transition-group name="stagger" tag="tbody" class="divide-y divide-outline-variant">
              <tr 
                v-for="room in paginatedRooms" :key="room.id" @click="$emit('select', room)"
                class="hover:bg-surface-container-low/40 last:border-b-0 cursor-pointer transition-all"
                :class="{ 'bg-secondary-container/20': selectedId === room.id }"
              >
                <td class="px-6 py-4">
                  <div class="text-base font-bold">Phòng {{ room.roomNumber }}</div>
                  <div class="text-[10px] text-on-surface-variant font-mono opacity-60">UUID: {{ room.id.slice(0,8) }}</div>
                </td>
                <td class="px-6 py-4">
                  <div class="text-sm font-medium">{{ resolveBuildingName(room.buildingId) }}</div>
                  <div class="text-xs text-on-surface-variant opacity-70">Tầng {{ room.floorNumber }}</div>
                </td>
                <td class="px-6 py-4">
                  <div class="text-xs font-bold">{{ resolveRoomTypeName(room) }}</div>
                  <div class="mt-1 w-full bg-surface-container-high rounded-full h-1.5 overflow-hidden">
                    <div 
                      class="bg-primary h-full transition-all duration-500" 
                      :style="{ width: `${(room.currentOccupancy || 0) / (resolveRoomCapacity(room) || 1) * 100}%` }"
                    ></div>
                  </div>
                  <div class="text-[10px] mt-1 opacity-60 font-bold">{{ room.currentOccupancy || 0 }} / {{ resolveRoomCapacity(room) }}</div>
                </td>
                <td class="px-6 py-4 text-center">
                  <span :class="getBadgeClass(roomTone(room.status))" class="px-2 py-1 rounded text-[10px] font-bold uppercase tracking-wider">
                    {{ translateStatus(room.status) }}
                  </span>
                </td>
                <td class="px-6 py-4 text-right space-x-1" @click.stop>
                  <button @click="startEdit(room)" class="p-1.5 hover:bg-surface-container-high rounded text-primary transition-all"><span class="material-symbols-outlined text-sm">edit</span></button>
                  <button @click="$emit('delete', room.id)" class="p-1.5 hover:bg-error-container hover:text-error rounded transition-all"><span class="material-symbols-outlined text-sm">delete</span></button>
                </td>
              </tr>
            </transition-group>
          </table>
        </div>
        <div class="px-6 py-4 border-t border-outline-variant flex items-center justify-between bg-surface-container-low/30 text-[10px]">
          <span class="text-on-surface-variant font-medium">Hiển thị {{ paginatedRooms.length }} / {{ filteredRooms.length }} phòng</span>
          <div class="flex gap-1">
            <button @click="currentPage--" :disabled="currentPage === 1" class="w-8 h-8 flex items-center justify-center rounded hover:bg-surface-container-high disabled:opacity-30"><span class="material-symbols-outlined text-sm">chevron_left</span></button>
            <div class="flex items-center px-3 font-bold text-primary">{{ currentPage }} / {{ totalPages || 1 }}</div>
            <button @click="currentPage++" :disabled="currentPage >= totalPages" class="w-8 h-8 flex items-center justify-center rounded hover:bg-surface-container-high disabled:opacity-30"><span class="material-symbols-outlined text-sm">chevron_right</span></button>
          </div>
        </div>
      </div>
      <div class="lg:col-span-4">
        <div class="bg-surface-container-lowest border border-outline-variant rounded-xl p-6 shadow-sm sticky top-6">
          <div class="flex justify-between items-center mb-6">
            <h4 class="font-bold text-sm">{{ editingId ? 'Sửa thông tin' : 'Thêm phòng mới' }}</h4>
            <button v-if="editingId" @click="resetForm" class="text-xs text-primary hover:underline font-bold">Thoát</button>
          </div>
          <form @submit.prevent="handleSubmit" class="space-y-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold opacity-50 uppercase tracking-widest">Tòa nhà trực thuộc</label>
              <select v-model="form.buildingId" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary" required>
                <option value="" disabled>--- Chọn tòa nhà ---</option>
                <option v-for="b in buildings" :key="b.id" :value="b.id">{{ b.name }}</option>
              </select>
            </div>
            <div class="grid grid-cols-2 gap-4">
              <div class="space-y-1">
                <label class="text-[10px] font-bold opacity-50 uppercase tracking-widest">Số hiệu phòng</label>
                <input v-model.trim="form.roomNumber" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm font-bold placeholder:italic" placeholder="VD: B201" required />
              </div>
              <div class="space-y-1">
                <label class="text-[10px] font-bold opacity-50 uppercase tracking-widest">Vị trí tầng</label>
                <input v-model.number="form.floorNumber" type="number" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none" required />
              </div>
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold opacity-50 uppercase tracking-widest">Cấu hình mẫu loại</label>
              <select v-model="form.roomTypeId" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary" required>
                <option value="" disabled>--- Chọn loại phòng ---</option>
                <option v-for="rt in roomTypes" :key="rt.id" :value="rt.id">{{ rt.typeName }} ({{ rt.capacity }} người)</option>
              </select>
            </div>
            <button type="submit" class="w-full bg-primary text-on-primary font-bold py-3 rounded-lg text-sm active:scale-95 transition-all uppercase tracking-widest mt-4">
              {{ editingId ? 'Cập nhật phòng' : 'Xác nhận tạo phòng' }}
            </button>
          </form>

          <div v-if="editingId" class="mt-8 pt-6 border-t border-outline-variant animate-in fade-in duration-500">
            <h5 class="text-[10px] font-bold opacity-50 uppercase tracking-widest mb-4">Trạng thái vận hành</h5>
            <div class="space-y-4">
               <div class="flex items-center justify-between p-3 bg-surface-container rounded-lg">
                 <div class="text-xs font-bold">{{ translateStatus(roomStatusForm.status) }}</div>
                 <select v-model="roomStatusForm.status" class="bg-transparent text-xs outline-none font-bold text-primary">
                   <option value="AVAILABLE">AVAILABLE</option>
                   <option value="FULL">FULL (Locked)</option>
                   <option value="UNDER_MAINTENANCE">UNDER_MAINTENANCE</option>
                 </select>
               </div>
               <div v-if="roomStatusForm.status === 'UNDER_MAINTENANCE'">
                 <label class="text-[9px] font-bold opacity-50">Lý do bảo trì</label>
                 <textarea v-model="roomStatusForm.maintenanceReason" class="w-full mt-1 px-2 py-1 bg-surface-container border border-outline-variant rounded text-xs outline-none" rows="2"></textarea>
               </div>
               <button @click="handleStatusUpdate" class="w-full py-2 bg-secondary-container text-primary font-bold rounded-lg text-xs hover:opacity-90 transition-all">Lưu trạng thái</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </article>
</template>

<script setup>
import { ref, reactive, computed, watch } from 'vue'

const props = defineProps({
  rooms: { type: Array, required: true },
  buildings: { type: Array, required: true },
  roomTypes: { type: Array, required: true },
  selectedId: { type: String, default: '' },
  busyKey: { type: String, default: '' }
})

const emit = defineEmits(['save', 'delete', 'select', 'update-status'])

const filters = reactive({
  buildingId: '',
  floor: '',
  status: '',
  q: ''
})

const currentPage = ref(1)
const itemsPerPage = 8
const editingId = ref('')

const form = reactive({
  buildingId: '',
  roomTypeId: '',
  roomNumber: '',
  floorNumber: 1
})

const roomStatusForm = reactive({
  status: 'AVAILABLE',
  maintenanceReason: ''
})

const filteredRooms = computed(() => {
  return props.rooms.filter(r => {
    const matchBuilding = !filters.buildingId || r.buildingId === filters.buildingId
    const matchFloor = filters.floor === '' || r.floorNumber === filters.floor
    const matchStatus = !filters.status || r.status === filters.status
    const matchQ = !filters.q || r.roomNumber.toLowerCase().includes(filters.q.toLowerCase())
    return matchBuilding && matchFloor && matchStatus && matchQ
  })
})

const totalPages = computed(() => Math.ceil(filteredRooms.value.length / itemsPerPage))

const paginatedRooms = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  return filteredRooms.value.slice(start, start + itemsPerPage)
})

watch([filters, () => props.rooms], () => {
  currentPage.value = 1
}, { deep: true })

function resetFilters() {
  Object.assign(filters, { buildingId: '', floor: '', status: '', q: '' })
}

function resetForm() {
  editingId.value = ''
  Object.assign(form, { buildingId: '', roomTypeId: '', roomNumber: '', floorNumber: 1 })
}

function startEdit(room) {
  editingId.value = room.id
  Object.assign(form, {
    buildingId: room.buildingId,
    roomTypeId: room.roomTypeId,
    roomNumber: room.roomNumber,
    floorNumber: room.floorNumber
  })
  roomStatusForm.status = room.status
  roomStatusForm.maintenanceReason = room.maintenanceReason || ''
  emit('select', room)
}

function handleSubmit() {
  emit('save', { id: editingId.value, payload: { ...form } })
}

function handleStatusUpdate() {
  emit('update-status', { id: editingId.value, payload: { ...roomStatusForm } })
}

// Helpers
function resolveBuildingName(id) {
  return props.buildings.find(b => b.id === id)?.name || 'Unknown'
}

function resolveRoomTypeName(room) {
  const typeId = room.roomTypeId || room.roomType?.id
  return props.roomTypes.find(t => t.id === typeId)?.typeName || room.roomType?.typeName || 'Unknown'
}

function resolveRoomCapacity(room) {
  const typeId = room.roomTypeId || room.roomType?.id
  return props.roomTypes.find(t => t.id === typeId)?.capacity || room.roomType?.capacity || 0
}

function translateStatus(s) {
  const map = { AVAILABLE: 'Sẵn sàng', FULL: 'Đã đầy', UNDER_MAINTENANCE: 'Bảo trì' }
  return map[s] || s
}

function roomTone(s) {
  return s === 'AVAILABLE' ? 'success' : s === 'FULL' ? 'neutral' : 'warning'
}

function getBadgeClass(tone) {
  if (tone === 'success') return 'bg-green-100 text-green-700'
  if (tone === 'warning') return 'bg-orange-100 text-orange-700'
  return 'bg-gray-100 text-gray-700'
}

defineExpose({ resetForm })
</script>
