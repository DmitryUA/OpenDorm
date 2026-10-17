<template>
  <section class="dormitories-page">
    <header class="page-header">
      <div>
        <h1 class="page-title">Общежития</h1>
        <p class="page-subtitle">Список общежитий и их основные данные</p>
      </div>
      <el-button v-if="canCreateDormitory" type="primary" :icon="Plus" @click="openCreateDialog">
        Добавить общежитие
      </el-button>
    </header>

    <el-alert
      v-if="dormitoryStore.error"
      :title="dormitoryStore.error"
      type="error"
      :closable="false"
      show-icon
    >
      <template #default>
        <el-button link type="primary" @click="loadDormitories">Повторить загрузку</el-button>
      </template>
    </el-alert>

    <div v-loading="dormitoryStore.isLoading" class="dormitories-content">
      <div v-if="dormitoryStore.dormitories.length" class="dormitory-grid">
        <article
          v-for="dormitory in dormitoryStore.dormitories"
          :key="dormitory.id"
          class="dormitory-card"
        >
          <button
            type="button"
            class="card-main"
            :aria-label="`Открыть общежитие: ${dormitory.city}, ${dormitory.street}, ${dormitory.house}`"
            @click="openDormitory(dormitory.id)"
          >
            <div class="card-address">
              <el-icon class="address-icon"><OfficeBuilding /></el-icon>
              <h2>{{ dormitory.city }}, {{ dormitory.street }}, {{ dormitory.house }}</h2>
            </div>

            <div class="card-floors">
              <span class="detail-label">Этажей</span>
              <span class="detail-value">{{ dormitory.floorCount }}</span>
            </div>
          </button>

          <div v-if="isAdmin" class="card-id">
            <span class="detail-label">ID общежития</span>
            <el-button
              class="id-copy-button"
              text
              :title="`Скопировать ID ${dormitory.id}`"
              :aria-label="`Скопировать ID общежития ${dormitory.id}`"
              @click="copyId(dormitory.id)"
            >
              <span>{{ dormitory.id }}</span>
              <el-icon><CopyDocument /></el-icon>
            </el-button>
          </div>
        </article>
      </div>

      <el-empty
        v-else-if="!dormitoryStore.isLoading && !dormitoryStore.error"
        description="Общежитий пока нет"
      >
        <el-button v-if="canCreateDormitory" type="primary" @click="openCreateDialog">
          Добавить общежитие
        </el-button>
      </el-empty>
    </div>

    <el-dialog
      v-model="isCreateDialogVisible"
      title="Новое общежитие"
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
        <el-form-item label="Город" prop="city">
          <el-input v-model="form.city" maxlength="50" autocomplete="address-level2" />
        </el-form-item>
        <el-form-item label="Улица" prop="street">
          <el-input v-model="form.street" maxlength="255" autocomplete="address-line1" />
        </el-form-item>
        <el-form-item label="Дом" prop="house">
          <el-input v-model="form.house" maxlength="20" autocomplete="address-line2" />
        </el-form-item>
        <el-form-item label="Количество этажей" prop="floorCount">
          <el-input-number v-model="form.floorCount" :min="1" :max="100" :precision="0" />
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
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { CopyDocument, OfficeBuilding, Plus } from '@element-plus/icons-vue'
import { useDormitoryStore } from '@/entities/dormitory/store/dormitoryStore'
import { usePermissions } from '@/shared/composables/usePermissions'

interface DormitoryForm {
  city: string
  street: string
  house: string
  floorCount: number | undefined
}

const dormitoryStore = useDormitoryStore()
const router = useRouter()
const { canCreateDormitory, isAdmin } = usePermissions()
const formRef = ref<FormInstance>()
const isCreateDialogVisible = ref(false)
const isSubmitting = ref(false)

const form = reactive<DormitoryForm>({
  city: '',
  street: '',
  house: '',
  floorCount: 1,
})

const rules: FormRules<DormitoryForm> = {
  city: [
    { required: true, message: 'Укажите город', trigger: 'blur' },
    { min: 3, message: 'Название города должно содержать не менее 3 символов', trigger: 'blur' },
  ],
  street: [
    { required: true, message: 'Укажите улицу', trigger: 'blur' },
    { min: 3, message: 'Название улицы должно содержать не менее 3 символов', trigger: 'blur' },
  ],
  house: [{ required: true, message: 'Укажите номер дома', trigger: 'blur' }],
  floorCount: [{ required: true, message: 'Укажите количество этажей', trigger: 'change' }],
}

const loadDormitories = async () => {
  try {
    await dormitoryStore.fetchDormitories()
  } catch {
    // Ошибка загрузки отображается из состояния хранилища.
  }
}

const openCreateDialog = () => {
  isCreateDialogVisible.value = true
}

const openDormitory = (id: string) => {
  router.push({ name: 'DormitoryDetails', params: { id } })
}

const resetForm = () => {
  form.city = ''
  form.street = ''
  form.house = ''
  form.floorCount = 1
  formRef.value?.clearValidate()
}

const handleCreate = async () => {
  if (!formRef.value || form.floorCount === undefined) return

  try {
    await formRef.value.validate()
  } catch {
    return
  }

  isSubmitting.value = true
  try {
    await dormitoryStore.createDormitory({
      city: form.city.trim(),
      street: form.street.trim(),
      house: form.house.trim(),
      floorCount: form.floorCount,
    })
    ElMessage.success('Общежитие добавлено')
    isCreateDialogVisible.value = false
  } catch (error) {
    const apiError = error as { response?: { data?: { title?: string } } }
    ElMessage.error(apiError.response?.data?.title || 'Не удалось добавить общежитие')
  } finally {
    isSubmitting.value = false
  }
}

const copyId = async (id: string) => {
  try {
    await navigator.clipboard.writeText(id)
    ElMessage.success('ID скопирован')
  } catch {
    ElMessage.error('Не удалось скопировать ID')
  }
}

onMounted(loadDormitories)
</script>

<style scoped>
.dormitories-page {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
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

.dormitories-content {
  min-height: 160px;
}

.dormitory-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 300px), 1fr));
  gap: var(--spacing-md);
}

.dormitory-card {
  min-width: 0;
  padding: var(--spacing-lg);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: var(--color-bg-white);
  box-shadow: var(--shadow-sm);
}

.card-main {
  display: flex;
  flex-direction: column;
  width: 100%;
  padding: 0;
  border: 0;
  background: transparent;
  color: inherit;
  cursor: pointer;
  text-align: left;
}

.card-main:focus-visible {
  border-radius: var(--border-radius-sm);
  outline: 2px solid var(--color-primary);
  outline-offset: 4px;
}

.card-address {
  display: flex;
  align-items: flex-start;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-lg);
}

.address-icon {
  flex: 0 0 auto;
  margin-top: 3px;
  color: var(--color-primary);
  font-size: var(--font-size-lg);
}

.card-address h2 {
  min-width: 0;
  margin: 0;
  color: var(--color-text-primary);
  font-size: var(--font-size-lg);
  font-weight: 600;
  line-height: 1.4;
  overflow-wrap: anywhere;
}

.card-floors,
.card-id {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-sm);
}

.card-id {
  flex-direction: column;
  align-items: stretch;
  margin-top: var(--spacing-md);
  padding-top: var(--spacing-md);
  border-top: 1px solid var(--color-border-light);
}

.detail-label {
  flex: 0 0 auto;
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
}

.detail-value {
  color: var(--color-text-primary);
  font-weight: 600;
}

.id-copy-button {
  display: flex;
  justify-content: flex-start;
  gap: var(--spacing-sm);
  min-width: 0;
  max-width: 100%;
  height: auto;
  white-space: normal;
  padding: 2px 0 2px 8px;
  color: var(--color-primary);
}

.id-copy-button span {
  flex: 1;
  min-width: 0;
  font-family: monospace;
  overflow-wrap: anywhere;
  text-align: left;
  white-space: normal;
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-sm);
}

@media (max-width: 600px) {
  .page-header {
    flex-direction: column;
  }

  .page-header .el-button {
    width: 100%;
  }
}
</style>
