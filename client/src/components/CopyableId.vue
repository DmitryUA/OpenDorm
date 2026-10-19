<template>
  <el-button
    class="copy-id-button"
    text
    :title="`Скопировать ID ${label} ${id}`"
    :aria-label="`Скопировать ID ${label} ${id}`"
    @click="copyId"
  >
    <span>{{ id }}</span>
    <el-icon><CopyDocument /></el-icon>
  </el-button>
</template>

<script setup lang="ts">
import { ElMessage } from 'element-plus'
import { CopyDocument } from '@element-plus/icons-vue'

const props = defineProps<{
  id: string
  label: string
}>()

const copyId = async () => {
  try {
    await navigator.clipboard.writeText(props.id)
    ElMessage.success('ID скопирован')
  } catch {
    ElMessage.error('Не удалось скопировать ID')
  }
}
</script>

<style scoped>
.copy-id-button {
  display: flex;
  justify-content: flex-start;
  gap: var(--spacing-sm);
  min-width: 0;
  max-width: 100%;
  height: auto;
  padding: 2px 0 2px 8px;
  color: var(--color-primary);
  white-space: normal;
}

.copy-id-button span {
  flex: 1;
  min-width: 0;
  font-family: monospace;
  overflow-wrap: anywhere;
  text-align: left;
  white-space: normal;
}
</style>
