<template>
  <el-card class="role-card" shadow="lg">
    <h2 class="role-title">Выберите роль</h2>
    <p class="role-subtitle">У вас несколько ролей в системе. Выберите, как войти:</p>

    <div class="role-buttons">
      <el-button
        v-for="role in availableRoles"
        :key="role"
        type="primary"
        size="large"
        class="role-button"
        :class="{ 'role-button--administrator': role === 'Administrator' }"
        @click="handleSelectRole(role)"
      >
        <el-icon :size="24" class="role-icon">
          <component :is="getRoleIcon(role)" />
        </el-icon>
        <span class="role-label">{{ getRoleLabel(role) }}</span>
      </el-button>
    </div>

    <el-button
      type="info"
      text
      class="logout-button"
      @click="handleLogout"
      :loading="authStore.isLoading"
    >
      <el-icon><SwitchButton /></el-icon>
      Выйти из системы
    </el-button>
  </el-card>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Setting, User, SwitchButton } from '@element-plus/icons-vue'
import { useAuthStore } from '@/entities/auth/store/authStore'

const router = useRouter()
const authStore = useAuthStore()

const isLoggingOut = ref(false)

const availableRoles = computed(() => authStore.userRoles)

const getRoleLabel = (role: string): string => {
  const labels: Record<string, string> = {
    Administrator: 'Администратор',
    Manager: 'Менеджер',
  }
  return labels[role] || role
}

const getRoleIcon = (role: string): any => {
  const icons: Record<string, any> = {
    Administrator: Setting,
    Manager: User,
  }
  return icons[role] || User
}

const handleSelectRole = (role: string) => {
  authStore.selectRole(role)
  ElMessage.success(`Вы вошли как ${getRoleLabel(role)}`)
  router.push('/dashboard')
}

const handleLogout = async () => {
  isLoggingOut.value = true
  try {
    await authStore.logout()
    router.push('/login')
  } finally {
    isLoggingOut.value = false
  }
}
</script>

<style scoped>
.role-card {
  border-radius: var(--border-radius-lg);
  border: 1px solid var(--color-border-light);
  padding: var(--spacing-xl);
}

.role-title {
  font-size: var(--font-size-2xl);
  font-weight: 600;
  color: var(--color-text-primary);
  margin-bottom: var(--spacing-xs);
  text-align: center;
}

.role-subtitle {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  margin-bottom: var(--spacing-xl);
  text-align: center;
}

.role-buttons {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  width: 100%;
  margin-bottom: var(--spacing-lg);
}

.role-button {
  width: 100%;
  box-sizing: border-box;
  margin-left: 0;
  height: 72px;
  font-size: var(--font-size-md);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-sm);
  border-radius: var(--border-radius-md);
  background: var(--gradient-primary);
  border: none;
  transition: all var(--transition-base);
}

.role-button:hover {
  opacity: 0.9;
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.role-button--administrator,
.role-button--administrator:hover,
.role-button--administrator:focus {
  background: var(--color-accent);
}

.role-button--administrator:hover {
  background: var(--color-accent-dark);
}

.role-icon {
  margin-right: var(--spacing-xs);
}

.role-label {
  font-weight: 500;
}

.logout-button {
  width: 100%;
  margin-top: var(--spacing-md);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-xs);
}
</style>
