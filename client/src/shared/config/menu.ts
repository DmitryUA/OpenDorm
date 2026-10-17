import type { Component } from 'vue'
import {
  House,
  OfficeBuilding,
  User,
  Tickets,
  UserFilled,
  Setting,
  HomeFilled,
} from '@element-plus/icons-vue'

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
    key: 'rooms',
    path: '/rooms',
    title: 'Комнаты',
    icon: HomeFilled,
  },
  {
    key: 'occupants',
    path: '/occupants',
    title: 'Жильцы',
    icon: User,
  },
  {
    key: 'accommodations',
    path: '/accommodations',
    title: 'Заселения',
    icon: Tickets,
  },
  {
    key: 'users',
    path: '/users',
    title: 'Пользователи',
    icon: UserFilled,
    roles: ['Administrator'], // Только для администратора
  },
  {
    key: 'settings',
    path: '/settings',
    title: 'Настройки',
    icon: Setting,
    roles: ['Administrator'],
  },
]
