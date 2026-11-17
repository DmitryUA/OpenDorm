<template>
  <el-tag
    class="status-tag"
    :class="isActive ? 'status-tag--active' : 'status-tag--inactive'"
    :type="isActive ? 'success' : 'info'"
    effect="light"
    :size="size"
  >
    {{ text }}
  </el-tag>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    text: string
    isActive: boolean
    size?: 'large' | 'default' | 'small'
  }>(),
  {
    size: 'default',
  },
)
</script>

<style scoped>
.status-tag.status-tag--active {
  --el-tag-border-color: var(--el-color-success-light-7);
}

.status-tag.status-tag--inactive {
  --el-tag-border-color: var(--el-color-info-light-7);
}
</style>
