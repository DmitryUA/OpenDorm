<template>
  <article class="occupant-card">
    <button
      type="button"
      class="card-main"
      :aria-label="`Открыть жильца: ${fullName}`"
      @click="emit('open', occupant.id)"
    >
      <div class="card-header">
        <el-avatar :size="42" class="occupant-avatar">
          {{ initials }}
        </el-avatar>
        <span class="status-badge" :class="occupant.isActive ? 'status-active' : 'status-inactive'">
          {{ occupant.isActive ? 'Активен' : 'Неактивен' }}
        </span>
      </div>

      <h2>{{ fullName }}</h2>

      <div class="meta-list">
        <span>{{ genderLabel }}</span>
        <span>{{ formattedBirthDate }}</span>
      </div>

      <p class="phone-text">{{ occupant.phone || 'Телефон не указан' }}</p>

      <p class="accommodation-text">
        <template v-if="occupant.currentAccommodation">
          {{ occupant.currentAccommodation.dormitoryAddress }} ·
          {{ occupant.currentAccommodation.roomName }}
        </template>
        <template v-else>На данный момент не заселен</template>
      </p>
    </button>
  </article>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { OccupantListDto } from '@/shared/api/generated/models'

const props = defineProps<{
  occupant: OccupantListDto
}>()

const emit = defineEmits<{
  open: [id: string]
}>()

const fullName = computed(() =>
  [props.occupant.lastName, props.occupant.firstName, props.occupant.patronymic]
    .filter(Boolean)
    .join(' '),
)

const initials = computed(
  () =>
    `${props.occupant.firstName?.charAt(0)?.toUpperCase() || ''}${props.occupant.lastName?.charAt(0)?.toUpperCase() || ''}`,
)

const genderLabel = computed(() => (props.occupant.gender === 1 ? 'Женский' : 'Мужской'))

const formattedBirthDate = computed(() => {
  const value = props.occupant.birthDate
  if (!value) return '—'

  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value

  return new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(date)
})
</script>

<style scoped>
.occupant-card {
  display: flex;
  min-width: 0;
}

.card-main {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
  width: 100%;
  height: 100%;
  min-height: 204px;
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

.card-main:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
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
  color: var(--color-text-primary);
  font-size: var(--font-size-lg);
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

.phone-text {
  margin: 0;
  color: var(--color-text-secondary);
  overflow-wrap: anywhere;
}
</style>
