<template>
  <section class="room-details-page">
    <div class="page-toolbar">
      <el-button
        text
        :icon="ArrowLeft"
        @click="router.push({ name: 'DormitoryDetails', params: { id: dormitoryId() } })"
      >
        К общежитию
      </el-button>
    </div>

    <el-alert v-if="roomError" :title="roomError" type="error" :closable="false" show-icon>
      <template #default>
        <el-button link type="primary" @click="loadRoomAndRelatedData"
          >Повторить загрузку</el-button
        >
      </template>
    </el-alert>

    <div v-loading="isRoomLoading" class="room-content">
      <template v-if="room">
        <header class="page-header">
          <div>
            <div class="room-title-row">
              <h1 class="page-title">Комната {{ room.name }}</h1>
              <el-tag :type="room.isActive ? 'success' : 'info'" effect="light">
                {{ room.isActive ? 'Активна' : 'Неактивна' }}
              </el-tag>
            </div>
            <p class="page-subtitle">{{ genderLabel(room.gender) }} комната</p>
          </div>

          <div class="room-actions">
            <el-button v-if="can('rooms.update')" :icon="Edit" @click="openEditDialog">
              Редактировать
            </el-button>
            <el-button
              v-if="room.isActive && can('rooms.deactivate')"
              type="danger"
              plain
              :loading="isStatusUpdating"
              @click="changeRoomStatus(false)"
            >
              Деактивировать
            </el-button>
            <el-button
              v-else-if="!room.isActive && can('rooms.activate')"
              type="success"
              plain
              :loading="isStatusUpdating"
              @click="changeRoomStatus(true)"
            >
              Активировать
            </el-button>
          </div>
        </header>

        <dl class="room-summary">
          <div>
            <dt>Этаж</dt>
            <dd>{{ room.floorNumber }}</dd>
          </div>
          <div>
            <dt>Вместимость</dt>
            <dd>{{ room.capacity }}</dd>
          </div>
          <div>
            <dt>Проживает</dt>
            <dd>{{ room.occupantCount }} / {{ room.capacity }}</dd>
          </div>
        </dl>
      </template>
    </div>

    <section v-if="room && can('occupants.read')" class="data-section">
      <header class="section-header">
        <div>
          <h2 class="section-title">Сейчас проживают</h2>
          <p class="section-subtitle">{{ occupants?.length ?? 0 }} жильцов</p>
        </div>
      </header>

      <el-alert
        v-if="occupantsError"
        :title="occupantsError"
        type="error"
        :closable="false"
        show-icon
      >
        <template #default>
          <el-button link type="primary" @click="loadOccupants">Повторить загрузку</el-button>
        </template>
      </el-alert>

      <div v-loading="isOccupantsLoading" class="occupants-content">
        <div v-if="occupants?.length" class="occupant-list">
          <RouterLink
            v-for="occupant in occupants"
            :key="occupant.id"
            class="occupant-item"
            :to="{ name: 'OccupantDetails', params: { id: occupant.id } }"
            :aria-label="`Открыть жильца ${fullName(occupant.lastName, occupant.firstName, occupant.patronymic)}`"
          >
            <el-avatar :size="40" class="occupant-avatar">
              {{ occupantInitials(occupant.firstName, occupant.lastName) }}
            </el-avatar>
            <div class="occupant-info">
              <h3>{{ fullName(occupant.lastName, occupant.firstName, occupant.patronymic) }}</h3>
              <p>Дата рождения: {{ formatDate(occupant.birthDate) }}</p>
            </div>
          </RouterLink>
        </div>
        <el-empty
          v-else-if="!isOccupantsLoading && !occupantsError"
          description="В комнате пока никто не проживает"
        />
      </div>
    </section>

    <section v-if="room && can('accommodations.read')" class="data-section">
      <header class="section-header">
        <div>
          <h2 class="section-title">История заселений</h2>
          <p class="section-subtitle">{{ historyResult?.totalCount ?? 0 }} записей</p>
        </div>
      </header>

      <el-form class="history-filters" inline @submit.prevent="applyHistoryFilters">
        <el-form-item label="Заезд с">
          <el-date-picker
            v-model="startCheckInDate"
            type="date"
            value-format="YYYY-MM-DD"
            placeholder="Начальная дата"
            clearable
          />
        </el-form-item>
        <el-form-item label="Заезд по">
          <el-date-picker
            v-model="endCheckInDate"
            type="date"
            value-format="YYYY-MM-DD"
            placeholder="Конечная дата"
            clearable
          />
        </el-form-item>
        <el-form-item label="Сортировка">
          <el-select v-model="sortOrder" class="sort-select">
            <el-option label="Сначала новые" :value="1" />
            <el-option label="Сначала старые" :value="0" />
          </el-select>
        </el-form-item>
        <el-button type="primary" :loading="isHistoryLoading" @click="applyHistoryFilters">
          Применить
        </el-button>
      </el-form>

      <el-alert v-if="historyError" :title="historyError" type="error" :closable="false" show-icon>
        <template #default>
          <el-button link type="primary" @click="loadHistory(historyPage)">
            Повторить загрузку
          </el-button>
        </template>
      </el-alert>

      <div v-loading="isHistoryLoading" class="history-content">
        <el-table v-if="historyResult?.items.length" :data="historyResult.items" stripe>
          <el-table-column label="Жилец" min-width="210">
            <template #default="{ row }">
              {{ fullName(row.lastName, row.firstName, row.patronymic) }}
            </template>
          </el-table-column>
          <el-table-column label="Тип комнаты" min-width="130">
            <template #default="{ row }">{{ genderLabel(row.gender) }}</template>
          </el-table-column>
          <el-table-column label="Дата заезда" min-width="130">
            <template #default="{ row }">{{ formatDate(row.checkInDate) }}</template>
          </el-table-column>
          <el-table-column label="Дата выезда" min-width="130">
            <template #default="{ row }">
              {{ row.checkOutDate ? formatDate(row.checkOutDate) : 'Проживает' }}
            </template>
          </el-table-column>
        </el-table>
        <el-empty
          v-else-if="!isHistoryLoading && !historyError"
          description="История заселений пуста"
        />
      </div>

      <el-pagination
        v-if="Number(historyResult?.totalCount ?? 0) > 0"
        background
        layout="prev, pager, next, total"
        :current-page="historyPage"
        :page-size="HISTORY_PAGE_SIZE"
        :total="Number(historyResult?.totalCount ?? 0)"
        :disabled="isHistoryLoading"
        @current-change="loadHistory"
      />
    </section>

    <el-dialog
      v-model="isEditDialogVisible"
      title="Редактировать комнату"
      width="min(520px, calc(100vw - 32px))"
      destroy-on-close
      @closed="resetEditForm"
    >
      <el-form
        ref="editFormRef"
        :model="editForm"
        :rules="editRules"
        label-position="top"
        @submit.prevent="handleUpdate"
      >
        <el-form-item label="Номер комнаты" prop="name">
          <el-input v-model="editForm.name" maxlength="128" />
        </el-form-item>
        <el-form-item label="Тип комнаты" prop="gender">
          <el-select v-model="editForm.gender" class="full-width">
            <el-option label="Мужская" :value="0" />
            <el-option label="Женская" :value="1" />
          </el-select>
        </el-form-item>
        <el-form-item label="Вместимость" prop="capacity">
          <el-input-number v-model="editForm.capacity" :min="1" :precision="0" />
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-actions">
          <el-button @click="isEditDialogVisible = false">Отмена</el-button>
          <el-button type="primary" :loading="isUpdating" @click="handleUpdate">
            Сохранить
          </el-button>
        </div>
      </template>
    </el-dialog>
  </section>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from 'element-plus'
import { ArrowLeft, Edit } from '@element-plus/icons-vue'
import { getDormitories } from '@/shared/api/generated/dormitories'
import { getRooms } from '@/shared/api/generated/rooms/rooms'
import type {
  GetApiRoomsIdHistoryParams,
  PagedResultOfRoomOccupancyHistoryListDto,
  RoomListDto,
  RoomOccupantDto,
  SortOrder,
  UpdateRoomRequest,
} from '@/shared/api/generated/models'
import { usePermissions } from '@/shared/composables/usePermissions'

interface RoomEditForm {
  name: string
  capacity: number | undefined
  gender: number
}

const ROOM_PAGE_SIZE = 100
const HISTORY_PAGE_SIZE = 10

const route = useRoute()
const router = useRouter()
const { can } = usePermissions()
const {
  getApiDormitoriesIdRooms,
  putApiDormitoriesDormitoryIdRoomsRoomId,
  postApiDormitoriesDormitoryIdRoomsRoomIdActivate,
  postApiDormitoriesDormitoryIdRoomsRoomIdDeactivate,
} = getDormitories()
const { getApiRoomsIdOccupants, getApiRoomsIdHistory } = getRooms()

const dormitoryId = () => String(route.params.dormitoryId ?? '')
const roomId = () => String(route.params.roomId ?? '')
const room = ref<RoomListDto>()
const occupants = ref<RoomOccupantDto[]>()
const historyResult = ref<PagedResultOfRoomOccupancyHistoryListDto>()
const roomError = ref<string | null>(null)
const occupantsError = ref<string | null>(null)
const historyError = ref<string | null>(null)
const isRoomLoading = ref(false)
const isOccupantsLoading = ref(false)
const isHistoryLoading = ref(false)
const isStatusUpdating = ref(false)
const isUpdating = ref(false)
const isEditDialogVisible = ref(false)
const editFormRef = ref<FormInstance>()
const historyPage = ref(1)
const startCheckInDate = ref<string>()
const endCheckInDate = ref<string>()
const sortOrder = ref<SortOrder>(1)

const editForm = reactive<RoomEditForm>({
  name: '',
  capacity: 1,
  gender: 0,
})

const editRules: FormRules<RoomEditForm> = {
  name: [{ required: true, message: 'Укажите номер комнаты', trigger: 'blur' }],
  capacity: [{ required: true, message: 'Укажите вместимость', trigger: 'change' }],
  gender: [{ required: true, message: 'Выберите тип комнаты', trigger: 'change' }],
}

const fullName = (lastName: string, firstName: string, patronymic: string | null) =>
  [lastName, firstName, patronymic].filter(Boolean).join(' ')

const occupantInitials = (firstName: string, lastName: string) =>
  `${firstName.charAt(0)}${lastName.charAt(0)}`.toLocaleUpperCase('ru-RU')

const genderLabel = (gender: number | string) => {
  if (gender === 0 || gender === 'Male') return 'Мужская'
  if (gender === 1 || gender === 'Female') return 'Женская'
  return 'Не указан'
}

const genderCode = (gender: number | string) => (gender === 1 || gender === 'Female' ? 1 : 0)

const formatDate = (value: string) => {
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? value : date.toLocaleDateString('ru-RU')
}

const loadRoom = async () => {
  const currentDormitoryId = dormitoryId()
  const currentRoomId = roomId()
  if (!currentDormitoryId || !currentRoomId) return

  isRoomLoading.value = true
  roomError.value = null
  room.value = undefined

  try {
    const queryName = typeof route.query.name === 'string' ? route.query.name : undefined
    let page = 1
    let totalPages = 1
    let foundRoom: RoomListDto | undefined

    while (page <= totalPages && !foundRoom) {
      const result = await getApiDormitoriesIdRooms(currentDormitoryId, {
        name: queryName,
        page,
        pageSize: ROOM_PAGE_SIZE,
      })
      foundRoom = result.items.find((item) => item.id === currentRoomId)
      totalPages = Math.ceil(Number(result.totalCount) / Number(result.pageSize || ROOM_PAGE_SIZE))
      page += 1
    }

    if (!foundRoom) {
      roomError.value = 'Комната не найдена в этом общежитии'
      return
    }

    room.value = foundRoom
  } catch (error) {
    const apiError = error as { response?: { data?: { title?: string } } }
    roomError.value = apiError.response?.data?.title || 'Не удалось загрузить комнату'
  } finally {
    isRoomLoading.value = false
  }
}

const loadOccupants = async () => {
  if (!room.value || !can('occupants.read')) return

  isOccupantsLoading.value = true
  occupantsError.value = null
  try {
    occupants.value = await getApiRoomsIdOccupants(room.value.id)
  } catch (error) {
    const apiError = error as { response?: { data?: { title?: string } } }
    occupantsError.value = apiError.response?.data?.title || 'Не удалось загрузить список жильцов'
  } finally {
    isOccupantsLoading.value = false
  }
}

const loadHistory = async (page = 1) => {
  if (!room.value || !can('accommodations.read')) return

  historyPage.value = page
  isHistoryLoading.value = true
  historyError.value = null
  const params: GetApiRoomsIdHistoryParams = {
    sortOrder: sortOrder.value,
    startCheckInDate: startCheckInDate.value || undefined,
    endCheckInDate: endCheckInDate.value || undefined,
    page,
    pageSize: HISTORY_PAGE_SIZE,
  }

  try {
    historyResult.value = await getApiRoomsIdHistory(room.value.id, params)
  } catch (error) {
    const apiError = error as { response?: { data?: { title?: string } } }
    historyError.value = apiError.response?.data?.title || 'Не удалось загрузить историю заселений'
  } finally {
    isHistoryLoading.value = false
  }
}

const loadRoomAndRelatedData = async () => {
  await loadRoom()
  if (!room.value) return

  await Promise.all([loadOccupants(), loadHistory(1)])
}

const applyHistoryFilters = () => {
  if (
    startCheckInDate.value &&
    endCheckInDate.value &&
    startCheckInDate.value > endCheckInDate.value
  ) {
    ElMessage.error('Начальная дата не может быть позже конечной')
    return
  }
  void loadHistory(1)
}

const openEditDialog = () => {
  if (!room.value) return
  editForm.name = room.value.name
  editForm.capacity = Number(room.value.capacity)
  editForm.gender = genderCode(room.value.gender)
  isEditDialogVisible.value = true
}

const resetEditForm = () => {
  editForm.name = ''
  editForm.capacity = 1
  editForm.gender = 0
  editFormRef.value?.clearValidate()
}

const handleUpdate = async () => {
  if (!editFormRef.value || !room.value || editForm.capacity === undefined) return

  try {
    await editFormRef.value.validate()
  } catch {
    return
  }

  isUpdating.value = true
  const request: UpdateRoomRequest = {
    name: editForm.name.trim(),
    capacity: editForm.capacity,
    gender: editForm.gender,
  }

  try {
    await putApiDormitoriesDormitoryIdRoomsRoomId(dormitoryId(), room.value.id, request)
    ElMessage.success('Данные комнаты обновлены')
    isEditDialogVisible.value = false
    await loadRoomAndRelatedData()
  } catch (error) {
    const apiError = error as { response?: { data?: { title?: string } } }
    ElMessage.error(apiError.response?.data?.title || 'Не удалось обновить комнату')
  } finally {
    isUpdating.value = false
  }
}

const changeRoomStatus = async (activate: boolean) => {
  if (!room.value) return

  const action = activate ? 'активировать' : 'деактивировать'
  try {
    await ElMessageBox.confirm(`Вы уверены, что хотите ${action} комнату ${room.value.name}?`, {
      confirmButtonText: activate ? 'Активировать' : 'Деактивировать',
      cancelButtonText: 'Отмена',
      type: activate ? 'success' : 'warning',
    })
  } catch {
    return
  }

  isStatusUpdating.value = true
  try {
    if (activate) {
      await postApiDormitoriesDormitoryIdRoomsRoomIdActivate(dormitoryId(), room.value.id)
    } else {
      await postApiDormitoriesDormitoryIdRoomsRoomIdDeactivate(dormitoryId(), room.value.id)
    }
    ElMessage.success(activate ? 'Комната активирована' : 'Комната деактивирована')
    await loadRoomAndRelatedData()
  } catch (error) {
    const apiError = error as { response?: { data?: { title?: string } } }
    ElMessage.error(apiError.response?.data?.title || `Не удалось ${action} комнату`)
  } finally {
    isStatusUpdating.value = false
  }
}

watch(
  () => [dormitoryId(), roomId()],
  () => {
    historyPage.value = 1
    void loadRoomAndRelatedData()
  },
  { immediate: true },
)
</script>

<style scoped>
.room-details-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.page-toolbar {
  margin-bottom: calc(var(--spacing-sm) * -1);
}

.room-content {
  min-height: 140px;
}

.page-header,
.section-header,
.room-title-row,
.room-actions {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.page-header,
.section-header {
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--spacing-md);
}

.room-title-row {
  flex-wrap: wrap;
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

.room-summary {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  margin: 0;
  border-top: 1px solid var(--color-border);
  border-bottom: 1px solid var(--color-border);
}

.room-summary > div {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
  padding: var(--spacing-md);
}

.room-summary > div + div {
  border-left: 1px solid var(--color-border);
}

.room-summary dt {
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
}

.room-summary dd {
  margin: 0;
  color: var(--color-text-primary);
  font-size: var(--font-size-lg);
  font-weight: 600;
}

.data-section {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.section-title {
  font-size: var(--font-size-xl);
}

.occupants-content,
.history-content {
  min-height: 100px;
}

.occupant-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 280px), 1fr));
  gap: var(--spacing-md);
}

.occupant-item {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  min-width: 0;
  padding: var(--spacing-md);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: var(--color-bg-white);
  color: inherit;
  text-decoration: none;
  transition:
    border-color var(--transition-fast),
    background-color var(--transition-fast);
}

.occupant-item:hover {
  border-color: var(--color-primary);
  background: var(--color-bg-hover);
}

.occupant-item:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

.occupant-avatar {
  flex: 0 0 auto;
  background: var(--color-primary-bg);
  color: var(--color-primary);
}

.occupant-info {
  min-width: 0;
}

.occupant-info h3 {
  margin: 0;
  color: var(--color-text-primary);
  font-size: var(--font-size-base);
  font-weight: 600;
  overflow-wrap: anywhere;
}

.occupant-info p {
  margin: var(--spacing-xs) 0 0;
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
}

.history-filters {
  display: flex;
  align-items: flex-end;
  gap: var(--spacing-sm);
  padding: var(--spacing-md) 0;
  border-top: 1px solid var(--color-border-light);
  border-bottom: 1px solid var(--color-border-light);
}

.history-filters :deep(.el-form-item) {
  margin: 0;
}

.sort-select {
  width: 180px;
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-sm);
}

.full-width {
  width: 100%;
}

@media (max-width: 760px) {
  .page-header {
    flex-direction: column;
  }

  .room-actions {
    flex-wrap: wrap;
  }

  .room-summary {
    grid-template-columns: 1fr;
  }

  .room-summary > div + div {
    border-top: 1px solid var(--color-border);
    border-left: 0;
  }

  .history-filters {
    align-items: stretch;
    flex-direction: column;
  }

  .history-filters :deep(.el-form-item),
  .history-filters :deep(.el-date-editor),
  .sort-select {
    width: 100%;
  }
}
</style>
