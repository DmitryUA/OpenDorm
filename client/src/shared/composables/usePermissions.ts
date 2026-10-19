import { computed } from 'vue'
import { useAuthStore } from '@/entities/auth/store/authStore'

export const usePermissions = () => {
  const authStore = useAuthStore()

  const isAdmin = computed(() => authStore.selectedRole === 'Administrator')
  const isManager = computed(() => authStore.selectedRole === 'Manager')

  const can = (permission: string) => authStore.hasPermission(permission)

  const canCreateDormitory = computed(() => can('dormitories.create'))
  const canUpdateDormitory = computed(() => can('dormitories.update'))
  const canCreateRoom = computed(() => can('rooms.create'))
  const canCheckIn = computed(() => can('accommodations.checkin'))
  const canManageUsers = computed(
    () => can('users.create') || can('users.update') || can('users.activate'),
  )

  return {
    isAdmin,
    isManager,
    can,
    canCreateDormitory,
    canUpdateDormitory,
    canCreateRoom,
    canCheckIn,
    canManageUsers,
  }
}
