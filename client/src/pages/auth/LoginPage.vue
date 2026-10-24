<template>
  <div class="login-card">
    <!-- Шапка карточки -->
    <div class="login-header">
      <h1 class="brand-title">Open Dorm</h1>
      <p class="login-subtitle">Авторизация</p>
    </div>

    <!-- Форма -->
    <el-form
      ref="formRef"
      :model="loginForm"
      :rules="rules"
      label-position="top"
      @submit.prevent="handleLogin"
      class="login-form"
    >
      <el-form-item label="Логин или Email" prop="login">
        <el-input
          v-model="loginForm.login"
          placeholder="Введите логин"
          size="large"
          :prefix-icon="User"
          clearable
        />
      </el-form-item>

      <el-form-item label="Пароль" prop="password">
        <el-input
          v-model="loginForm.password"
          type="password"
          placeholder="Введите пароль"
          size="large"
          :prefix-icon="Lock"
          show-password
          @keyup.enter="handleLogin"
        />
      </el-form-item>

      <el-form-item class="mt-md">
        <el-button
          type="primary"
          size="large"
          class="login-button"
          :loading="authStore.isLoading"
          native-type="submit"
        >
          {{ authStore.isLoading ? 'Вход...' : 'Войти в систему' }}
        </el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import axios from 'axios'
import { User, Lock } from '@element-plus/icons-vue'
import { useAuthStore } from '@/entities/auth/store/authStore'
import type { LoginRequest } from '@/shared/api/generated/models'

const router = useRouter()
const authStore = useAuthStore()

const formRef = ref<FormInstance>()

const loginForm = reactive<LoginRequest>({
  login: '',
  password: '',
})

const rules: FormRules = {
  login: [
    { required: true, message: 'Введите логин или email', trigger: 'blur' },
    { min: 3, message: 'Минимум 3 символа', trigger: 'blur' },
  ],
  password: [
    { required: true, message: 'Введите пароль', trigger: 'blur' },
    { min: 4, message: 'Минимум 4 символов', trigger: 'blur' },
  ],
}

const handleLogin = async () => {
  if (!formRef.value) return

  await formRef.value.validate(async (valid) => {
    if (valid) {
      try {
        await authStore.login(loginForm)

        ElMessage.success('Добро пожаловать!')

        if (authStore.needsRoleSelection) {
          router.push('/select-role')
        } else {
          router.push('/dashboard')
        }
      } catch (error: unknown) {
        const apiError = axios.isAxiosError<{ title?: string }>(error) ? error : null

        if (
          apiError &&
          (!apiError.response ||
            apiError.response.status === 408 ||
            apiError.response.status >= 500)
        ) {
          await router.push({ name: 'ServerUnavailable' })
          return
        }

        const errorMessage = apiError?.response?.data?.title || 'Неверный логин или пароль'
        ElMessage.error(errorMessage)
      }
    }
  })
}
</script>

<style scoped>
/* ===== Карточка ===== */
.login-card {
  width: 100%;
  max-width: 440px;
  min-height: 540px;
  background: var(--gradient-primary);
  border-radius: var(--border-radius-xl);
  padding: var(--spacing-2xl);
  box-shadow: var(--shadow-lg);
  display: flex;
  flex-direction: column;
}

/* ===== Шапка ===== */
.login-header {
  text-align: center;
  margin-bottom: var(--spacing-xl);
}

.brand-title {
  font-size: 36px;
  font-weight: 800;
  color: #ffffff;
  margin: 0 0 var(--spacing-xs) 0;
  letter-spacing: -0.5px;
}

.login-subtitle {
  font-size: var(--font-size-md);
  color: rgba(255, 255, 255, 0.85);
  margin: 0;
  font-weight: 500;
}

/* ===== Форма ===== */
.login-form {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

/* Лейблы полей  */
.login-form :deep(.el-form-item__label) {
  color: rgba(255, 255, 255, 0.9) !important;
  font-weight: 500;
  font-size: var(--font-size-sm);
  margin-bottom: 4px;
}

/* ===== Переопределение стилей Element Plus для полей ввода ===== */

/* Обёртка инпута: белый фон */
.login-form :deep(.el-input__wrapper) {
  background-color: var(--color-bg-white) !important;
  box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.3) inset !important;
  border-radius: var(--border-radius-md);
  padding: 4px 12px;
}

/* Обёртка при наведении */
.login-form :deep(.el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.6) inset !important;
}

/* Обёртка в фокусе */
.login-form :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 2px #ffffff inset !important;
}

/* Текст внутри инпута */
.login-form :deep(.el-input__inner) {
  color: var(--color-text-regular) !important; /* #374151 */
  font-size: var(--font-size-base);
}

/* Плейсхолдер */
.login-form :deep(.el-input__inner::placeholder) {
  color: var(--color-text-placeholder) !important; /* #9CA3AF */
}

/* Иконки внутри инпута */
.login-form :deep(.el-input__prefix .el-icon) {
  color: var(--color-text-secondary) !important;
}

/* ===== Кнопка ===== */
.login-button {
  width: 100%;
  height: 52px;
  font-size: var(--font-size-md);
  font-weight: 600;
  /* Белая кнопка на градиентном фоне */
  background-color: #ffffff !important;
  color: var(--color-primary) !important;
  border: none !important;
  border-radius: var(--border-radius-md);
  margin-top: var(--spacing-md);
  transition: all var(--transition-base);
}

.login-button:hover {
  background-color: rgba(255, 255, 255, 0.9) !important;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.login-button:active {
  transform: translateY(0);
}
</style>
