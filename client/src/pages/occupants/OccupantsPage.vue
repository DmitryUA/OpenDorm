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

    <el-form class="filters" inline @submit.prevent="applyFilters">
      <el-form-item label="Фамилия">
        <el-input
          v-model="filters.lastName"
          clearable
          placeholder="Точное совпадение"
          @clear="applyFilters"
        />
      </el-form-item>

      <el-form-item label="Имя">
        <el-input
          v-model="filters.firstName"
          clearable
          placeholder="Точное совпадение"
          @clear="applyFilters"
        />
      </el-form-item>

      <el-form-item label="Отчество">
        <el-input
          v-model="filters.patronymic"
          clearable
          placeholder="Точное совпадение"
          @clear="applyFilters"
        />
      </el-form-item>

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

      <el-button type="primary" :loading="loading" @click="applyFilters">Применить</el-button>
    </el-form>

    <RetryErrorAlert v-if="error" :message="error" @retry="loadOccupants(page)" />

    <div v-loading="loading" class="occupants-content">
      <div v-if="occupants.length" class="occupant-grid">
        <OccupantCard
          v-for="occupant in occupants"
          :key="occupant.id"
          :occupant="occupant"
          @open="openOccupant"
        />
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
        <el-form-item label="Телефон" prop="phone">
          <el-input
            v-model="form.phone"
            type="tel"
            autocomplete="tel"
            placeholder="+7 900 000-00-00"
          />
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
import OccupantCard from '@/components/occupants/OccupantCard.vue'
import RetryErrorAlert from '@/components/RetryErrorAlert.vue'
import { getOccupants } from '@/shared/api/generated/occupants/occupants'
import type { GetApiOccupantsParams, OccupantListDto } from '@/shared/api/generated/models'
import { usePermissions } from '@/shared/composables/usePermissions'

interface OccupantForm {
  lastName: string
  firstName: string
  patronymic: string
  gender: number
  birthDate: string
  phone: string
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
  lastName: '',
  firstName: '',
  patronymic: '',
  status: 'all',
  gender: 'all',
})

const form = reactive<OccupantForm>({
  lastName: '',
  firstName: '',
  patronymic: '',
  gender: 0,
  birthDate: '',
  phone: '',
})

const rules: FormRules<OccupantForm> = {
  lastName: [{ required: true, message: 'Укажите фамилию', trigger: 'blur' }],
  firstName: [{ required: true, message: 'Укажите имя', trigger: 'blur' }],
  gender: [{ required: true, message: 'Выберите пол', trigger: 'change' }],
  birthDate: [{ required: true, message: 'Укажите дату рождения', trigger: 'change' }],
  phone: [{ required: true, message: 'Укажите телефон', trigger: 'blur' }],
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

    const lastName = filters.lastName.trim()
    const firstName = filters.firstName.trim()
    const patronymic = filters.patronymic.trim()
    if (lastName) params.lastName = lastName
    if (firstName) params.firstName = firstName
    if (patronymic) params.patronymic = patronymic

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

const applyFilters = () => {
  page.value = 1
  void loadOccupants(1)
}

const resetForm = () => {
  form.lastName = ''
  form.firstName = ''
  form.patronymic = ''
  form.gender = 0
  form.birthDate = ''
  form.phone = ''
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
      phone: form.phone.trim(),
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

.filters :deep(.el-input) {
  width: 180px;
}

.occupants-content {
  min-height: 180px;
}

.occupant-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 320px), 1fr));
  gap: var(--spacing-md);
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
  .filters :deep(.el-select),
  .filters :deep(.el-input) {
    width: 100%;
  }
}
</style>
