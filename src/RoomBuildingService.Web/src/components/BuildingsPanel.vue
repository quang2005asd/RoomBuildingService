<template>
  <article class="grid grid-cols-1 lg:grid-cols-12 gap-6">
    <div class="lg:col-span-8 bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden shadow-sm">
      <div class="px-6 py-4 border-b border-outline-variant bg-surface-container-low flex justify-between items-center">
        <h3 class="font-bold text-sm">Quản lý Tòa nhà</h3>
        <div class="flex gap-2">
          <input 
            v-model="localSearch" 
            class="px-3 py-1 text-xs bg-white border border-outline-variant rounded-md outline-none focus:ring-1 focus:ring-primary w-48" 
            placeholder="Tìm kiếm tòa nhà..." 
          />
        </div>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead class="bg-surface-container-low/50 text-[10px] text-on-surface-variant uppercase font-bold tracking-widest border-b border-outline-variant">
            <tr>
              <th class="px-6 py-4">Tòa nhà</th>
              <th class="px-6 py-4 text-center">Tầng</th>
              <th class="px-6 py-4">Loại</th>
              <th class="px-6 py-4">Trạng thái</th>
              <th class="px-6 py-4 text-right">Hành động</th>
            </tr>
          </thead>
          <transition-group name="stagger" tag="tbody" class="divide-y divide-outline-variant">
            <tr 
              v-for="building in paginatedBuildings" :key="building.id"
              @click="$emit('select', building)"
              class="hover:bg-surface-container-low/40 cursor-pointer transition-all group"
              :class="{ 'bg-secondary-container/20': selectedId === building.id }"
            >
              <td class="px-6 py-4">
                <div class="font-bold text-base">{{ building.name }}</div>
                <div class="text-xs text-on-surface-variant opacity-60 truncate max-w-[200px]">{{ building.description || '—' }}</div>
              </td>
              <td class="px-6 py-4 text-center text-base font-label-mono">{{ building.totalFloors }}</td>
              <td class="px-6 py-4 text-sm">{{ normalizeGenderLabel(building.genderType) }}</td>
              <td class="px-6 py-4 text-sm font-bold uppercase">
                <span :class="getBadgeClass(buildingTone(building.status))" class="px-2 py-1 rounded text-xs">
                  {{ translateStatus(building.status) }}
                </span>
              </td>
              <td class="px-6 py-4 text-right space-x-1" @click.stop>
                <button @click="startEdit(building)" class="p-1.5 hover:bg-surface-container-high rounded transition-all text-primary">
                  <span class="material-symbols-outlined text-sm">edit</span>
                </button>
                <button @click="$emit('delete', building.id)" :disabled="busyKey === `building-delete-${building.id}`" class="p-1.5 hover:bg-error-container hover:text-error rounded transition-all">
                  <span class="material-symbols-outlined text-sm">delete</span>
                </button>
              </td>
            </tr>
          </transition-group>
        </table>
      </div>
      <!-- Pagination -->
      <div class="px-6 py-3 border-t border-outline-variant flex items-center justify-between bg-surface-container-low/30 text-[10px]">
        <span class="text-on-surface-variant">Hiển thị {{ paginatedBuildings.length }} / {{ filteredBuildings.length }} tòa nhà</span>
        <div class="flex gap-1">
          <button @click="currentPage--" :disabled="currentPage === 1" class="w-8 h-8 flex items-center justify-center rounded hover:bg-surface-container-high disabled:opacity-30">
            <span class="material-symbols-outlined text-sm">chevron_left</span>
          </button>
          <button @click="currentPage++" :disabled="currentPage >= totalPages" class="w-8 h-8 flex items-center justify-center rounded hover:bg-surface-container-high disabled:opacity-30">
            <span class="material-symbols-outlined text-sm">chevron_right</span>
          </button>
        </div>
      </div>
    </div>
    <div class="lg:col-span-4">
      <div class="bg-surface-container-lowest border border-outline-variant rounded-xl p-6 shadow-sm sticky top-6">
        <div class="flex justify-between items-center mb-6">
          <h4 class="font-bold text-sm">{{ editingId ? 'Sửa' : 'Tạo' }} Tòa nhà</h4>
          <button v-if="editingId" @click="resetForm" class="text-xs text-primary hover:underline">Hủy sửa</button>
        </div>
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div class="space-y-1">
            <label class="text-[10px] font-bold opacity-50 uppercase tracking-wider">Tên tòa nhà</label>
            <input 
              v-model.trim="form.name" 
              class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all" 
              placeholder="VD: Tòa A1, KTX Khu B..." 
              required 
            />
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold opacity-50 uppercase tracking-wider">Số tầng</label>
              <input v-model.number="form.totalFloors" type="number" min="1" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary" required />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold opacity-50 uppercase tracking-wider">Đối tượng</label>
              <select v-model="form.genderType" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary">
                <option value="MALE">Nam</option>
                <option value="FEMALE">Nữ</option>
                <option value="MIXED">Hỗn hợp</option>
              </select>
            </div>
          </div>
          <div class="space-y-1">
            <label class="text-[10px] font-bold opacity-50 uppercase tracking-wider">Trạng thái vận hành</label>
            <select v-model="form.status" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm font-bold focus:ring-1 focus:ring-primary">
              <option value="ACTIVE">ĐANG HOẠT ĐỘNG</option>
              <option value="INACTIVE">TẠM NGỪNG</option>
              <option value="UNDER_MAINTENANCE">ĐANG BẢO TRÌ</option>
            </select>
          </div>
          <div class="space-y-1">
            <label class="text-[10px] font-bold opacity-50 uppercase tracking-wider">Mô tả thêm</label>
            <textarea 
              v-model.trim="form.description" 
              rows="3"
              class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary resize-none"
              placeholder="Thông tin chi tiết về vị trí, số phòng..."
            ></textarea>
          </div>
          <button 
            type="submit" 
            :disabled="busyKey === 'building-save'" 
            class="w-full bg-primary text-on-primary font-bold py-3 rounded-lg text-sm active:scale-95 transition-all mt-4 disabled:opacity-50 flex items-center justify-center gap-2"
          >
            <span v-if="busyKey === 'building-save'" class="material-symbols-outlined animate-spin text-sm">sync</span>
            {{ editingId ? 'Lưu thay đổi' : 'Tạo tòa nhà mới' }}
          </button>
        </form>
      </div>
    </div>
  </article>
</template>

<script setup>
import { ref, reactive, computed, watch } from 'vue'

const props = defineProps({
  buildings: { type: Array, required: true },
  selectedId: { type: String, default: '' },
  busyKey: { type: String, default: '' }
})

const emit = defineEmits(['save', 'delete', 'select', 'refresh'])

const localSearch = ref('')
const currentPage = ref(1)
const itemsPerPage = 8
const editingId = ref('')

const form = reactive({
  name: '',
  totalFloors: 1,
  genderType: 'MIXED',
  status: 'ACTIVE',
  description: ''
})

const filteredBuildings = computed(() => {
  if (!localSearch.value.trim()) return props.buildings
  const q = localSearch.value.toLowerCase()
  return props.buildings.filter(b => 
    b.name.toLowerCase().includes(q) || 
    (b.description && b.description.toLowerCase().includes(q))
  )
})

const totalPages = computed(() => Math.ceil(filteredBuildings.value.length / itemsPerPage))

const paginatedBuildings = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  return filteredBuildings.value.slice(start, start + itemsPerPage)
})

watch(localSearch, () => {
  currentPage.value = 1
})

function resetForm() {
  editingId.value = ''
  Object.assign(form, {
    name: '',
    totalFloors: 1,
    genderType: 'MIXED',
    status: 'ACTIVE',
    description: ''
  })
}

function startEdit(building) {
  editingId.value = building.id
  Object.assign(form, {
    name: building.name,
    totalFloors: building.totalFloors,
    genderType: building.genderType,
    status: building.status,
    description: building.description || ''
  })
  emit('select', building)
}

function handleSubmit() {
  emit('save', { id: editingId.value, payload: { ...form } })
  if (!editingId.value) {
    // We don't reset immediately here because we wait for parent to handle API and refresh
    // But usually we can reset after a successful save which is handled via events or props
  }
}

// Helper methods (ported from App.vue or simplified)
function normalizeGenderLabel(g) {
  const map = { MALE: 'Nam', FEMALE: 'Nữ', MIXED: 'Hỗn hợp' }
  return map[g] || g
}

function translateStatus(s) {
  const map = { ACTIVE: 'Hoạt động', INACTIVE: 'Dừng', UNDER_MAINTENANCE: 'Bảo trì' }
  return map[s] || s
}

function buildingTone(s) {
  return s === 'ACTIVE' ? 'success' : s === 'UNDER_MAINTENANCE' ? 'warning' : 'neutral'
}

function getBadgeClass(tone) {
  if (tone === 'success') return 'bg-green-100 text-green-700'
  if (tone === 'warning') return 'bg-orange-100 text-orange-700'
  return 'bg-gray-100 text-gray-700'
}

// Expose reset to parent if needed
defineExpose({ resetForm })
</script>
