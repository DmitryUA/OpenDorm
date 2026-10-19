<template>
  <article class="room-card">
    <button
      type="button"
      class="room-card-link"
      :aria-label="`Открыть комнату ${room.name}`"
      @click="emit('open', room)"
    >
      <header class="room-card-header">
        <h3 class="room-name">Комната {{ room.name }}</h3>
        <el-tag :type="room.isActive ? 'success' : 'info'" effect="light">
          {{ room.isActive ? 'Активна' : 'Неактивна' }}
        </el-tag>
      </header>
      <dl class="room-details">
        <div>
          <dt>Этаж</dt>
          <dd>{{ room.floorNumber }}</dd>
        </div>
        <div>
          <dt>Тип</dt>
          <dd>{{ genderLabel }}</dd>
        </div>
        <div>
          <dt>Жильцы</dt>
          <dd>{{ room.occupantCount }} / {{ room.capacity }}</dd>
        </div>
      </dl>
    </button>
  </article>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { RoomListDto } from '@/shared/api/generated/models'

const props = defineProps<{
  room: RoomListDto
}>()

const emit = defineEmits<{
  open: [room: RoomListDto]
}>()

const genderLabel = computed(() => {
  if (props.room.gender === 0) return 'Мужская'
  if (props.room.gender === 1) return 'Женская'
  return 'Не указан'
})
</script>

<style scoped>
.room-card {
  min-width: 0;
  padding: var(--spacing-md);
  border: 1px solid var(--color-border);
  border-radius: var(--border-radius-md);
  background: var(--color-bg-white);
}

.room-card-link {
  display: block;
  width: 100%;
  padding: 0;
  border: 0;
  background: transparent;
  color: inherit;
  cursor: pointer;
  text-align: left;
}

.room-card-link:focus-visible {
  border-radius: var(--border-radius-sm);
  outline: 2px solid var(--color-primary);
  outline-offset: 4px;
}

.room-card-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--spacing-sm);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--color-border-light);
}

.room-name {
  margin: 0;
  color: var(--color-text-primary);
  font-size: var(--font-size-md);
  font-weight: 600;
  overflow-wrap: anywhere;
}

.room-details {
  display: grid;
  gap: var(--spacing-sm);
  margin: var(--spacing-md) 0 0;
}

.room-details > div {
  display: flex;
  justify-content: space-between;
  gap: var(--spacing-sm);
}

.room-details dt {
  color: var(--color-text-secondary);
}

.room-details dd {
  margin: 0;
  color: var(--color-text-primary);
  font-weight: 500;
  text-align: right;
}
</style>
