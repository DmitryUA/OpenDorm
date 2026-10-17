import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import ru from 'element-plus/es/locale/lang/ru'

import App from './App.vue'
import router from './app/router'
import { useAuthStore } from './entities/auth/store/authStore'
import './app/styles/global.css'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(ElementPlus, { locale: ru })

// Регистрируем все иконки Element Plus глобально
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

// Инициализируем auth store (восстановление сессии из localStorage)
const authStore = useAuthStore()
authStore.init()

app.mount('#app')
