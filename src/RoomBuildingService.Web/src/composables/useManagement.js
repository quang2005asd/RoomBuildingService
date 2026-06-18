import { ref, reactive } from 'vue'
import { api } from '../services/api'

export function useManagement(showToast, openConfirm) {
  const loading = ref(false)
  const busyKey = ref('')
  const buildings = ref([])
  const roomTypes = ref([])
  const rooms = ref([])
  const beds = ref([])
  const equipments = ref([])
  const selectedRoomId = ref('')

  async function withBusy(key, action) {
    if (busyKey.value) return // Prevent concurrent clicks if already busy
    busyKey.value = key
    try { 
      await action() 
    } 
    catch (e) { 
      console.error(`Error in ${key}:`, e)
      showToast('error', 'Lỗi thao tác', e.message || 'Cơ sở dữ liệu từ chối yêu cầu.') 
    }
    finally { busyKey.value = '' }
  }

  async function refreshAll() {
    loading.value = true
    try {
      const [b, t, r] = await Promise.all([api.getBuildings(), api.getRoomTypes(), api.getRooms()])
      buildings.value = b || []
      roomTypes.value = t || []
      rooms.value = r || []
      if (rooms.value.length && !selectedRoomId.value) {
        selectedRoomId.value = rooms.value[0].id
      }
    } catch (e) {
      showToast('error', 'Kết nối thất bại', e.message)
      throw e
    } finally {
      loading.value = false
    }
  }

  async function saveBuilding({ id, payload }) {
    await withBusy('building-save', async () => {
      const data = {
        Name: payload.name,
        TotalFloors: payload.totalFloors,
        GenderType: payload.genderType,
        Status: payload.status,
        Description: payload.description
      }
      if (id) await api.updateBuilding(id, data)
      else await api.createBuilding(data)
      showToast('success', 'Hoàn tất', 'Tòa nhà đã được lưu')
      await refreshAll()
    })
  }

  async function deleteBuilding(id) {
    openConfirm('Xóa tòa nhà này?', async () => {
      await withBusy('building-delete', async () => {
        await api.deleteBuilding(id)
        showToast('success', 'Đã xóa', 'Dữ liệu đã đồng bộ')
        await refreshAll()
      })
    })
  }

  async function saveRoomType({ id, payload }) {
    await withBusy('roomtype-save', async () => {
      // DTO fields for RoomType must match PascalCase from the C# controller
      const aList = Array.isArray(payload.amenities) ? payload.amenities : [];
      const data = {
        TypeName: payload.typeName,
        Capacity: Number(payload.capacity),
        BasePrice: Number(payload.basePrice),
        Amenities: aList.map(a => String(a).trim()).filter(Boolean),
        Description: payload.description || ''
      }
      
      try {
        if (id) await api.updateRoomType(id, data)
        else await api.createRoomType(data)
        showToast('success', 'Hoàn tất', id ? 'Đã cập nhật loại phòng' : 'Đã tạo loại phòng mới')
        await refreshAll()
      } catch (err) {
        console.error("RoomType save error details:", err);
        throw err; // Re-throw to be caught by withBusy
      }
    })
  }

  async function deleteRoomType(id) {
    openConfirm('Gỡ bỏ mẫu loại phòng này?', async () => {
      await withBusy('roomtype-delete', async () => {
        await api.deleteRoomType(id)
        showToast('success', 'Thành công', 'Dữ liệu đã được gỡ')
        await refreshAll()
      })
    })
  }

  async function saveRoom({ id, payload }) {
    await withBusy('room-save', async () => {
      const data = {
        RoomTypeId: payload.roomTypeId,
        RoomNumber: payload.roomNumber,
        FloorNumber: payload.floorNumber
      }
      if (id) await api.updateRoom(id, data)
      else await api.createRoom(data)
      showToast('success', 'Hoàn tất', 'Phòng ở đã được lưu')
      await refreshAll()
    })
  }

  async function updateRoomStatus({ id, payload }) {
    await withBusy('room-status-update', async () => {
      const data = {
        Status: payload.status,
        MaintenanceReason: payload.maintenanceReason
      }
      await api.updateRoomStatus(id, data)
      showToast('success', 'Cập nhật', 'Tình trạng phòng đã thay đổi')
      await refreshAll()
    })
  }

  async function deleteRoom(id) {
    openConfirm('Bạn có chắc muốn xóa phòng này?', async () => {
      await withBusy('room-delete', async () => {
        await api.deleteRoom(id)
        showToast('success', 'Đã xóa', 'Phòng đã gỡ khỏi hệ thống')
        await refreshAll()
      })
    })
  }

  async function loadRoomDetails(roomId) {
    selectedRoomId.value = roomId
    if (roomId) {
      const [b, e] = await Promise.all([api.getBeds(roomId), api.getEquipments(roomId)])
      beds.value = b || []
      equipments.value = e || []
    }
  }

  async function saveBed({ id, payload }) {
    await withBusy('bed-save', async () => {
      const data = {
        BedNumber: payload.bedNumber,
        Status: payload.status || 'AVAILABLE'
      }
      if (id) await api.updateBed(id, data)
      else await api.createBed({ ...data, RoomId: selectedRoomId.value })
      showToast('success', 'Thêm mới', 'Giường đã được cấu hình')
      await loadRoomDetails(selectedRoomId.value)
    })
  }

  async function deleteBed(id) {
    openConfirm('Xóa giường này khỏi danh sách?', async () => {
      await withBusy('bed-delete', async () => {
        await api.deleteBed(id)
        showToast('success', 'Xóa bỏ', 'Giường đã được gỡ')
        await loadRoomDetails(selectedRoomId.value)
      })
    })
  }

  async function saveEquipment({ roomId, payload }) {
    await withBusy('equipment-save', async () => {
      const data = {
        RoomId: roomId,
        EquipmentName: payload.equipmentName,
        Status: 'ACTIVE'
      }
      await api.createEquipment(data)
      showToast('success', 'Thành công', 'Thiết bị đã được thêm vào phòng')
      await loadRoomDetails(roomId)
    })
  }

  async function updateEquipmentStatus({ id, payload }) {
    await withBusy('equipment-status-update', async () => {
      const data = {
        Status: payload.status
      }
      await api.updateEquipmentStatus(id, data)
      showToast('success', 'Cập nhật', 'Tình trạng thiết bị đã lưu')
      await loadRoomDetails(selectedRoomId.value)
    })
  }

  async function deleteEquipment(id) {
    openConfirm('Gỡ trang thiết bị này?', async () => {
      await withBusy('equipment-delete', async () => {
        await api.deleteEquipment(id)
        showToast('success', 'Gỡ bỏ', 'Thiết bị đã được xóa')
        await loadRoomDetails(selectedRoomId.value)
      })
    })
  }

  return {
    loading, busyKey, buildings, roomTypes, rooms, beds, equipments, selectedRoomId,
    refreshAll, saveBuilding, deleteBuilding, saveRoomType, deleteRoomType,
    saveRoom, updateRoomStatus, deleteRoom, loadRoomDetails,
    saveBed, deleteBed, saveEquipment, updateEquipmentStatus, deleteEquipment
  }
}
