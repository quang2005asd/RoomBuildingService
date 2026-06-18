<template>
  <article class="grid grid-cols-1 lg:grid-cols-12 gap-6">
    <div class="lg:col-span-8 bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden shadow-sm min-h-[400px]">
      <div class="px-6 py-4 border-b border-outline-variant bg-surface-container-low flex flex-wrap items-center justify-between gap-4">
        <div class="flex items-center gap-3">
          <h3 class="font-bold text-sm">Danh sách giường tại</h3>
          <select v-model="localRoomId" class="px-3 py-1 bg-white border border-outline-variant rounded text-xs font-bold outline-none focus:ring-1 focus:ring-primary">
            <option value="" disabled>--- Chọn phòng ---</option>
            <option v-for="r in rooms" :key="r.id" :value="r.id">Phòng {{ r.roomNumber }}</option>
          </select>
        </div>
        <div v-if="localRoomId" class="flex gap-2">
           <input v-model="localSearch" class="px-2 py-1 text-[10px] bg-white border border-outline-variant rounded outline-none w-32" placeholder="Tìm mã giường..." />
        </div>
      </div>
      
      <div v-if="!localRoomId" class="flex flex-col items-center justify-center py-20 opacity-40">
        <span class="material-symbols-outlined text-6xl mb-4">bed</span>
        <p class="text-sm font-bold">Vui lòng chọn một phòng để quản lý giường</p>
      </div>
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead class="bg-surface-container-low/50 text-[10px] text-on-surface-variant uppercase font-bold border-b border-outline-variant">
            <tr>
              <th class="px-6 py-4">Mã số giường</th>
              <th class="px-6 py-4 text-center">Trạng thái hiện tại</th>
              <th class="px-6 py-4 text-right">Thao tác</th>
            </tr>
          </thead>
          <transition-group name="stagger" tag="tbody" class="divide-y divide-outline-variant">
            <tr 
              v-for="bed in filteredBeds" :key="bed.id"
              @click="$emit('select', bed)"
              class="hover:bg-surface-container-low/40 cursor-pointer transition-all"
              :class="{ 'bg-secondary-container/20': selectedId === bed.id }"
            >
              <td class="px-6 py-4 flex items-center gap-4">
                <div class="w-10 h-10 rounded-full bg-surface-container-high flex items-center justify-center text-primary">
                   <span class="material-symbols-outlined text-base">king_bed</span>
                </div>
                <div>
                  <div class="font-bold text-base">{{ bed.bedNumber }}</div>
                  <div class="text-[10px] opacity-50">ID: {{ bed.id.slice(0,8) }}</div>
                </div>
              </td>
              <td class="px-6 py-4 text-center">
                <span :class="getBadgeClass(bedTone(bed.status))" class="px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-wider">
                  {{ translateStatus(bed.status) }}
                </span>
              </td>
              <td class="px-6 py-4 text-right space-x-1" @click.stop>
                <button @click="startEdit(bed)" class="p-1.5 hover:bg-surface-container-high rounded text-primary transition-all"><span class="material-symbols-outlined text-sm">edit</span></button>
                <button @click="$emit('delete', bed.id)" class="p-1.5 hover:bg-error-container hover:text-error rounded transition-all"><span class="material-symbols-outlined text-sm">delete</span></button>
              </td>
            </tr>
          </transition-group>
        </table>
        <div v-if="filteredBeds.length === 0" class="py-12 text-center opacity-50 italic text-sm">
           Không tìm thấy giường nào trong phòng này.
        </div>
      </div>
    </div>
    <div class="lg:col-span-4 bg-surface-container-lowest border border-outline-variant rounded-xl p-6 shadow-sm sticky top-6 self-start">
      <h4 class="font-bold text-sm mb-6">{{ editingId ? 'Cập nhật giường' : 'Thêm giường mới' }}</h4>
      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div class="space-y-1">
          <label class="text-[10px] font-bold opacity-50 uppercase tracking-widest">Phòng đích</label>
          <select v-model="form.roomId" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm" required>
            <option value="" disabled>--- Chọn phòng ---</option>
            <option v-for="r in rooms" :key="r.id" :value="r.id">Phòng {{ r.roomNumber }}</option>
          </select>
        </div>
        <div class="space-y-1">
          <label class="text-[10px] font-bold opacity-50 uppercase tracking-widest">Mã số giường</label>
          <input v-model.trim="form.bedNumber" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm font-bold" placeholder="VD: G1, Bed-01..." required />
        </div>
        <div class="space-y-1">
          <label class="text-[10px] font-bold opacity-50 uppercase tracking-widest">Trạng thái</label>
          <select v-model="form.status" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm font-bold">
            <option value="AVAILABLE">AVAILABLE (Có sẵn)</option>
            <option value="OCCUPIED">OCCUPIED (Đã có người)</option>
            <option value="UNDER_MAINTENANCE">UNDER_MAINTENANCE (Hỏng)</option>
          </select>
        </div>
        <button 
          type="submit" 
          :disabled="!form.roomId"
          class="w-full bg-primary text-on-primary font-bold py-3 rounded-lg text-sm active:scale-95 transition-all mt-4 disabled:opacity-50"
        >
          {{ editingId ? 'Lưu thay đổi' : 'Thêm giường' }}
        </button>
        <button v-if="editingId" type="button" @click="resetForm" class="w-full py-2 text-xs font-bold text-on-surface-variant hover:bg-surface-container-high rounded transition-all">Hủy bỏ</button>
      </form>

      <div class="mt-8 p-4 bg-surface-container-low rounded-lg border border-outline-variant/50">
         <h5 class="text-[10px] font-bold opacity-70 uppercase mb-2">Quy tắc mã hóa</h5>
         <ul class="text-[10px] space-y-1 opacity-60">
           <li>• Tên giường nên phân biệt trong cùng một phòng.</li>
           <li>• Có thể sử dụng tiền tố tòa nhà (VD: A1-101-G1).</li>
         </ul>
      </div>
    </div>
  </article>
</template>

<script setup>
import { ref, reactive, watch, computed } from 'vue'

const props = defineProps({
  beds: { type: Array, required: true },
  rooms: { type: Array, required: true },
  selectedId: { type: String, default: '' },
  selectedRoomId: { type: String, default: '' }
})

const emit = defineEmits(['save', 'delete', 'select', 'change-room'])

const localRoomId = ref(props.selectedRoomId)
const localSearch = ref('')
const editingId = ref('')

const form = reactive({
  roomId: props.selectedRoomId,
  bedNumber: '',
  status: 'AVAILABLE'
})

watch(() => props.selectedRoomId, (val) => {
  localRoomId.value = val
  if (!editingId.value) form.roomId = val
})

watch(localRoomId, (val) => {
  emit('change-room', val)
  if (!editingId.value) form.roomId = val
})

const filteredBeds = computed(() => {
  const q = localSearch.value.trim().toLowerCase()
  return props.beds.filter(b => !q || b.bedNumber.toLowerCase().includes(q))
})

function resetForm() {
  editingId.value = ''
  Object.assign(form, {
    roomId: localRoomId.value,
    bedNumber: '',
    status: 'AVAILABLE'
  })
}

function startEdit(bed) {
  editingId.value = bed.id
  Object.assign(form, {
    roomId: bed.roomId || localRoomId.value,
    bedNumber: bed.bedNumber,
    status: bed.status || 'AVAILABLE'
  })
}

function handleSubmit() {
  emit('save', { id: editingId.value, payload: { ...form } })
}

function translateStatus(s) {
  const map = { AVAILABLE: 'Có sẵn', OCCUPIED: 'Đã ở', UNDER_MAINTENANCE: 'Bảo trì' }
  return map[s] || s
}

function bedTone(s) {
  return s === 'AVAILABLE' ? 'success' : s === 'OCCUPIED' ? 'neutral' : 'warning'
}

function getBadgeClass(tone) {
  if (tone === 'success') return 'bg-green-100 text-green-700'
  if (tone === 'warning') return 'bg-orange-100 text-orange-700'
  return 'bg-gray-100 text-gray-700'
}

defineExpose({ resetForm })
</script>
