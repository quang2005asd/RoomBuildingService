const baseUrl = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:5285').replace(/\/$/, '')

async function request(path, options = {}) {
  const response = await fetch(`${baseUrl}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers || {})
    },
    ...options
  })

  if (!response.ok) {
    let detail = ''
    try {
      const body = await response.json()
      detail = body.detail || body.title || body.message || JSON.stringify(body)
    } catch {
      detail = await response.text()
    }

    throw new Error(detail || `Request failed with status ${response.status}`)
  }

  if (response.status === 204) {
    return null
  }

  const text = await response.text()
  return text ? JSON.parse(text) : null
}

function queryString(params = {}) {
  const search = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value !== null && value !== undefined && value !== '') {
      search.set(key, value)
    }
  })
  return search.toString()
}

export const api = {
  getBuildings() {
    return request('/api/buildings')
  },
  getBuilding(id) {
    return request(`/api/buildings/${id}`)
  },
  createBuilding(payload) {
    return request('/api/buildings', {
      method: 'POST',
      body: JSON.stringify(payload)
    })
  },
  updateBuilding(id, payload) {
    return request(`/api/buildings/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    })
  },
  deleteBuilding(id) {
    return request(`/api/buildings/${id}`, { method: 'DELETE' })
  },
  getRoomTypes() {
    return request('/api/roomtypes')
  },
  createRoomType(payload) {
    return request('/api/roomtypes', {
      method: 'POST',
      body: JSON.stringify(payload)
    })
  },
  updateRoomType(id, payload) {
    return request(`/api/roomtypes/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    })
  },
  deleteRoomType(id) {
    return request(`/api/roomtypes/${id}`, { method: 'DELETE' })
  },
  getRooms(filters = {}) {
    const query = queryString(filters)
    return request(`/api/rooms${query ? `?${query}` : ''}`)
  },
  getRoom(id) {
    return request(`/api/rooms/${id}`)
  },
  getFloorMap(buildingId, floor) {
    const query = queryString({ buildingId, floor })
    return request(`/api/rooms/floormap?${query}`)
  },
  createRoom(payload) {
    return request('/api/rooms', {
      method: 'POST',
      body: JSON.stringify(payload)
    })
  },
  updateRoom(id, payload) {
    return request(`/api/rooms/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    })
  },
  updateRoomStatus(id, payload) {
    return request(`/api/rooms/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify(payload)
    })
  },
  deleteRoom(id) {
    return request(`/api/rooms/${id}`, { method: 'DELETE' })
  },
  getBeds(roomId) {
    const query = queryString({ roomId })
    return request(`/api/beds?${query}`)
  },
  getBed(id) {
    return request(`/api/beds/${id}`)
  },
  createBed(payload) {
    return request('/api/beds', {
      method: 'POST',
      body: JSON.stringify(payload)
    })
  },
  updateBed(id, payload) {
    return request(`/api/beds/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload)
    })
  },
  deleteBed(id) {
    return request(`/api/beds/${id}`, { method: 'DELETE' })
  },
  getEquipments(roomId) {
    const query = queryString({ roomId })
    return request(`/api/equipments?${query}`)
  },
  getEquipment(id) {
    return request(`/api/equipments/${id}`)
  },
  createEquipment(payload) {
    return request('/api/equipments', {
      method: 'POST',
      body: JSON.stringify(payload)
    })
  },
  updateEquipmentStatus(id, payload) {
    return request(`/api/equipments/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify(payload)
    })
  },
  deleteEquipment(id) {
    return request(`/api/equipments/${id}`, { method: 'DELETE' })
  }
}
