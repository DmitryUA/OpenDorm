import type { Component } from 'vue'
import { House, OfficeBuilding, User, UserFilled } from '@element-plus/icons-vue'

export interface MenuItem {
  key: string
  path: string
  title: string
  icon: Component
  roles?: string[] // Если не указано — доступно всем ролям
  children?: MenuItem[]
}

export const menuItems: MenuItem[] = [
  {
    key: 'dashboard',
    path: '/dashboard',
    title: 'Главная',
    icon: House,
  },
  {
    key: 'dormitories',
    path: '/dormitories',
    title: 'Общежития',
    icon: OfficeBuilding,
  },
  {
    key: 'occupants',
    path: '/occupants',
    title: 'Жильцы',
    icon: User,
  },
  {
    key: 'users',
    path: '/users',
    title: 'Пользователи',
    icon: UserFilled,
    roles: ['Administrator'], // Только для администратора
  },
]
