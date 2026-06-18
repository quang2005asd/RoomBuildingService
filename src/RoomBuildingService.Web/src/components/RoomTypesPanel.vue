<template>
  <article class="grid grid-cols-1 lg:grid-cols-12 gap-6">
    <div class="lg:col-span-8 bg-surface-container-lowest border border-outline-variant rounded-xl overflow-hidden shadow-sm">
      <div class="px-6 py-4 border-b border-outline-variant bg-surface-container-low flex justify-between items-center">
        <h3 class="font-bold text-base">Loại Phòng</h3>
        <input 
          v-model="localSearch" 
          class="px-3 py-1 text-xs bg-white border border-outline-variant rounded-md outline-none focus:ring-1 focus:ring-primary w-48" 
          placeholder="Lọc loại phòng..." 
        />
      </div>
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead class="bg-surface-container-low/50 text-[10px] text-on-surface-variant uppercase font-bold tracking-widest border-b border-outline-variant">
            <tr class="text-xs">
              <th class="px-6 py-4">Tên loại</th>
              <th class="px-6 py-4">Sức chứa</th>
              <th class="px-6 py-4">Đơn giá</th>
              <th class="px-6 py-4 text-right">Thao tác</th>
            </tr>
          </thead>
          <transition-group name="stagger" tag="tbody" class="divide-y divide-outline-variant">
            <tr 
              v-for="type in paginatedRoomTypes" :key="type.id"
              @click="$emit('select', type)"
              class="hover:bg-surface-container-low/40 cursor-pointer transition-all group"
              :class="{ 'bg-secondary-container/20': selectedId === type.id }"
            >
              <td class="px-6 py-4">
                <div class="font-bold text-base">{{ type.typeName }}</div>
                <div class="text-xs text-on-surface-variant opacity-60 truncate max-w-[250px]">{{ type.description || '—' }}</div>
                <div class="flex flex-wrap gap-1 mt-2">
                  <span v-for="a in type.amenities" :key="a" class="px-1.5 py-0.5 bg-surface-container-high rounded text-[9px] font-bold text-primary">{{ a }}</span>
                </div>
              </td>
              <td class="px-6 py-4 text-base font-bold">{{ type.capacity }} người</td>
              <td class="px-6 py-4 text-base font-bold text-primary">{{ formatCurrency(type.basePrice) }}</td>
              <td class="px-6 py-4 text-right space-x-1" @click.stop>
                <button @click="startEdit(type)" class="p-1.5 hover:bg-surface-container-high rounded transition-all text-primary">
                  <span class="material-symbols-outlined text-sm">edit</span>
                </button>
                <button @click="$emit('delete', type.id)" :disabled="busyKey === `roomtype-delete-${type.id}`" class="p-1.5 hover:bg-error-container hover:text-error rounded transition-all">
                  <span class="material-symbols-outlined text-sm">delete</span>
                </button>
              </td>
            </tr>
          </transition-group>
        </table>
      </div>
      <div class="px-6 py-3 border-t border-outline-variant flex items-center justify-between bg-surface-container-low/30 text-[10px]">
        <span class="text-on-surface-variant">Hiển thị {{ paginatedRoomTypes.length }} / {{ filteredRoomTypes.length }} loại phòng</span>
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
          <h4 class="font-bold text-sm">{{ editingId ? 'Cập nhật' : 'Thêm mới' }} Loại Phòng</h4>
          <button v-if="editingId" @click="resetForm" class="text-xs text-primary hover:underline">Đặt lại</button>
        </div>
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div class="space-y-1">
            <label class="text-[10px] font-bold opacity-50 uppercase">Tên loại phòng</label>
            <input v-model.trim="form.typeName" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none focus:ring-1 focus:ring-primary" placeholder="VD: Standard 4, VIP 2..." required />
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold opacity-50 uppercase">Sức chứa tối đa</label>
              <input v-model.number="form.capacity" type="number" min="1" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none" required />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold opacity-50 uppercase">Đơn giá (VND)</label>
              <input v-model.number="form.basePrice" type="number" min="0" step="1000" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none" required />
            </div>
          </div>
          <div class="space-y-1">
            <label class="text-[10px] font-bold opacity-50 uppercase">Tiện ích (phân cách bằng dấu phẩy)</label>
            <input v-model="amenitiesInput" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-xs outline-none" placeholder="Wifi, Điều hòa, WC riêng..." />
          </div>
          <div class="space-y-1">
            <label class="text-[10px] font-bold opacity-50 uppercase">Mô tả</label>
            <textarea v-model.trim="form.description" rows="3" class="w-full px-3 py-2 bg-surface-container border border-outline-variant rounded-lg text-sm outline-none resize-none" placeholder="Ghi chú thêm về trang thiết bị..."></textarea>
          </div>
          <button type="submit" :disabled="busyKey === 'roomtype-save'" class="w-full bg-primary text-on-primary font-bold py-3 rounded-lg text-sm active:scale-95 transition-all mt-4 disabled:opacity-50">
            {{ editingId ? 'Lưu thay đổi' : 'Tạo mới ngay' }}
          </button>
        </form>
      </div>
    </div>
  </article>
</template>

<script setup>
import { ref, reactive, computed, watch } from 'vue'

const props = defineProps({
  roomTypes: { type: Array, required: true },
  selectedId: { type: String, default: '' },
  busyKey: { type: String, default: '' }
})

const emit = defineEmits(['save', 'delete', 'select'])

const localSearch = ref('')
const currentPage = ref(1)
const itemsPerPage = 8
const editingId = ref('')
const amenitiesInput = ref('')

const form = reactive({
  typeName: '',
  capacity: 4,
  basePrice: 0,
  description: ''
})

const filteredRoomTypes = computed(() => {
  if (!localSearch.value.trim()) return props.roomTypes
  const q = localSearch.value.toLowerCase()
  return props.roomTypes.filter(t => 
    t.typeName.toLowerCase().includes(q) || 
    (t.description && t.description.toLowerCase().includes(q))
  )
})

const totalPages = computed(() => Math.ceil(filteredRoomTypes.value.length / itemsPerPage))

const paginatedRoomTypes = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage
  return filteredRoomTypes.value.slice(start, start + itemsPerPage)
})

watch(localSearch, () => {
  currentPage.value = 1
})

function resetForm() {
  editingId.value = ''
  amenitiesInput.value = ''
  Object.assign(form, {
    typeName: '',
    capacity: 4,
    basePrice: 0,
    description: ''
  })
}

function startEdit(type) {
  editingId.value = type.id
  Object.assign(form, {
    typeName: type.typeName,
    capacity: type.capacity,
    basePrice: type.basePrice,
    description: type.description || ''
  })
  amenitiesInput.value = (type.amenities || []).join(', ')
  emit('select', type)
}

function handleSubmit() {
  // Ensure unique amenities to prevent Backend index violation
  const uniqueAmenities = [...new Set(
    amenitiesInput.value.split(',')
      .map(s => s.trim())
      .filter(Boolean)
  )]
  
  const payload = {
    ...form,
    amenities: uniqueAmenities
  }
  emit('save', { id: editingId.value, payload })
}

function formatCurrency(v) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(v)
}

defineExpose({ resetForm })
</script>
