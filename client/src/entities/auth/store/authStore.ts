import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { isAxiosError } from 'axios'
import { AXIOS_INSTANCE, setTokenRefreshHandler } from '@/shared/api/axiosInstance'
import { getAuth } from '@/shared/api/generated/auth'
import { decodeToken, isTokenExpired } from '@/shared/lib/jwt'
import type { LoginRequest, LoginResponse, LogoutCommand } from '@/shared/api/generated/models'
import type { JwtPayload } from '@/shared/lib/jwt'

// Ключ роли в JWT-claims (стандартный для ASP.NET Core Identity)
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

export const useAuthStore = defineStore('auth', () => {
  // Получаем сгенерированные Orval функции
  const { postApiAuthLogin, postApiAuthLogout, postApiAuthRefresh } = getAuth()

  // === State ===
  const accessToken = ref<string | null>(localStorage.getItem('accessToken'))
  const refreshToken = ref<string | null>(localStorage.getItem('refreshToken'))
  const selectedRole = ref<string | null>(localStorage.getItem('selectedRole'))
  const user = ref<JwtPayload | null>(null)
  const isLoading = ref(false)
  let refreshPromise: Promise<string> | null = null
  let refreshTimer: ReturnType<typeof setTimeout> | null = null

  // === Getters ===
  const isAuthenticated = computed(() => !!accessToken.value && !isTokenExpired(accessToken.value))

  const userRoles = computed<string[]>(() => {
    const roles = user.value?.[ROLE_CLAIM]
    if (!roles) return []
    return Array.isArray(roles) ? roles : [roles]
  })

  const userPermissions = computed<string[]>(() => user.value?.permissions ?? [])

  const hasMultipleRoles = computed(() => userRoles.value.length > 1)

  const needsRoleSelection = computed(
    () => isAuthenticated.value && hasMultipleRoles.value && !selectedRole.value,
  )

  const username = computed(() => user.value?.unique_name ?? '')

  const hasRole = (role: string) => userRoles.value.includes(role)
  const hasPermission = (permission: string) => userPermissions.value.includes(permission)
  const hasAnyRole = (roles: string[]) => roles.some((role) => hasRole(role))
  const hasAllPermissions = (permissions: string[]) => permissions.every((p) => hasPermission(p))

  // === Actions ===
  const setTokens = (access: string, refresh: string) => {
    accessToken.value = access
    refreshToken.value = refresh
    localStorage.setItem('accessToken', access)
    localStorage.setItem('refreshToken', refresh)
    scheduleTokenRefresh(access)
    AXIOS_INSTANCE.defaults.headers.common['Authorization'] = `Bearer ${access}`

    // Декодируем токен и сохраняем данные пользователя
    const payload = decodeToken(access)
    if (payload) {
      user.value = payload

      // Если роль одна — выбираем её автоматически
      const roles = payload[ROLE_CLAIM]
      const rolesArray = Array.isArray(roles) ? roles : [roles]
      if (rolesArray.length === 1) {
        selectRole(rolesArray[0])
      }
    }
  }

  const selectRole = (role: string) => {
    selectedRole.value = role
    localStorage.setItem('selectedRole', role)
  }

  const clearTokens = () => {
    if (refreshTimer) {
      clearTimeout(refreshTimer)
      refreshTimer = null
    }

    accessToken.value = null
    refreshToken.value = null
    user.value = null
    selectedRole.value = null
    localStorage.removeItem('accessToken')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('selectedRole')
    delete AXIOS_INSTANCE.defaults.headers.common['Authorization']
  }

  const refreshAccessToken = async () => {
    if (refreshPromise) return refreshPromise
    if (!refreshToken.value) {
      clearTokens()
      window.dispatchEvent(new Event('auth:session-expired'))
      throw new Error('Refresh token is unavailable.')
    }

    refreshPromise = postApiAuthRefresh({ refreshToken: refreshToken.value })
      .then((response) => {
        setTokens(response.accessToken, response.refreshToken)
        return response.accessToken
      })
      .catch((error: unknown) => {
        if (isAxiosError(error) && [400, 401].includes(error.response?.status ?? 0)) {
          clearTokens()
          window.dispatchEvent(new Event('auth:session-expired'))
        }
        throw error
      })
      .finally(() => {
        refreshPromise = null
      })

    return refreshPromise
  }

  const scheduleTokenRefresh = (token: string) => {
    if (refreshTimer) clearTimeout(refreshTimer)

    const payload = decodeToken(token)
    if (!payload) return

    const refreshInMs = Math.max(payload.exp * 1000 - Date.now() - 60_000, 0)
    refreshTimer = setTimeout(() => {
      void refreshAccessToken().catch(() => undefined)
    }, refreshInMs)
  }

  const login = async (dto: LoginRequest) => {
    isLoading.value = true
    try {
      const response: LoginResponse = await postApiAuthLogin(dto)
      setTokens(response.accessToken, response.refreshToken)
    } finally {
      isLoading.value = false
    }
  }

  const logout = async () => {
    try {
      if (refreshToken.value) {
        const logoutCommand: LogoutCommand = { refreshToken: refreshToken.value }
        await postApiAuthLogout(logoutCommand)
      }
    } catch {
      // Игнорируем ошибки при выходе — всё равно очищаем локальное состояние
    } finally {
      clearTokens()
    }
  }

  const init = async () => {
    setTokenRefreshHandler(refreshAccessToken)

    if (!accessToken.value && !refreshToken.value) return

    // Восстанавливаем заголовок авторизации
    AXIOS_INSTANCE.defaults.headers.common['Authorization'] = `Bearer ${accessToken.value}`

    // Проверяем срок действия токена
    if (!accessToken.value || isTokenExpired(accessToken.value)) {
      if (!refreshToken.value) {
        clearTokens()
        return
      }

      try {
        await refreshAccessToken()
      } catch {
        return
      }
    }

    if (!accessToken.value) return
    scheduleTokenRefresh(accessToken.value)

    // Декодируем токен
    const payload = decodeToken(accessToken.value)
    if (payload) {
      user.value = payload

      // Восстанавливаем выбранную роль (если она всё ещё валидна)
      const savedRole = localStorage.getItem('selectedRole')
      if (savedRole && userRoles.value.includes(savedRole)) {
        selectedRole.value = savedRole
      }
    }
  }

  return {
    // state
    accessToken,
    refreshToken,
    selectedRole,
    user,
    isLoading,
    // getters
    isAuthenticated,
    userRoles,
    userPermissions,
    hasMultipleRoles,
    needsRoleSelection,
    username,
    hasRole,
    hasPermission,
    hasAnyRole,
    hasAllPermissions,
    // actions
    login,
    logout,
    refreshAccessToken,
    selectRole,
    init,
    clearTokens,
  }
})
