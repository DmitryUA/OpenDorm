<template>
  <el-alert :title="message" type="error" :closable="false" show-icon>
    <template #default>
      <el-button link type="primary" @click="emit('retry')">Повторить загрузку</el-button>
    </template>
  </el-alert>
</template>

<script setup lang="ts">
defineProps<{
  message: string
}>()

const emit = defineEmits<{
  retry: []
}>()
</script>
