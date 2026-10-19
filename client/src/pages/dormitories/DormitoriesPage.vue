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

    <RetryErrorAlert
      v-if="dormitoryStore.error"
      :message="dormitoryStore.error"
      @retry="loadDormitories"
    />

    <div v-loading="dormitoryStore.isLoading" class="dormitories-content">
      <div v-if="dormitoryStore.dormitories.length" class="dormitory-grid">
        <DormitoryCard
          v-for="dormitory in dormitoryStore.dormitories"
          :key="dormitory.id"
          :dormitory="dormitory"
          :is-admin="isAdmin"
          @open="openDormitory"
        />
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
import { Plus } from '@element-plus/icons-vue'
import RetryErrorAlert from '@/components/RetryErrorAlert.vue'
import DormitoryCard from '@/components/dormitories/DormitoryCard.vue'
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
