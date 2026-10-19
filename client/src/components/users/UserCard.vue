<template>
  <article class="user-card">
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

    <div v-if="isAdmin" class="user-card-id">
      <span class="detail-label">ID пользователя</span>
      <CopyableId :id="user.id" label="пользователя" />
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
        <el-button v-if="can('users.update')" text @click="emit('edit-roles', user)">
          Роли
        </el-button>
        <el-button v-if="can('users.update')" text @click="emit('edit', user)">
          Редактировать
        </el-button>
        <el-button
          v-if="user.isActive && can('users.deactivate')"
          text
          type="danger"
          :loading="isUpdating"
          @click="emit('change-status', user, false)"
        >
          Деактивировать
        </el-button>
        <el-button
          v-else-if="!user.isActive && can('users.activate')"
          text
          type="success"
          :loading="isUpdating"
          @click="emit('change-status', user, true)"
        >
          Активировать
        </el-button>
      </div>
    </div>
  </article>
</template>

<script setup lang="ts">
import CopyableId from '@/components/CopyableId.vue'
import type { UserDto } from '@/shared/api/generated/models'
import { usePermissions } from '@/shared/composables/usePermissions'

defineProps<{
  user: UserDto
  isAdmin: boolean
  isUpdating: boolean
}>()

const emit = defineEmits<{
  edit: [user: UserDto]
  'edit-roles': [user: UserDto]
  'change-status': [user: UserDto, activate: boolean]
}>()

const { can } = usePermissions()

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
</script>

<style scoped>
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

.user-card-id {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.detail-label {
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
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
</style>
