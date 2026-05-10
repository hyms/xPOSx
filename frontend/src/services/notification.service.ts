import api from '@/api'
import type { Notification, NotificationSetting } from '@/types'

export const notificationService = {
  // Notifications
  getAll: () => api.get<Notification[]>('/notifications'),
  getUnreadCount: () => api.get<{ count: number }>('/notifications/unread-count'),
  markAsRead: (id: number) => api.put(`/notifications/${id}/read`),
  markAllAsRead: () => api.put('/notifications/read-all'),
  delete: (id: number) => api.delete(`/notifications/${id}`),

  // Settings
  getSettings: () => api.get<NotificationSetting[]>('/notifications/settings'),
  updateSetting: (key: string, value: boolean) => api.put(`/notifications/settings/${key}`, { value })
}
