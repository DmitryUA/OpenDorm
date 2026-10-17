<template>
  <section class="dormitory-details-page">
    <div class="page-toolbar">
      <el-button text :icon="ArrowLeft" @click="router.push({ name: 'Dormitories' })">
        Все общежития
      </el-button>
    </div>

    <el-alert v-if="detailsError" :title="detailsError" type="error" :closable="false" show-icon>
      <template #default>
        <el-button link type="primary" @click="loadDetails">Повторить загрузку</el-button>
      </template>
    </el-alert>

    <div v-loading="isDetailsLoading" class="details-content">
      <template v-if="details">
        <header class="page-header">
          <div class="heading-address">
            <el-icon class="address-icon"><OfficeBuilding /></el-icon>
            <div>
              <h1 class="page-title">
                {{ details.city }}, {{ details.street }}, {{ details.house }}
              </h1>
              <p class="page-subtitle">Информация об общежитии</p>
            </div>
          </div>
          <el-button v-if="canCreateRoom" type="primary" :icon="Plus" @click="openCreateDialog">
            Добавить комнату
          </el-button>
        </header>

        <div class="summary-grid">
          <div class="summary-item">
            <span class="summary-value">{{ details.totalRoomCount }}</span>
            <span class="summary-label">Комнат</span>
          </div>
          <div class="summary-item">
            <span class="summary-value">{{ details.totalSeatCount }}</span>
            <span class="summary-label">Мест</span>
          </div>
          <div class="summary-item">
            <span class="summary-value">{{ details.totalAvailablePlaceCount }}</span>
            <span class="summary-label">Свободных мест</span>
          </div>
          <div class="summary-item">
            <span class="summary-value">{{ details.totalFloorCount }}</span>
            <span class="summary-label">Этажей</span>
          </div>
        </div>
      </template>
    </div>

    <section v-if="details" class="rooms-section">
      <header class="section-header">
        <div>
          <h2 class="section-title">Комнаты</h2>
          <p class="section-subtitle">{{ roomsResult?.totalCount ?? 0 }} комнат</p>
        </div>
      </header>

      <el-form class="room-filters" inline @submit.prevent="applyFilters">
        <el-form-item label="Номер комнаты">
          <el-input
            v-model="roomNameFilter"
            placeholder="Например, 214"
            clearable
            @keyup.enter="applyFilters"
            @clear="applyFilters"
          />
        </el-form-item>
        <el-form-item label="Статус">
          <el-select
            v-model="statusFilter"
            placeholder="Все комнаты"
            clearable
            @change="applyFilters"
            @clear="applyFilters"
          >
            <el-option label="Активные" :value="true" />
            <el-option label="Неактивные" :value="false" />
          </el-select>
        </el-form-item>
        <el-button type="primary" :loading="isRoomsLoading" @click="applyFilters">
          Применить
        </el-button>
      </el-form>

      <el-alert v-if="roomsError" :title="roomsError" type="error" :closable="false" show-icon>
        <template #default>
          <el-button link type="primary" @click="loadRooms(currentPage)">
            Повторить загрузку
          </el-button>
        </template>
      </el-alert>

      <div v-loading="isRoomsLoading" class="rooms-content">
        <div v-if="roomsResult?.items.length" class="room-grid">
          <article v-for="room in roomsResult.items" :key="room.id" class="room-card">
            <button type="button" class="room-card-link" @click="openRoom(room)">
              <header class="room-card-header">
                <h3 class="room-name">Комната {{ room.name }}</h3>
                <el-tag :type="room.isActive ? 'success' : 'info'" effect="light">
                  {{ room.isActive ? 'Активна' : 'Неактивна' }}
                </el-tag>
              </header>
              <dl class="room-details">
                <div>
                  <dt>Этаж</dt>
                  <dd>{{ room.floorNumber }}</dd>
                </div>
                <div>
                  <dt>Тип</dt>
                  <dd>{{ genderLabel(room.gender) }}</dd>
                </div>
                <div>
                  <dt>Жильцы</dt>
                  <dd>{{ room.occupantCount }} / {{ room.capacity }}</dd>
                </div>
              </dl>
            </button>
          </article>
        </div>

        <el-empty v-else-if="!isRoomsLoading && !roomsError" description="Комнаты не найдены" />
      </div>

      <el-pagination
        v-if="Number(roomsResult?.totalCount ?? 0) > 0"
        background
        layout="prev, pager, next, total"
        :current-page="currentPage"
        :page-size="Number(roomsResult?.pageSize ?? PAGE_SIZE)"
        :total="Number(roomsResult?.totalCount ?? 0)"
        :disabled="isRoomsLoading"
        @current-change="loadRooms"
      />
    </section>

    <el-dialog
      v-model="isCreateDialogVisible"
      title="Новая комната"
      width="min(520px, calc(100vw - 32px))"
      destroy-on-close
      @closed="resetForm"
    >
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-position="top"
        @submit.prevent="handleCreate"
      >
        <el-form-item label="Номер комнаты" prop="roomName">
          <el-input v-model="form.roomName" maxlength="128" />
        </el-form-item>
        <el-form-item label="Тип комнаты" prop="gender">
          <el-select v-model="form.gender" class="full-width">
            <el-option label="Мужская" :value="0" />
            <el-option label="Женская" :value="1" />
          </el-select>
        </el-form-item>
        <el-form-item label="Количество мест" prop="capacity">
          <el-input-number v-model="form.capacity" :min="1" :precision="0" />
        </el-form-item>
        <el-form-item label="Этаж" prop="floorNumber">
          <el-input-number v-model="form.floorNumber" :min="1" :max="maximumFloor" :precision="0" />
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-actions">
          <el-button @click="isCreateDialogVisible = false">Отмена</el-button>
          <el-button type="primary" :loading="isSubmitting" @click="handleCreate">
            Добавить
          </el-button>
        </div>
      </template>
    </el-dialog>
  </section>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { ArrowLeft, OfficeBuilding, Plus } from '@element-plus/icons-vue'
import { getDormitories } from '@/shared/api/generated/dormitories'
import type {
  CreateRoomRequest,
  DormitoryDetailsDto,
  Gender,
  GetApiDormitoriesIdRoomsParams,
  PagedResultOfRoomListDto,
  RoomListDto,
} from '@/shared/api/generated/models'
import { usePermissions } from '@/shared/composables/usePermissions'

interface RoomForm {
  roomName: string
  gender: Gender
  capacity: number | undefined
  floorNumber: number | undefined
}

const PAGE_SIZE = 12

const route = useRoute()
const router = useRouter()
const { canCreateRoom } = usePermissions()
const { getApiDormitoriesIdDetails, getApiDormitoriesIdRooms, postApiDormitoriesIdAddRooms } =
  getDormitories()

const dormitoryId = computed(() => String(route.params.id ?? ''))
const details = ref<DormitoryDetailsDto>()
const roomsResult = ref<PagedResultOfRoomListDto>()
const isDetailsLoading = ref(false)
const isRoomsLoading = ref(false)
const detailsError = ref<string | null>(null)
const roomsError = ref<string | null>(null)
const roomNameFilter = ref('')
const statusFilter = ref<boolean>()
const currentPage = ref(1)
const isCreateDialogVisible = ref(false)
const isSubmitting = ref(false)
const formRef = ref<FormInstance>()

const form = reactive<RoomForm>({
  roomName: '',
  gender: 0,
  capacity: 2,
  floorNumber: 1,
})

const maximumFloor = computed(() => Math.max(Number(details.value?.totalFloorCount ?? 1), 1))

const rules: FormRules<RoomForm> = {
  roomName: [{ required: true, message: 'Укажите номер комнаты', trigger: 'blur' }],
  gender: [{ required: true, message: 'Выберите тип комнаты', trigger: 'change' }],
  capacity: [{ required: true, message: 'Укажите количество мест', trigger: 'change' }],
  floorNumber: [{ required: true, message: 'Укажите этаж', trigger: 'change' }],
}

const genderLabel = (gender: Gender | string) => {
  if (gender === 0 || gender === 'Male') return 'Мужская'
  if (gender === 1 || gender === 'Female') return 'Женская'
  return 'Не указан'
}

const loadDetails = async () => {
  if (!dormitoryId.value) return

  isDetailsLoading.value = true
  detailsError.value = null
  try {
    details.value = await getApiDormitoriesIdDetails(dormitoryId.value)
  } catch (error) {
    const apiError = error as { response?: { data?: { title?: string } } }
    detailsError.value = apiError.response?.data?.title || 'Не удалось загрузить общежитие'
  } finally {
    isDetailsLoading.value = false
  }
}

const loadRooms = async (page = 1) => {
  if (!dormitoryId.value) return

  currentPage.value = page
  isRoomsLoading.value = true
  roomsError.value = null
  const params: GetApiDormitoriesIdRoomsParams = {
    name: roomNameFilter.value.trim() || undefined,
    isActive: statusFilter.value,
    page,
    pageSize: PAGE_SIZE,
  }

  try {
    roomsResult.value = await getApiDormitoriesIdRooms(dormitoryId.value, params)
  } catch (error) {
    const apiError = error as { response?: { data?: { title?: string } } }
    roomsError.value = apiError.response?.data?.title || 'Не удалось загрузить комнаты'
  } finally {
    isRoomsLoading.value = false
  }
}

const applyFilters = () => loadRooms(1)

const openCreateDialog = () => {
  isCreateDialogVisible.value = true
}

const openRoom = (room: RoomListDto) => {
  router.push({
    name: 'RoomDetails',
    params: { dormitoryId: dormitoryId.value, roomId: room.id },
    query: { name: room.name },
  })
}

const resetForm = () => {
  form.roomName = ''
  form.gender = 0
  form.capacity = 2
  form.floorNumber = 1
  formRef.value?.clearValidate()
}

const handleCreate = async () => {
  if (!formRef.value || form.capacity === undefined || form.floorNumber === undefined) {
    return
  }

  try {
    await formRef.value.validate()
  } catch {
    return
  }

  isSubmitting.value = true
  const request: CreateRoomRequest = {
    roomName: form.roomName.trim(),
    gender: form.gender,
    capacity: form.capacity,
    floorNumber: form.floorNumber,
  }

  try {
    await postApiDormitoriesIdAddRooms(dormitoryId.value, request)
    ElMessage.success('Комната добавлена')
    isCreateDialogVisible.value = false
    await Promise.all([loadDetails(), loadRooms(currentPage.value)])
  } catch (error) {
    const apiError = error as { response?: { data?: { title?: string } } }
    ElMessage.error(apiError.response?.data?.title || 'Не удалось добавить комнату')
  } finally {
    isSubmitting.value = false
  }
}

watch(
  dormitoryId,
  () => {
    roomNameFilter.value = ''
    statusFilter.value = undefined
    currentPage.value = 1
    void Promise.all([loadDetails(), loadRooms(1)])
  },
  { immediate: true },
)
</script>

<style scoped>
.dormitory-details-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.page-toolbar {
  margin-bottom: calc(var(--spacing-sm) * -1);
}

.details-content {
  min-height: 96px;
}

.page-header,
.section-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--spacing-md);
}

.heading-address {
  display: flex;
  align-items: flex-start;
  gap: var(--spacing-sm);
  min-width: 0;
}

.address-icon {
  flex: 0 0 auto;
  margin-top: 4px;
  color: var(--color-primary);
  font-size: var(--font-size-xl);
}

.page-title,
.section-title {
  margin: 0;
  color: var(--color-text-primary);
  font-size: var(--font-size-2xl);
  font-weight: 600;
  overflow-wrap: anywhere;
}

.page-subtitle,
.section-subtitle {
  margin: var(--spacing-xs) 0 0;
  color: var(--color-text-secondary);
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  border-top: 1px solid var(--color-border);
  border-bottom: 1px solid var(--color-border);
}

.summary-item {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
  padding: var(--spacing-md);
}

.summary-item + .summary-item {
  border-left: 1px solid var(--color-border);
}

.summary-value {
  color: var(--color-text-primary);
  font-size: var(--font-size-xl);
  font-weight: 600;
}

.summary-label {
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
}

.rooms-section {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.section-title {
  font-size: var(--font-size-xl);
}

.room-filters {
  display: flex;
  align-items: flex-end;
  gap: var(--spacing-sm);
  padding: var(--spacing-md) 0;
  border-top: 1px solid var(--color-border-light);
  border-bottom: 1px solid var(--color-border-light);
}

.room-filters :deep(.el-form-item) {
  margin: 0;
}

.room-filters :deep(.el-input) {
  width: 220px;
}

.room-filters :deep(.el-select) {
  width: 180px;
}

.rooms-content {
  min-height: 160px;
}

.room-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 260px), 1fr));
  gap: var(--spacing-md);
}

.room-card {
  min-width: 0;
  padding: var(--spacing-md);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: var(--color-bg-white);
}

.room-card-link {
  display: block;
  width: 100%;
  padding: 0;
  border: 0;
  background: transparent;
  color: inherit;
  cursor: pointer;
  text-align: left;
}

.room-card-link:focus-visible {
  border-radius: var(--border-radius-sm);
  outline: 2px solid var(--color-primary);
  outline-offset: 4px;
}

.room-card-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--spacing-sm);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--color-border-light);
}

.room-name {
  margin: 0;
  color: var(--color-text-primary);
  font-size: var(--font-size-md);
  font-weight: 600;
  overflow-wrap: anywhere;
}

.room-details {
  display: grid;
  gap: var(--spacing-sm);
  margin: var(--spacing-md) 0 0;
}

.room-details > div {
  display: flex;
  justify-content: space-between;
  gap: var(--spacing-sm);
}

.room-details dt {
  color: var(--color-text-secondary);
}

.room-details dd {
  margin: 0;
  color: var(--color-text-primary);
  font-weight: 500;
  text-align: right;
}

.full-width {
  width: 100%;
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-sm);
}

@media (max-width: 760px) {
  .summary-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .summary-item:nth-child(3) {
    border-left: 0;
  }

  .summary-item:nth-child(n + 3) {
    border-top: 1px solid var(--color-border);
  }

  .page-header {
    flex-direction: column;
  }

  .room-filters {
    align-items: stretch;
    flex-direction: column;
  }

  .room-filters :deep(.el-form-item),
  .room-filters :deep(.el-input),
  .room-filters :deep(.el-select) {
    width: 100%;
  }
}
</style>
