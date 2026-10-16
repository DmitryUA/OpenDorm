import axios from 'axios'
import type { AxiosRequestConfig } from 'axios'

export const AXIOS_INSTANCE = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5140',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

AXIOS_INSTANCE.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export const customInstance = <T>(config: AxiosRequestConfig): Promise<T> => {
  return AXIOS_INSTANCE.request<T>(config).then((response) => response.data)
}

export default customInstance
