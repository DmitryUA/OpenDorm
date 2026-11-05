import axios from 'axios'
import type { AxiosError, AxiosRequestConfig, InternalAxiosRequestConfig } from 'axios'
import { isTokenExpired } from '@/shared/lib/jwt'

declare module 'axios' {
  interface InternalAxiosRequestConfig {
    _authRetry?: boolean
  }
}

export const AXIOS_INSTANCE = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

type TokenRefreshHandler = () => Promise<string>

let refreshTokenHandler: TokenRefreshHandler | null = null

export const setTokenRefreshHandler = (handler: TokenRefreshHandler) => {
  refreshTokenHandler = handler
}

const isAuthEndpoint = (url?: string) => url?.startsWith('/api/auth/')

AXIOS_INSTANCE.interceptors.request.use(async (config: InternalAxiosRequestConfig) => {
  let token = localStorage.getItem('accessToken')

  if (token && isTokenExpired(token) && refreshTokenHandler && !isAuthEndpoint(config.url)) {
    token = await refreshTokenHandler()
  }

  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  return config
})

AXIOS_INSTANCE.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const config = error.config
    if (
      error.response?.status !== 401 ||
      !config ||
      config._authRetry ||
      isAuthEndpoint(config.url) ||
      !refreshTokenHandler
    ) {
      return Promise.reject(error)
    }

    config._authRetry = true
    try {
      const token = await refreshTokenHandler()
      config.headers.Authorization = `Bearer ${token}`
      return await AXIOS_INSTANCE.request(config)
    } catch {
      return Promise.reject(error)
    }
  },
)

export const customInstance = <T>(config: AxiosRequestConfig): Promise<T> => {
  return AXIOS_INSTANCE.request<T>(config).then((response) => response.data)
}

export default customInstance
