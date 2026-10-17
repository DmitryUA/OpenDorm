<template>
  <el-container class="main-layout">
    <!-- Боковое меню -->
    <el-aside :width="isCollapsed ? '64px' : '240px'" class="sidebar">
      <div class="sidebar-header">
        <el-icon :size="28" color="#fff">
          <OfficeBuilding />
        </el-icon>
        <transition name="fade">
          <span v-if="!isCollapsed" class="sidebar-title">OpenDorm</span>
        </transition>
      </div>

      <el-scrollbar class="sidebar-menu-wrapper">
        <el-menu
          :default-active="activeMenu"
          :collapse="isCollapsed"
          :collapse-transition="false"
          router
          background-color="transparent"
          text-color="rgba(255, 255, 255, 0.75)"
          active-text-color="#ffffff"
          class="sidebar-menu"
        >
          <el-menu-item v-for="item in visibleMenuItems" :key="item.key" :index="item.path">
            <el-icon><component :is="item.icon" /></el-icon>
            <template #title>{{ item.title }}</template>
          </el-menu-item>
        </el-menu>
      </el-scrollbar>

      <div class="sidebar-footer">
        <el-button text class="collapse-button" @click="toggleCollapse">
          <el-icon :size="20">
            <component :is="isCollapsed ? Expand : Fold" />
          </el-icon>
        </el-button>
      </div>
    </el-aside>

    <!-- Основная область -->
    <el-container class="main-container">
      <!-- Шапка -->
      <el-header class="header">
        <div class="header-left">
          <el-breadcrumb separator="/">
            <el-breadcrumb-item :to="{ path: '/dashboard' }"> Главная </el-breadcrumb-item>
            <el-breadcrumb-item v-if="currentPageTitle !== 'Главная'">
              {{ currentPageTitle }}
            </el-breadcrumb-item>
          </el-breadcrumb>
        </div>

        <div class="header-right">
          <!-- Выбранная роль -->
          <el-tag
            v-if="authStore.selectedRole"
            :type="authStore.selectedRole === 'Administrator' ? 'danger' : 'primary'"
            effect="dark"
            class="role-tag"
          >
            {{ getRoleLabel(authStore.selectedRole) }}
          </el-tag>

          <!-- Пользователь -->
          <el-dropdown trigger="click" @command="handleUserCommand">
            <div class="user-info">
              <el-avatar :size="36" class="user-avatar">
                {{ userInitials }}
              </el-avatar>
              <div class="user-details">
                <div class="user-name">{{ authStore.username || 'Пользователь' }}</div>
                <div class="user-email">{{ authStore.user?.sub || '' }}</div>
              </div>
              <el-icon class="dropdown-icon"><ArrowDown /></el-icon>
            </div>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item disabled>
                  <el-icon><User /></el-icon>
                  Профиль (скоро)
                </el-dropdown-item>
                <el-dropdown-item divided command="logout">
                  <el-icon><SwitchButton /></el-icon>
                  Выйти
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </div>
      </el-header>

      <!-- Контент -->
      <el-main class="main-content">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessageBox, ElMessage } from 'element-plus'
import {
  OfficeBuilding,
  Expand,
  Fold,
  ArrowDown,
  User,
  SwitchButton,
} from '@element-plus/icons-vue'
import { useAuthStore } from '@/entities/auth/store/authStore'
import { menuItems } from '@/shared/config/menu'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const isCollapsed = ref(false)

// Текущий активный пункт меню
const activeMenu = computed(() => route.path)

// Текущая страница (для хлебных крошек)
const currentPageTitle = computed(() => {
  const item = menuItems.find((i) => i.path === route.path)
  return item?.title || (route.meta.title as string | undefined) || 'Главная'
})

// Меню с учётом ролей
const visibleMenuItems = computed(() => {
  return menuItems.filter((item) => {
    // Если роли не указаны — пункт виден всем
    if (!item.roles || item.roles.length === 0) return true

    if (!authStore.selectedRole) return false
    return item.roles.includes(authStore.selectedRole)
  })
})

// Инициалы пользователя для аватара
const userInitials = computed(() => {
  const name = authStore.username
  if (!name) return '?'
  return name
    .split(' ')
    .map((w) => w[0])
    .join('')
    .toUpperCase()
    .slice(0, 2)
})

// Подписи ролей
const getRoleLabel = (role: string): string => {
  const labels: Record<string, string> = {
    Administrator: 'Администратор',
    Manager: 'Менеджер',
  }
  return labels[role] || role
}

const toggleCollapse = () => {
  isCollapsed.value = !isCollapsed.value
}

const handleUserCommand = async (command: string) => {
  if (command === 'logout') {
    try {
      await ElMessageBox.confirm('Вы уверены, что хотите выйти из системы?', 'Выход', {
        confirmButtonText: 'Выйти',
        cancelButtonText: 'Отмена',
        type: 'warning',
      })
      await authStore.logout()
      ElMessage.success('Вы вышли из системы')
      router.push('/login')
    } catch {
      // Пользователь отменил
    }
  }
}
</script>

<style scoped>
.main-layout {
  min-height: 100vh;
  background: var(--color-bg-page);
}

/* ===== Сайдбар ===== */
.sidebar {
  background: var(--color-primary);
  transition: width var(--transition-base);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 2px 0 8px rgba(0, 0, 0, 0.08);
}

.sidebar-header {
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-sm);
  padding: 0 var(--spacing-md);
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.sidebar-title {
  font-size: var(--font-size-xl);
  font-weight: 700;
  color: #ffffff;
  white-space: nowrap;
  letter-spacing: -0.5px;
}

.sidebar-menu-wrapper {
  flex: 1;
  overflow-y: auto;
}

.sidebar-menu {
  border-right: none;
}

.sidebar-menu :deep(.el-menu-item) {
  height: 48px;
  line-height: 48px;
  margin: 4px 8px;
  border-radius: var(--border-radius-md);
  transition: all var(--transition-fast);
}

.sidebar-menu :deep(.el-menu-item:hover) {
  background: rgba(255, 255, 255, 0.1) !important;
}

.sidebar-menu :deep(.el-menu-item.is-active) {
  background: rgba(255, 255, 255, 0.18) !important;
  color: #ffffff !important;
  font-weight: 500;
}

.sidebar-footer {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  padding: var(--spacing-sm);
}

.collapse-button {
  width: 100%;
  color: rgba(255, 255, 255, 0.75) !important;
}

.collapse-button:hover {
  color: #ffffff !important;
  background: rgba(255, 255, 255, 0.1) !important;
}

/* ===== Шапка ===== */
.header {
  height: 64px;
  background: var(--color-bg-white);
  border-bottom: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--spacing-lg);
  box-shadow: var(--shadow-sm);
}

.header-left {
  display: flex;
  align-items: center;
}

.header-right {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.role-tag {
  font-weight: 500;
}

/* ===== Пользователь ===== */
.user-info {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  cursor: pointer;
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--border-radius-md);
  transition: background var(--transition-fast);
}

.user-info:hover {
  background: var(--color-bg-hover);
}

.user-avatar {
  background: var(--gradient-primary);
  color: #ffffff;
  font-weight: 600;
  font-size: var(--font-size-sm);
}

.user-details {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.user-name {
  font-size: var(--font-size-sm);
  font-weight: 500;
  color: var(--color-text-primary);
  line-height: 1.2;
}

.user-email {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  line-height: 1.2;
}

.dropdown-icon {
  color: var(--color-text-secondary);
  font-size: 12px;
}

/* ===== Контент ===== */
.main-container {
  flex: 1;
  overflow: hidden;
}

.main-content {
  padding: var(--spacing-lg);
  overflow-y: auto;
}

/* ===== Анимации ===== */
.fade-enter-active,
.fade-leave-active {
  transition: opacity var(--transition-base);
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
