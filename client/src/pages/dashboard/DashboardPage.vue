<template>
  <div class="dashboard-page">
    <div class="page-header mb-lg">
      <h1 class="page-title">Добро пожаловать, {{ authStore.username }}!</h1>
      <p class="page-subtitle">
        Вы вошли как <strong>{{ getRoleLabel(authStore.selectedRole || '') }}</strong>
      </p>
    </div>

    <RetryErrorAlert
      v-if="summaryError"
      :message="summaryError"
      @retry="loadSummary"
    />

    <el-row v-loading="isSummaryLoading" :gutter="20">
      <el-col :xs="24" :sm="12" :md="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-content">
            <div class="stat-icon stat-icon--primary">
              <el-icon :size="28"><OfficeBuilding /></el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-value">{{ summary?.dormitoryCount ?? '—' }}</div>
              <div class="stat-label">Общежитий</div>
            </div>
          </div>
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="12" :md="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-content">
            <div class="stat-icon stat-icon--info">
              <el-icon :size="28"><HomeFilled /></el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-value">{{ summary?.activeRoomCount ?? '—' }}</div>
              <div class="stat-label">Комнат</div>
            </div>
          </div>
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="12" :md="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-content">
            <div class="stat-icon stat-icon--success">
              <el-icon :size="28"><User /></el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-value">{{ summary?.activeOccupantCount ?? '—' }}</div>
              <div class="stat-label">Жильцов</div>
            </div>
          </div>
        </el-card>
      </el-col>

      <el-col :xs="24" :sm="12" :md="6">
        <el-card shadow="hover" class="stat-card">
          <div class="stat-content">
            <div class="stat-icon stat-icon--accent">
              <el-icon :size="28"><Tickets /></el-icon>
            </div>
            <div class="stat-info">
              <div class="stat-value">{{ summary?.accommodationCount ?? '—' }}</div>
              <div class="stat-label">Заселений</div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-card class="welcome-card mt-lg" shadow="never">
      <template #header>
        <div class="card-header">
          <span>Информация о системе</span>
        </div>
      </template>
      <p class="welcome-text">
        <strong>OpenDorm</strong> — система администрирования общежитий для образовательных
        организаций.
      </p>
      <p class="welcome-text">
        Используйте боковое меню для навигации. Разделы, доступные вам, зависят от выбранной роли.
      </p>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { OfficeBuilding, HomeFilled, User, Tickets } from '@element-plus/icons-vue'
import RetryErrorAlert from '@/components/RetryErrorAlert.vue'
import { useAuthStore } from '@/entities/auth/store/authStore'
import { getOpenDormAPIV1 } from '@/shared/api/generated/endpoints'
import type { SummaryDto } from '@/shared/api/generated/models'

const authStore = useAuthStore()
const summary = ref<SummaryDto | null>(null)
const isSummaryLoading = ref(false)
const summaryError = ref<string | null>(null)

const loadSummary = async () => {
  isSummaryLoading.value = true
  summaryError.value = null

  try {
    const { getApiDashboardsSummary } = getOpenDormAPIV1()
    summary.value = await getApiDashboardsSummary()
  } catch (caughtError) {
    const apiError = caughtError as { response?: { data?: { title?: string; detail?: string } } }
    summaryError.value =
      apiError.response?.data?.detail ||
      apiError.response?.data?.title ||
      'Не удалось загрузить статистику'
  } finally {
    isSummaryLoading.value = false
  }
}

const getRoleLabel = (role: string): string => {
  const labels: Record<string, string> = {
    Administrator: 'Администратор',
    Manager: 'Менеджер',
  }
  return labels[role] || role
}

onMounted(loadSummary)
</script>

<style scoped>
.page-title {
  font-size: var(--font-size-2xl);
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0 0 var(--spacing-xs) 0;
}

.page-subtitle {
  font-size: var(--font-size-base);
  color: var(--color-text-secondary);
  margin: 0;
}

.stat-card {
  border-radius: var(--border-radius-lg);
  transition: transform var(--transition-base);
}

.stat-card:hover {
  transform: translateY(-2px);
}

.stat-content {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
}

.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: var(--border-radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #ffffff;
}

.stat-icon--primary {
  background: var(--color-primary);
}
.stat-icon--info {
  background: var(--color-info);
}
.stat-icon--success {
  background: var(--color-success);
}
.stat-icon--accent {
  background: var(--color-accent);
}

.stat-info {
  display: flex;
  flex-direction: column;
}

.stat-value {
  font-size: var(--font-size-2xl);
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: 1.2;
}

.stat-label {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  margin-top: 2px;
}

.welcome-card {
  border-radius: var(--border-radius-lg);
}

.card-header {
  font-weight: 600;
  font-size: var(--font-size-md);
}

.welcome-text {
  color: var(--color-text-regular);
  line-height: 1.6;
  margin: 0 0 var(--spacing-sm) 0;
}

.welcome-text:last-child {
  margin-bottom: 0;
}
</style>
