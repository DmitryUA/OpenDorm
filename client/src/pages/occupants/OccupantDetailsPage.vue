<template>
  <section class="occupant-details-page">
    <div class="page-toolbar">
      <el-button text :icon="ArrowLeft" @click="router.push({ name: 'Occupants' })">
        К списку жильцов
      </el-button>
    </div>

    <el-alert v-if="error" :title="error" type="error" :closable="false" show-icon>
      <template #default>
        <el-button link type="primary" @click="loadOccupantData">Повторить загрузку</el-button>
      </template>
    </el-alert>

    <div v-loading="isLoading" class="occupant-content">
      <template v-if="occupant">
        <header class="page-header">
          <div>
            <div class="title-row">
              <h1 class="page-title">
                {{ fullName(occupant.lastName, occupant.firstName, occupant.patronymic) }}
              </h1>
              <el-tag :type="occupant.isActive ? 'success' : 'info'" effect="light">
                {{ occupant.isActive ? 'Активен' : 'Неактивен' }}
              </el-tag>
            </div>
            <p class="page-subtitle">
              {{ genderLabel(occupant.gender) }} · {{ formatDate(occupant.birthDate) }}
            </p>
          </div>

          <div class="action-group">
            <el-button
              v-if="can('accommodations.checkin')"
              type="primary"
              @click="openCheckInDialog"
            >
              Заселение
            </el-button>
            <el-button
              v-if="occupant.isActive && can('accommodations.checkout')"
              type="warning"
              plain
              :loading="isStatusUpdating"
              @click="handleCheckOut"
            >
              Выселение
            </el-button>
            <el-button
              v-if="can('accommodations.transfer')"
              type="info"
              plain
              @click="openTransferDialog"
            >
              Переселение
            </el-button>
            <el-button
              v-if="occupant.isActive && can('occupants.deactivate')"
              type="danger"
              plain
              :loading="isStatusUpdating"
              @click="toggleStatus(false)"
            >
              Деактивировать
            </el-button>
            <el-button
              v-else-if="!occupant.isActive && can('occupants.activate')"
              type="success"
              plain
              :loading="isStatusUpdating"
              @click="toggleStatus(true)"
            >
              Активировать
            </el-button>
          </div>
        </header>

        <dl class="common-summary">
          <div>
            <dt>Пол</dt>
            <dd>{{ genderLabel(occupant.gender) }}</dd>
          </div>
          <div>
            <dt>Дата рождения</dt>
            <dd>{{ formatDate(occupant.birthDate) }}</dd>
          </div>
          <div>
            <dt>Текущее проживание</dt>
            <dd>
              <template v-if="occupant.currentAccommodation">
                {{ occupant.currentAccommodation.dormitoryAddress }} ·
                {{ occupant.currentAccommodation.roomName }}
              </template>
              <template v-else>Не заселен</template>
            </dd>
          </div>
        </dl>
      </template>
    </div>

    <section v-if="occupant && can('accommodations.read')" class="data-section">
      <header class="section-header">
        <div>
          <h2 class="section-title">История переселений</h2>
          <p class="section-subtitle">{{ history.length }} записей</p>
        </div>
      </header>

      <el-alert v-if="historyError" :title="historyError" type="error" :closable="false" show-icon>
        <template #default>
          <el-button link type="primary" @click="loadHistory">Повторить загрузку</el-button>
        </template>
      </el-alert>

      <div v-loading="isHistoryLoading" class="history-content">
        <el-table v-if="history.length" :data="history" stripe>
          <el-table-column label="Период" min-width="180">
            <template #default="{ row }">
              {{ formatDate(row.checkInDate) }}
              <template v-if="row.checkOutDate"> — {{ formatDate(row.checkOutDate) }}</template>
              <template v-else> — Сейчас</template>
            </template>
          </el-table-column>
          <el-table-column label="Адрес" min-width="220">
            <template #default="{ row }">{{ row.address.dormitoryAddress }}</template>
          </el-table-column>
          <el-table-column label="Комната" min-width="120">
            <template #default="{ row }">{{ row.address.roomName }}</template>
          </el-table-column>
        </el-table>
        <el-empty
          v-else-if="!isHistoryLoading && !historyError"
          description="История заселений пуста"
        />
      </div>
    </section>

    <el-dialog
      v-model="isCheckInDialogVisible"
      title="Заселение жильца"
      width="min(520px, calc(100vw - 32px))"
      destroy-on-close
    >
      <el-form @submit.prevent="handleCheckIn">
        <el-form-item label="Комната для заселения">
          <el-select
            v-model="selectedRoomId"
            filterable
            class="full-width"
            placeholder="Выберите комнату"
          >
            <el-option
              v-for="room in availableRooms"
              :key="room.id"
              :label="`${room.label}`"
              :value="room.id"
            />
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-actions">
          <el-button @click="isCheckInDialogVisible = false">Отмена</el-button>
          <el-button type="primary" :loading="isActionLoading" @click="handleCheckIn">
            Заселить
          </el-button>
        </div>
      </template>
    </el-dialog>

    <el-dialog
      v-model="isTransferDialogVisible"
      title="Переселение жильца"
      width="min(520px, calc(100vw - 32px))"
      destroy-on-close
    >
      <el-form @submit.prevent="handleTransfer">
        <el-form-item label="Новая комната">
          <el-select
            v-model="targetRoomId"
            filterable
            class="full-width"
            placeholder="Выберите комнату"
          >
            <el-option
              v-for="room in availableRooms"
              :key="room.id"
              :label="room.label"
              :value="room.id"
            />
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-actions">
          <el-button @click="isTransferDialogVisible = false">Отмена</el-button>
          <el-button type="primary" :loading="isActionLoading" @click="handleTransfer">
            Переселить
          </el-button>
        </div>
      </template>
    </el-dialog>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { ArrowLeft } from '@element-plus/icons-vue'
import { getDormitories } from '@/shared/api/generated/dormitories'
import { getOccupants } from '@/shared/api/generated/occupants/occupants'
import type { AccommodationDto, OccupantListDto, RoomListDto } from '@/shared/api/generated/models'
import { usePermissions } from '@/shared/composables/usePermissions'

interface RoomSelectOption extends RoomListDto {
  label: string
  dormitoryAddress: string
}

const route = useRoute()
const router = useRouter()
const { can } = usePermissions()

const occupantId = computed(() => {
  const value = route.params.id
  return Array.isArray(value) ? value[0] : value
})

const occupant = ref<OccupantListDto | null>(null)
const history = ref<AccommodationDto[]>([])
const error = ref<string | null>(null)
const historyError = ref<string | null>(null)
const availableRooms = ref<RoomSelectOption[]>([])
const selectedRoomId = ref('')
const targetRoomId = ref('')
const isLoading = ref(false)
const isHistoryLoading = ref(false)
const isActionLoading = ref(false)
const isStatusUpdating = ref(false)
const isCheckInDialogVisible = ref(false)
const isTransferDialogVisible = ref(false)

const fullName = (lastName: string, firstName: string, patronymic?: string | null) => {
  const parts = [lastName, firstName, patronymic].filter(Boolean)
  return parts.join(' ')
}

const genderLabel = (gender: number) => {
  return gender === 1 ? 'Женский' : 'Мужской'
}

const formatDate = (value?: string | null) => {
  if (!value) return '—'

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(date)
}

const loadOccupantData = async () => {
  const id = occupantId.value
  if (!id) {
    error.value = 'Не удалось определить идентификатор жильца'
    return
  }

  isLoading.value = true
  error.value = null

  try {
    const { getApiOccupants } = getOccupants()
    const response = await getApiOccupants({ page: 1, pageSize: 1000 })
    const found = response.items?.find((item) => item.id === id) ?? null

    if (!found) {
      throw new Error('Жилец не найден')
    }

    occupant.value = found
    await Promise.all([loadHistory(), loadAvailableRooms()])
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string } }; message?: string }
    error.value =
      apiError.response?.data?.title || apiError.message || 'Не удалось загрузить данные жильца'
  } finally {
    isLoading.value = false
  }
}

const loadHistory = async () => {
  const id = occupantId.value
  if (!id) return

  isHistoryLoading.value = true
  historyError.value = null

  try {
    const { getApiOccupantsIdAccommodations } = getOccupants()
    const result = await getApiOccupantsIdAccommodations(id)
    history.value = Array.isArray(result) ? result : []
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string } } }
    historyError.value = apiError.response?.data?.title || 'Не удалось загрузить историю заселений'
  } finally {
    isHistoryLoading.value = false
  }
}

const loadAvailableRooms = async () => {
  try {
    const { getApiDormitories, getApiDormitoriesIdRooms } = getDormitories()
    const dormitories = await getApiDormitories()

    const roomGroups = await Promise.all(
      dormitories.map(async (dormitory) => {
        const result = await getApiDormitoriesIdRooms(dormitory.id, { pageSize: 500 })
        return (result.items ?? []).map((room) => ({
          ...room,
          label: `${dormitory.city}, ${dormitory.street}, ${dormitory.house} · Комната ${room.name}`,
          dormitoryAddress: `${dormitory.city}, ${dormitory.street}, ${dormitory.house}`,
        }))
      }),
    )

    availableRooms.value = roomGroups.flat().filter((room) => room.isActive)
  } catch {
    availableRooms.value = []
  }
}

const openCheckInDialog = () => {
  selectedRoomId.value = ''
  isCheckInDialogVisible.value = true
}

const openTransferDialog = () => {
  targetRoomId.value = ''
  isTransferDialogVisible.value = true
}

const handleCheckIn = async () => {
  const id = occupantId.value
  if (!id || !selectedRoomId.value) {
    ElMessage.warning('Выберите комнату для заселения')
    return
  }

  isActionLoading.value = true

  try {
    const { postApiOccupantsIdCheckIn } = getOccupants()
    await postApiOccupantsIdCheckIn(id, { roomId: selectedRoomId.value })
    ElMessage.success('Жилец успешно заселен')
    isCheckInDialogVisible.value = false
    await loadOccupantData()
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string } } }
    ElMessage.error(apiError.response?.data?.title || 'Не удалось заселить жильца')
  } finally {
    isActionLoading.value = false
  }
}

const handleCheckOut = async () => {
  const id = occupantId.value
  if (!id) return

  try {
    await ElMessageBox.confirm('Выселить жильца из текущей комнаты?', 'Подтверждение', {
      confirmButtonText: 'Выселить',
      cancelButtonText: 'Отмена',
      type: 'warning',
    })

    isStatusUpdating.value = true
    const { postApiOccupantsIdCheckOut } = getOccupants()
    await postApiOccupantsIdCheckOut(id)
    ElMessage.success('Жилец выселен')
    await loadOccupantData()
  } catch {
    // Пользователь отменил действие.
  } finally {
    isStatusUpdating.value = false
  }
}

const handleTransfer = async () => {
  const id = occupantId.value
  if (!id || !targetRoomId.value) {
    ElMessage.warning('Выберите новую комнату для переселения')
    return
  }

  isActionLoading.value = true

  try {
    const { postApiOccupantsIdTransfer } = getOccupants()
    await postApiOccupantsIdTransfer(id, { targetRoomId: targetRoomId.value })
    ElMessage.success('Жилец переселен')
    isTransferDialogVisible.value = false
    await loadOccupantData()
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string } } }
    ElMessage.error(apiError.response?.data?.title || 'Не удалось переселить жильца')
  } finally {
    isActionLoading.value = false
  }
}

const toggleStatus = async (nextActive: boolean) => {
  const id = occupantId.value
  if (!id) return

  isStatusUpdating.value = true

  try {
    const { postApiOccupantsIdActivate, postApiOccupantsIdDeactivate } = getOccupants()
    if (nextActive) {
      await postApiOccupantsIdActivate(id)
      ElMessage.success('Жилец активирован')
    } else {
      await postApiOccupantsIdDeactivate(id)
      ElMessage.success('Жилец деактивирован')
    }
    await loadOccupantData()
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string } } }
    ElMessage.error(apiError.response?.data?.title || 'Не удалось изменить статус жильца')
  } finally {
    isStatusUpdating.value = false
  }
}

onMounted(() => {
  loadOccupantData()
})
</script>

<style scoped>
.occupant-details-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.page-toolbar {
  display: flex;
  justify-content: flex-start;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--spacing-md);
}

.title-row {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  flex-wrap: wrap;
}

.page-title {
  margin: 0;
  color: var(--color-text-primary);
  font-size: var(--font-size-2xl);
  font-weight: 600;
}

.page-subtitle {
  margin: var(--spacing-xs) 0 0;
  color: var(--color-text-secondary);
}

.action-group {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: var(--spacing-sm);
}

.common-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: var(--spacing-md);
  margin: 0;
  padding: var(--spacing-lg);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: var(--color-bg-card);
}

.common-summary div {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.common-summary dt {
  color: var(--color-text-secondary);
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.common-summary dd {
  margin: 0;
  color: var(--color-text-primary);
  font-weight: 500;
}

.data-section {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding: var(--spacing-lg);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: var(--color-bg-card);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--spacing-md);
}

.section-title {
  margin: 0;
  color: var(--color-text-primary);
  font-size: var(--font-size-xl);
}

.section-subtitle {
  margin: var(--spacing-xs) 0 0;
  color: var(--color-text-secondary);
}

.history-content {
  min-height: 120px;
}

.full-width {
  width: 100%;
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-sm);
}
</style>
