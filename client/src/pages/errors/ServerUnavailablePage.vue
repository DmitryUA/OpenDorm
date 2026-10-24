<template>
  <main class="server-unavailable-page">
    <div class="server-unavailable-content">
      <div class="error-code">503</div>
      <h1 class="error-title">Сервер временно недоступен</h1>
      <p class="error-subtitle">
        Возможно, ведутся технические работы. Попробуйте войти позже.
      </p>
      <el-button type="primary" size="large" @click="router.push({ name: 'Login' })">
        Вернуться ко входу
      </el-button>
    </div>
  </main>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()
</script>

<style scoped>
.server-unavailable-page {
  min-height: calc(100vh - 32px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--spacing-lg);
}

.server-unavailable-content {
  max-width: 480px;
  text-align: center;
}

.error-code {
  margin-bottom: var(--spacing-md);
  background: var(--gradient-primary);
  background-clip: text;
  color: transparent;
  font-size: 120px;
  font-weight: 800;
  line-height: 1;
  -webkit-text-fill-color: transparent;
}

.error-title {
  margin: 0 0 var(--spacing-sm);
  color: var(--color-text-primary);
  font-size: var(--font-size-2xl);
  font-weight: 600;
}

.error-subtitle {
  margin: 0 0 var(--spacing-xl);
  color: var(--color-text-secondary);
  font-size: var(--font-size-base);
}
</style>
