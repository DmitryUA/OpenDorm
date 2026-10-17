<template>
  <section class="occupants-page">
    <header class="page-header">
      <div>
        <h1 class="page-title">Жильцы</h1>
        <p class="page-subtitle">Список жильцов и их текущее проживание</p>
      </div>
      <el-button v-if="canCreateOccupant" type="primary" :icon="Plus" @click="openCreateDialog">
        Добавить жильца
      </el-button>
    </header>

    <el-form class="filters" inline @submit.prevent="loadOccupants(1)">
      <el-form-item label="Статус">
        <el-select v-model="filters.status" clearable @change="loadOccupants(1)">
          <el-option label="Все" value="all" />
          <el-option label="Активные" value="active" />
          <el-option label="Неактивные" value="inactive" />
        </el-select>
      </el-form-item>

      <el-form-item label="Пол">
        <el-select v-model="filters.gender" clearable @change="loadOccupants(1)">
          <el-option label="Все" value="all" />
          <el-option label="Мужской" :value="0" />
          <el-option label="Женский" :value="1" />
        </el-select>
      </el-form-item>
    </el-form>

    <el-alert v-if="error" :title="error" type="error" :closable="false" show-icon>
      <template #default>
        <el-button link type="primary" @click="loadOccupants(page)">Повторить загрузку</el-button>
      </template>
    </el-alert>

    <div v-loading="loading" class="occupants-content">
      <div v-if="occupants.length" class="occupant-grid">
        <article v-for="occupant in occupants" :key="occupant.id" class="occupant-card">
          <button
            type="button"
            class="card-main"
            :aria-label="`Открыть жильца: ${fullName(occupant.lastName, occupant.firstName, occupant.patronymic)}`"
            @click="openOccupant(occupant.id)"
          >
            <div class="card-header">
              <el-avatar :size="42" class="occupant-avatar">
                {{ getInitials(occupant.firstName, occupant.lastName) }}
              </el-avatar>
              <span
                class="status-badge"
                :class="occupant.isActive ? 'status-active' : 'status-inactive'"
              >
                {{ occupant.isActive ? 'Активен' : 'Неактивен' }}
              </span>
            </div>

            <h2>{{ fullName(occupant.lastName, occupant.firstName, occupant.patronymic) }}</h2>

            <div class="meta-list">
              <span>{{ genderLabel(occupant.gender) }}</span>
              <span>{{ formatDate(occupant.birthDate) }}</span>
            </div>

            <p class="accommodation-text">
              <template v-if="occupant.currentAccommodation">
                {{ occupant.currentAccommodation.dormitoryAddress }} ·
                {{ occupant.currentAccommodation.roomName }}
              </template>
              <template v-else>На данный момент не заселен</template>
            </p>
          </button>
        </article>
      </div>

      <el-empty v-else-if="!loading && !error" description="Жильцов пока нет">
        <el-button v-if="canCreateOccupant" type="primary" @click="openCreateDialog">
          Добавить жильца
        </el-button>
      </el-empty>
    </div>

    <el-pagination
      v-if="totalCount > 0"
      background
      layout="prev, pager, next, total"
      :current-page="page"
      :page-size="pageSize"
      :total="totalCount"
      :disabled="loading"
      @current-change="loadOccupants"
    />

    <el-dialog
      v-model="isCreateDialogVisible"
      title="Новый жилец"
      width="min(560px, calc(100vw - 32px))"
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
        <el-form-item label="Фамилия" prop="lastName">
          <el-input v-model="form.lastName" maxlength="100" />
        </el-form-item>
        <el-form-item label="Имя" prop="firstName">
          <el-input v-model="form.firstName" maxlength="100" />
        </el-form-item>
        <el-form-item label="Отчество" prop="patronymic">
          <el-input v-model="form.patronymic" maxlength="100" placeholder="Необязательно" />
        </el-form-item>
        <el-form-item label="Пол" prop="gender">
          <el-select v-model="form.gender" class="full-width">
            <el-option label="Мужской" :value="0" />
            <el-option label="Женский" :value="1" />
          </el-select>
        </el-form-item>
        <el-form-item label="Дата рождения" prop="birthDate">
          <el-date-picker
            v-model="form.birthDate"
            type="date"
            value-format="YYYY-MM-DD"
            placeholder="Выберите дату"
            style="width: 100%"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-actions">
          <el-button @click="isCreateDialogVisible = false">Отмена</el-button>
          <el-button type="primary" :loading="isSubmitting" @click="handleCreate">
            Создать
          </el-button>
        </div>
      </template>
    </el-dialog>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { Plus } from '@element-plus/icons-vue'
import { getOccupants } from '@/shared/api/generated/occupants/occupants'
import type { GetApiOccupantsParams, OccupantListDto } from '@/shared/api/generated/models'
import { usePermissions } from '@/shared/composables/usePermissions'

interface OccupantForm {
  lastName: string
  firstName: string
  patronymic: string
  gender: number
  birthDate: string
}

const router = useRouter()
const { can } = usePermissions()
const canCreateOccupant = computed(() => can('occupants.create'))

const page = ref(1)
const pageSize = ref(20)
const loading = ref(false)
const error = ref<string | null>(null)
const totalCount = ref(0)
const occupants = ref<OccupantListDto[]>([])
const formRef = ref<FormInstance>()
const isCreateDialogVisible = ref(false)
const isSubmitting = ref(false)

const filters = reactive({
  status: 'all',
  gender: 'all',
})

const form = reactive<OccupantForm>({
  lastName: '',
  firstName: '',
  patronymic: '',
  gender: 0,
  birthDate: '',
})

const rules: FormRules<OccupantForm> = {
  lastName: [{ required: true, message: 'Укажите фамилию', trigger: 'blur' }],
  firstName: [{ required: true, message: 'Укажите имя', trigger: 'blur' }],
  gender: [{ required: true, message: 'Выберите пол', trigger: 'change' }],
  birthDate: [{ required: true, message: 'Укажите дату рождения', trigger: 'change' }],
}

const getInitials = (firstName: string, lastName: string) => {
  return `${firstName?.charAt(0)?.toUpperCase() || ''}${lastName?.charAt(0)?.toUpperCase() || ''}`
}

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

const loadOccupants = async (targetPage = page.value) => {
  loading.value = true
  error.value = null

  try {
    const { getApiOccupants } = getOccupants()
    const params: GetApiOccupantsParams = {
      page: targetPage,
      pageSize: pageSize.value,
    }

    if (filters.status !== 'all') {
      params.status = filters.status === 'active'
    }

    if (filters.gender !== 'all') {
      params.gender = Number(filters.gender)
    }

    const response = await getApiOccupants(params)
    occupants.value = response.items ?? []
    totalCount.value = Number(response.totalCount ?? 0)
    page.value = Number(response.page ?? targetPage)
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string } } }
    error.value = apiError.response?.data?.title || 'Не удалось загрузить список жильцов'
  } finally {
    loading.value = false
  }
}

const openCreateDialog = () => {
  isCreateDialogVisible.value = true
}

const openOccupant = (id: string) => {
  router.push({ name: 'OccupantDetails', params: { id } })
}

const resetForm = () => {
  form.lastName = ''
  form.firstName = ''
  form.patronymic = ''
  form.gender = 0
  form.birthDate = ''
  formRef.value?.clearValidate()
}

const handleCreate = async () => {
  if (!formRef.value) return

  try {
    await formRef.value.validate()
  } catch {
    return
  }

  isSubmitting.value = true

  try {
    const { postApiOccupants } = getOccupants()
    await postApiOccupants({
      lastName: form.lastName.trim(),
      firstName: form.firstName.trim(),
      patronymic: form.patronymic.trim() || null,
      gender: Number(form.gender),
      birthDate: form.birthDate,
    })
    ElMessage.success('Жилец добавлен')
    isCreateDialogVisible.value = false
    await loadOccupants(1)
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string } } }
    ElMessage.error(apiError.response?.data?.title || 'Не удалось добавить жильца')
  } finally {
    isSubmitting.value = false
  }
}

watch(
  () => [filters.status, filters.gender],
  () => {
    page.value = 1
    loadOccupants(1)
  },
)

onMounted(() => {
  loadOccupants(1)
})
</script>

<style scoped>
.occupants-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--spacing-md);
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

.filters {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: var(--color-bg-soft);
}

.filters :deep(.el-form-item) {
  flex: 0 0 auto;
  margin: 0;
}

.filters :deep(.el-select) {
  width: 160px;
}

.occupants-content {
  min-height: 180px;
}

.occupant-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 320px), 1fr));
  gap: var(--spacing-md);
}

.occupant-card {
  display: flex;
  min-width: 0;
}

.card-main {
  width: 100%;
  height: 100%;
  min-height: 204px;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  padding: var(--spacing-lg);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: var(--color-bg-card);
  text-align: left;
  cursor: pointer;
  transition:
    border-color 0.2s ease,
    box-shadow 0.2s ease,
    transform 0.2s ease;
}

.card-main:hover {
  border-color: var(--color-primary);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.06);
  transform: translateY(-1px);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--spacing-md);
}

.occupant-avatar {
  background: linear-gradient(135deg, var(--color-primary), #7bb7ff);
  color: white;
  font-weight: 600;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 600;
}

.status-active {
  color: #1b8e5a;
  background: rgba(39, 174, 96, 0.12);
}

.status-inactive {
  color: #5c6877;
  background: rgba(96, 109, 128, 0.12);
}

.card-main h2 {
  min-height: 2.7em;
  margin: 0;
  font-size: var(--font-size-lg);
  color: var(--color-text-primary);
  line-height: 1.35;
  overflow-wrap: anywhere;
}

.meta-list {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-sm);
  color: var(--color-text-secondary);
  font-size: 0.9rem;
}

.accommodation-text {
  margin: auto 0 0;
  color: var(--color-text-secondary);
  line-height: 1.5;
}

.full-width {
  width: 100%;
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-sm);
}

@media (max-width: 600px) {
  .filters {
    align-items: stretch;
    flex-direction: column;
  }

  .filters :deep(.el-form-item),
  .filters :deep(.el-select) {
    width: 100%;
  }
}
</style>
