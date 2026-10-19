<template>
  <article class="dormitory-card">
    <button
      type="button"
      class="card-main"
      :aria-label="`Открыть общежитие: ${dormitory.city}, ${dormitory.street}, ${dormitory.house}`"
      @click="emit('open', dormitory.id)"
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
      <CopyableId :id="dormitory.id" label="общежития" />
    </div>
  </article>
</template>

<script setup lang="ts">
import { OfficeBuilding } from '@element-plus/icons-vue'
import CopyableId from '@/components/CopyableId.vue'
import type { DormitoryListDto } from '@/shared/api/generated/models'

defineProps<{
  dormitory: DormitoryListDto
  isAdmin: boolean
}>()

const emit = defineEmits<{
  open: [id: string]
}>()
</script>

<style scoped>
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
</style>
