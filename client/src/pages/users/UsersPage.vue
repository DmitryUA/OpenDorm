<template>
  <section class="users-page">
    <header class="page-header">
      <div>
        <h1 class="page-title">Пользователи</h1>
        <p class="page-subtitle">Учетные записи и назначенные роли</p>
      </div>
      <el-button v-if="can('users.create')" type="primary" :icon="Plus" @click="openCreateDialog">
        Добавить пользователя
      </el-button>
    </header>

    <el-form class="filters" inline>
      <el-form-item label="Поиск">
        <el-input
          v-model="filters.search"
          clearable
          placeholder="ФИО или логин"
          :prefix-icon="Search"
        />
      </el-form-item>
      <el-form-item label="Статус">
        <el-select v-model="filters.status" class="status-filter">
          <el-option label="Все" value="all" />
          <el-option label="Активные" value="active" />
          <el-option label="Неактивные" value="inactive" />
        </el-select>
      </el-form-item>
      <el-form-item label="Роль">
        <el-select v-model="filters.role" class="role-filter">
          <el-option label="Все роли" value="all" />
          <el-option label="Администратор" value="Administrator" />
          <el-option label="Менеджер" value="Manager" />
        </el-select>
      </el-form-item>
    </el-form>

    <el-alert v-if="error" :title="error" type="error" :closable="false" show-icon>
      <template #default>
        <el-button link type="primary" @click="loadUsers">Повторить загрузку</el-button>
      </template>
    </el-alert>

    <div v-loading="isLoading" class="users-content">
      <div v-if="filteredUsers.length" class="user-grid">
        <article v-for="user in filteredUsers" :key="user.id" class="user-card">
          <header class="user-card-header">
            <el-avatar :size="44" class="user-avatar">
              {{ initials(user.firstName, user.lastName) }}
            </el-avatar>
            <el-tag :type="user.isActive ? 'success' : 'info'" effect="light">
              {{ user.isActive ? 'Активен' : 'Неактивен' }}
            </el-tag>
          </header>

          <div class="user-identity">
            <h2>{{ fullName(user.lastName, user.firstName, user.patronymic) }}</h2>
            <p>@{{ user.login }}</p>
          </div>

          <div class="user-card-footer">
            <div class="role-list">
              <el-tag v-for="role in user.roles" :key="role" size="small" effect="plain">
                {{ roleLabel(role) }}
              </el-tag>
              <span v-if="!user.roles.length" class="no-role">Роль не назначена</span>
            </div>
            <span class="created-date">Создан {{ formatDate(user.createdAt) }}</span>
            <div class="user-actions">
              <el-button v-if="can('users.update')" text @click="openEditDialog(user)">
                Редактировать
              </el-button>
              <el-button
                v-if="user.isActive && can('users.deactivate')"
                text
                type="danger"
                :loading="statusUpdatingId === user.id"
                @click="changeUserStatus(user, false)"
              >
                Деактивировать
              </el-button>
              <el-button
                v-else-if="!user.isActive && can('users.activate')"
                text
                type="success"
                :loading="statusUpdatingId === user.id"
                @click="changeUserStatus(user, true)"
              >
                Активировать
              </el-button>
            </div>
          </div>
        </article>
      </div>

      <el-empty
        v-else-if="!isLoading && !error"
        :description="
          users.length ? 'Нет пользователей по выбранным фильтрам' : 'Пользователей пока нет'
        "
      >
        <el-button
          v-if="!users.length && can('users.create')"
          type="primary"
          @click="openCreateDialog"
        >
          Добавить пользователя
        </el-button>
      </el-empty>
    </div>

    <el-dialog
      v-model="isCreateDialogVisible"
      title="Новый пользователь"
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
        <el-form-item label="Логин" prop="login">
          <el-input
            v-model="form.login"
            maxlength="20"
            autocomplete="username"
            placeholder="латинские буквы, цифры, _ . -"
          />
        </el-form-item>
        <el-form-item label="Пароль" prop="password">
          <el-input
            v-model="form.password"
            type="password"
            maxlength="20"
            autocomplete="new-password"
            show-password
          />
        </el-form-item>
        <div class="name-fields">
          <el-form-item label="Фамилия" prop="lastName">
            <el-input v-model="form.lastName" maxlength="30" autocomplete="family-name" />
          </el-form-item>
          <el-form-item label="Имя" prop="firstName">
            <el-input v-model="form.firstName" maxlength="30" autocomplete="given-name" />
          </el-form-item>
        </div>
        <el-form-item label="Отчество" prop="patronymic">
          <el-input
            v-model="form.patronymic"
            maxlength="30"
            autocomplete="additional-name"
            placeholder="Необязательно"
          />
        </el-form-item>
        <el-form-item label="Роль" prop="roleName">
          <el-select v-model="form.roleName" class="full-width">
            <el-option label="Администратор" value="Administrator" />
            <el-option label="Менеджер" value="Manager" />
          </el-select>
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

    <el-dialog
      v-model="isEditDialogVisible"
      title="Редактировать пользователя"
      width="min(560px, calc(100vw - 32px))"
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
        <div class="name-fields">
          <el-form-item label="Фамилия" prop="lastName">
            <el-input v-model="editForm.lastName" maxlength="30" autocomplete="family-name" />
          </el-form-item>
          <el-form-item label="Имя" prop="firstName">
            <el-input v-model="editForm.firstName" maxlength="30" autocomplete="given-name" />
          </el-form-item>
        </div>
        <el-form-item label="Отчество" prop="patronymic">
          <el-input
            v-model="editForm.patronymic"
            maxlength="30"
            autocomplete="additional-name"
            placeholder="Необязательно"
          />
        </el-form-item>
        <el-form-item label="Новый пароль" prop="newPassword">
          <el-input
            v-model="editForm.newPassword"
            type="password"
            maxlength="100"
            autocomplete="new-password"
            placeholder="Оставьте пустым, чтобы не менять"
            show-password
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-actions">
          <el-button @click="isEditDialogVisible = false">Отмена</el-button>
          <el-button type="primary" :loading="isEditSubmitting" @click="handleUpdate">
            Сохранить
          </el-button>
        </div>
      </template>
    </el-dialog>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox, type FormInstance, type FormRules } from 'element-plus'
import { Plus, Search } from '@element-plus/icons-vue'
import { getUsers } from '@/shared/api/generated/users/users'
import type { CreateUserRequest, UpdateUserRequest, UserDto } from '@/shared/api/generated/models'
import { usePermissions } from '@/shared/composables/usePermissions'

interface UserForm extends CreateUserRequest {
  patronymic: string | null
}

interface UserEditForm {
  lastName: string
  firstName: string
  patronymic: string
  newPassword: string
}

const namePattern = /^[А-ЯA-ZЁ][а-яa-zё]+$/u
const loginPattern = /^[a-z0-9_.-]+$/

const { can } = usePermissions()
const {
  getApiUsers,
  postApiUsers,
  putApiUsersId,
  postApiUsersIdActivate,
  postApiUsersIdDeactivate,
} = getUsers()
const users = ref<UserDto[]>([])
const isLoading = ref(false)
const isSubmitting = ref(false)
const error = ref<string | null>(null)
const isCreateDialogVisible = ref(false)
const formRef = ref<FormInstance>()
const isEditDialogVisible = ref(false)
const editFormRef = ref<FormInstance>()
const isEditSubmitting = ref(false)
const statusUpdatingId = ref<string | null>(null)
const editingUser = ref<UserDto | null>(null)

const filters = reactive({
  search: '',
  status: 'all',
  role: 'all',
})

const form = reactive<UserForm>({
  login: '',
  password: '',
  lastName: '',
  firstName: '',
  patronymic: null,
  roleName: 'Manager',
})

const editForm = reactive<UserEditForm>({
  lastName: '',
  firstName: '',
  patronymic: '',
  newPassword: '',
})

const rules: FormRules<UserForm> = {
  login: [
    { required: true, message: 'Укажите логин', trigger: 'blur' },
    { min: 4, max: 20, message: 'Логин должен содержать от 4 до 20 символов', trigger: 'blur' },
    {
      pattern: loginPattern,
      message: 'Используйте латинские строчные буквы, цифры и _ . -',
      trigger: 'blur',
    },
  ],
  password: [
    { required: true, message: 'Укажите пароль', trigger: 'blur' },
    { min: 8, max: 20, message: 'Пароль должен содержать от 8 до 20 символов', trigger: 'blur' },
  ],
  lastName: [
    { required: true, message: 'Укажите фамилию', trigger: 'blur' },
    { min: 2, max: 30, message: 'Фамилия должна содержать от 2 до 30 букв', trigger: 'blur' },
    {
      pattern: namePattern,
      message: 'Первая буква заглавная, остальные строчные',
      trigger: 'blur',
    },
  ],
  firstName: [
    { required: true, message: 'Укажите имя', trigger: 'blur' },
    { min: 2, max: 30, message: 'Имя должно содержать от 2 до 30 букв', trigger: 'blur' },
    {
      pattern: namePattern,
      message: 'Первая буква заглавная, остальные строчные',
      trigger: 'blur',
    },
  ],
  patronymic: [
    {
      validator: (_rule, value: string | null, callback) => {
        if (!value || (value.length >= 2 && value.length <= 30 && namePattern.test(value))) {
          callback()
          return
        }
        callback(new Error('Отчество: от 2 до 30 букв, первая заглавная'))
      },
      trigger: 'blur',
    },
  ],
  roleName: [{ required: true, message: 'Выберите роль', trigger: 'change' }],
}

const editRules: FormRules<UserEditForm> = {
  lastName: [
    { required: true, message: 'Укажите фамилию', trigger: 'blur' },
    { min: 2, max: 30, message: 'Фамилия должна содержать от 2 до 30 букв', trigger: 'blur' },
    {
      pattern: namePattern,
      message: 'Первая буква заглавная, остальные строчные',
      trigger: 'blur',
    },
  ],
  firstName: [
    { required: true, message: 'Укажите имя', trigger: 'blur' },
    { min: 2, max: 30, message: 'Имя должно содержать от 2 до 30 букв', trigger: 'blur' },
    {
      pattern: namePattern,
      message: 'Первая буква заглавная, остальные строчные',
      trigger: 'blur',
    },
  ],
  patronymic: [
    {
      validator: (_rule, value: string, callback) => {
        if (!value || (value.length >= 2 && value.length <= 30 && namePattern.test(value))) {
          callback()
          return
        }
        callback(new Error('Отчество: от 2 до 30 букв, первая заглавная'))
      },
      trigger: 'blur',
    },
  ],
  newPassword: [
    {
      validator: (_rule, value: string, callback) => {
        if (!value || (value.length >= 6 && value.length <= 100)) {
          callback()
          return
        }
        callback(new Error('Новый пароль должен содержать от 6 до 100 символов'))
      },
      trigger: 'blur',
    },
  ],
}

const filteredUsers = computed(() => {
  const query = filters.search.trim().toLocaleLowerCase('ru-RU')
  return users.value.filter((user) => {
    const matchesSearch =
      !query ||
      user.login.toLocaleLowerCase('ru-RU').includes(query) ||
      fullName(user.lastName, user.firstName, user.patronymic)
        .toLocaleLowerCase('ru-RU')
        .includes(query)
    const matchesStatus =
      filters.status === 'all' || user.isActive === (filters.status === 'active')
    const matchesRole =
      filters.role === 'all' ||
      user.roles.some((role) => role.toLocaleUpperCase('en-US') === filters.role.toUpperCase())
    return matchesSearch && matchesStatus && matchesRole
  })
})

const fullName = (lastName: string, firstName: string, patronymic: string | null) =>
  [lastName, firstName, patronymic].filter(Boolean).join(' ')

const initials = (firstName: string, lastName: string) =>
  `${firstName.charAt(0)}${lastName.charAt(0)}`.toLocaleUpperCase('ru-RU')

const roleLabel = (role: string) => {
  const labels: Record<string, string> = {
    ADMINISTRATOR: 'Администратор',
    MANAGER: 'Менеджер',
  }
  return labels[role.toLocaleUpperCase('en-US')] || role
}

const formatDate = (value: string) => {
  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? value
    : new Intl.DateTimeFormat('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
      }).format(date)
}

const loadUsers = async () => {
  isLoading.value = true
  error.value = null
  try {
    users.value = await getApiUsers()
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string } } }
    error.value = apiError.response?.data?.title || 'Не удалось загрузить список пользователей'
  } finally {
    isLoading.value = false
  }
}

const openCreateDialog = () => {
  isCreateDialogVisible.value = true
}

const openEditDialog = (user: UserDto) => {
  editingUser.value = user
  editForm.lastName = user.lastName
  editForm.firstName = user.firstName
  editForm.patronymic = user.patronymic || ''
  editForm.newPassword = ''
  isEditDialogVisible.value = true
}

const resetForm = () => {
  form.login = ''
  form.password = ''
  form.lastName = ''
  form.firstName = ''
  form.patronymic = null
  form.roleName = 'Manager'
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
    await postApiUsers({
      ...form,
      login: form.login.trim().toLowerCase(),
      lastName: form.lastName.trim(),
      firstName: form.firstName.trim(),
      patronymic: form.patronymic?.trim() || null,
    })
    ElMessage.success('Пользователь добавлен')
    isCreateDialogVisible.value = false
    await loadUsers()
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string } } }
    ElMessage.error(apiError.response?.data?.title || 'Не удалось добавить пользователя')
  } finally {
    isSubmitting.value = false
  }
}

const resetEditForm = () => {
  editingUser.value = null
  editForm.lastName = ''
  editForm.firstName = ''
  editForm.patronymic = ''
  editForm.newPassword = ''
  editFormRef.value?.clearValidate()
}

const handleUpdate = async () => {
  if (!editFormRef.value || !editingUser.value) return
  try {
    await editFormRef.value.validate()
  } catch {
    return
  }

  isEditSubmitting.value = true
  const request: UpdateUserRequest = {
    id: editingUser.value.id,
    lastName: editForm.lastName.trim(),
    firstName: editForm.firstName.trim(),
    patronymic: editForm.patronymic.trim() || null,
    newPassword: editForm.newPassword || null,
  }

  try {
    await putApiUsersId(editingUser.value.id, request)
    ElMessage.success('Данные пользователя обновлены')
    isEditDialogVisible.value = false
    await loadUsers()
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string } } }
    ElMessage.error(apiError.response?.data?.title || 'Не удалось обновить пользователя')
  } finally {
    isEditSubmitting.value = false
  }
}

const changeUserStatus = async (user: UserDto, activate: boolean) => {
  const action = activate ? 'активировать' : 'деактивировать'
  try {
    await ElMessageBox.confirm(
      `Вы уверены, что хотите ${action} пользователя ${user.login}?`,
      activate ? 'Активация пользователя' : 'Деактивация пользователя',
      {
        confirmButtonText: activate ? 'Активировать' : 'Деактивировать',
        cancelButtonText: 'Отмена',
        type: activate ? 'success' : 'warning',
      },
    )
  } catch {
    return
  }

  statusUpdatingId.value = user.id
  try {
    if (activate) {
      await postApiUsersIdActivate(user.id)
    } else {
      await postApiUsersIdDeactivate(user.id)
    }
    ElMessage.success(activate ? 'Пользователь активирован' : 'Пользователь деактивирован')
    await loadUsers()
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string } } }
    ElMessage.error(apiError.response?.data?.title || `Не удалось ${action} пользователя`)
  } finally {
    statusUpdatingId.value = null
  }
}

onMounted(loadUsers)
</script>

<style scoped>
.users-page {
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

.filters {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: var(--color-bg-white);
}

.filters :deep(.el-form-item) {
  margin: 0;
}

.filters :deep(.el-input) {
  width: 250px;
}

.status-filter,
.role-filter {
  width: 170px;
}

.users-content {
  min-height: 180px;
}

.user-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(100%, 320px), 1fr));
  gap: var(--spacing-md);
}

.user-card {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  min-width: 0;
  padding: var(--spacing-lg);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: var(--color-bg-white);
}

.user-card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-sm);
}

.user-avatar {
  background: var(--color-primary-bg);
  color: var(--color-primary);
  font-weight: 600;
}

.user-identity h2 {
  min-height: 2.7em;
  margin: 0;
  color: var(--color-text-primary);
  font-size: var(--font-size-lg);
  font-weight: 600;
  line-height: 1.35;
  overflow-wrap: anywhere;
}

.user-identity p {
  margin: var(--spacing-xs) 0 0;
  color: var(--color-text-secondary);
}

.user-card-footer {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
  margin-top: auto;
  padding-top: var(--spacing-md);
  border-top: 1px solid var(--color-border-light);
}

.user-actions {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-xs);
  margin: 0 calc(var(--spacing-sm) * -1);
}

.role-list {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-xs);
}

.no-role,
.created-date {
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
}

.name-fields {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: var(--spacing-md);
}

.name-fields :deep(.el-form-item) {
  min-width: 0;
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
  .page-header {
    flex-direction: column;
  }

  .page-header .el-button,
  .filters :deep(.el-form-item),
  .filters :deep(.el-input),
  .status-filter,
  .role-filter {
    width: 100%;
  }

  .name-fields {
    grid-template-columns: 1fr;
    gap: 0;
  }
}
</style>
