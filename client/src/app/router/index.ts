import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/entities/auth/store/authStore'

const routes: RouteRecordRaw[] = [
  // 1. Маршруты авторизации (без сайдбара)
  {
    path: '/login',
    component: () => import('@/layouts/AuthLayout.vue'),
    children: [{ path: '', name: 'Login', component: () => import('@/pages/auth/LoginPage.vue') }],
  },
  {
    path: '/select-role',
    component: () => import('@/layouts/AuthLayout.vue'),
    children: [
      {
        path: '',
        name: 'SelectRole',
        component: () => import('@/pages/auth/RoleSelectionPage.vue'),
      },
    ],
  },

  // 2. Основные маршруты приложения (с сайдбаром и шапкой)
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    meta: { requiresAuth: true, requiresRoleSelection: true },
    children: [
      { path: '', redirect: '/dashboard' },
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/pages/dashboard/DashboardPage.vue'),
      },
      {
        path: 'dormitories',
        name: 'Dormitories',
        component: () => import('@/pages/dormitories/DormitoriesPage.vue'),
      },
      {
        path: 'occupants',
        name: 'Occupants',
        component: () => import('@/pages/occupants/OccupantsPage.vue'),
      },
      {
        path: 'occupants/:id',
        name: 'OccupantDetails',
        component: () => import('@/pages/occupants/OccupantDetailsPage.vue'),
        meta: { title: 'Жилец' },
      },
      {
        path: 'dormitories/:id',
        name: 'DormitoryDetails',
        component: () => import('@/pages/dormitories/DormitoryDetailsPage.vue'),
        meta: { title: 'Общежитие' },
      },
      {
        path: 'dormitories/:dormitoryId/rooms/:roomId',
        name: 'RoomDetails',
        component: () => import('@/pages/rooms/RoomDetailsPage.vue'),
        meta: { title: 'Комната' },
      },
      {
        path: 'users',
        name: 'Users',
        component: () => import('@/pages/users/UsersPage.vue'),
        meta: { roles: ['Administrator'] },
      },
    ],
  },

  /*
  {
      path: 'users',
      name: 'Users',
      component: () => import('@/pages/users/UserListPage.vue'),
      meta: { 
        requiresAuth: true, 
        requiresRoleSelection: true, 
        roles: ['Administrator'] // Только администратор
      },
    },
    */

  // 3. Страница 404
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('@/pages/errors/NotFoundPage.vue'),
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// Глобальный guard (без устаревшего next())
router.beforeEach((to) => {
  const authStore = useAuthStore()

  // 1. Если маршрут требует авторизации, а токена нет -> на логин
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return '/login'
  }

  // 2. Если пользователь авторизован, но не выбрал роль -> на выбор роли
  if (authStore.isAuthenticated && authStore.needsRoleSelection && to.path !== '/select-role') {
    return '/select-role'
  }

  // 3. Проверка прав на основе ВЫБРАННОЙ роли
  const requiredRoles = to.meta.roles as string[] | undefined
  if (requiredRoles && requiredRoles.length > 0) {
    if (!authStore.selectedRole || !requiredRoles.includes(authStore.selectedRole)) {
      return '/dashboard' // Или на страницу 403
    }
  }

  // 4. Если авторизованный пользователь лезет на страницы входа -> в дашборд
  if (
    (to.path === '/login' || to.path === '/select-role') &&
    authStore.isAuthenticated &&
    !authStore.needsRoleSelection
  ) {
    return '/dashboard'
  }

  return true
})

export default router
