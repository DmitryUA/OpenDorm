import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getDormitories } from '@/shared/api/generated/dormitories'
import type { DormitoryListDto, CreateDormitoryRequest } from '@/shared/api/generated/models'

export const useDormitoryStore = defineStore('dormitory', () => {
  // Получаем функции из сгенерированного объекта
  const { getApiDormitories, postApiDormitories } = getDormitories()

  const dormitories = ref<DormitoryListDto[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const fetchDormitories = async () => {
    isLoading.value = true
    error.value = null
    try {
      dormitories.value = await getApiDormitories()
    } catch (e: any) {
      error.value = e.response?.data?.title || 'Ошибка загрузки общежитий'
      throw e
    } finally {
      isLoading.value = false
    }
  }

  const createDormitory = async (dto: CreateDormitoryRequest): Promise<string> => {
    const id = await postApiDormitories(dto)
    // После успешного создания обновляем список
    await fetchDormitories()
    return id
  }

  return {
    dormitories,
    isLoading,
    error,
    fetchDormitories,
    createDormitory,
  }
})
