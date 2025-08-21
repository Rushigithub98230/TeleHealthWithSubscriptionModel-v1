import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'info' | 'warning';
  title: string;
  message: string;
  duration?: number;
  timestamp: Date;
  read: boolean;
}

export interface NotificationOptions {
  duration?: number;
  showClose?: boolean;
  position?: 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left';
}

@Injectable({
  providedIn: 'root'
})
export class AdminNotificationService {
  private notificationsSubject = new Subject<Notification[]>();
  private notifications: Notification[] = [];
  private nextId = 1;

  public notifications$ = this.notificationsSubject.asObservable();

  constructor() {
    // Load notifications from localStorage on service initialization
    this.loadNotifications();
  }

  // Show success notification
  success(title: string, message: string, options: NotificationOptions = {}): void {
    this.showNotification({
      type: 'success',
      title,
      message,
      duration: options.duration || 5000,
      showClose: options.showClose !== false,
      position: options.position || 'top-right'
    });
  }

  // Show error notification
  error(title: string, message: string, options: NotificationOptions = {}): void {
    this.showNotification({
      type: 'error',
      title,
      message,
      duration: options.duration || 8000,
      showClose: options.showClose !== false,
      position: options.position || 'top-right'
    });
  }

  // Show info notification
  info(title: string, message: string, options: NotificationOptions = {}): void {
    this.showNotification({
      type: 'info',
      title,
      message,
      duration: options.duration || 4000,
      showClose: options.showClose !== false,
      position: options.position || 'top-right'
    });
  }

  // Show warning notification
  warning(title: string, message: string, options: NotificationOptions = {}): void {
    this.showNotification({
      type: 'warning',
      title,
      message,
      duration: options.duration || 6000,
      showClose: options.showClose !== false,
      position: options.position || 'top-right'
    });
  }

  // Show notification with custom options
  private showNotification(options: {
    type: 'success' | 'error' | 'info' | 'warning';
    title: string;
    message: string;
    duration: number;
    showClose: boolean;
    position: string;
  }): void {
    const notification: Notification = {
      id: `notification-${this.nextId++}`,
      type: options.type,
      title: options.title,
      message: options.message,
      duration: options.duration,
      timestamp: new Date(),
      read: false
    };

    this.notifications.unshift(notification);
    this.updateNotifications();
    this.saveNotifications();

    // Auto-remove notification after duration
    if (options.duration > 0) {
      setTimeout(() => {
        this.removeNotification(notification.id);
      }, options.duration);
    }
  }

  // Remove notification by ID
  removeNotification(id: string): void {
    this.notifications = this.notifications.filter(n => n.id !== id);
    this.updateNotifications();
    this.saveNotifications();
  }

  // Mark notification as read
  markAsRead(id: string): void {
    const notification = this.notifications.find(n => n.id === id);
    if (notification) {
      notification.read = true;
      this.updateNotifications();
      this.saveNotifications();
    }
  }

  // Mark all notifications as read
  markAllAsRead(): void {
    this.notifications.forEach(n => n.read = true);
    this.updateNotifications();
    this.saveNotifications();
  }

  // Clear all notifications
  clearAll(): void {
    this.notifications = [];
    this.updateNotifications();
    this.saveNotifications();
  }

  // Get unread notifications count
  getUnreadCount(): number {
    return this.notifications.filter(n => !n.read).length;
  }

  // Get all notifications
  getAllNotifications(): Notification[] {
    return [...this.notifications];
  }

  // Get notifications by type
  getNotificationsByType(type: 'success' | 'error' | 'info' | 'warning'): Notification[] {
    return this.notifications.filter(n => n.type === type);
  }

  // Update notifications subject
  private updateNotifications(): void {
    this.notificationsSubject.next([...this.notifications]);
  }

  // Save notifications to localStorage
  private saveNotifications(): void {
    try {
      const notificationsToSave = this.notifications.slice(0, 50); // Keep only last 50 notifications
      localStorage.setItem('admin-notifications', JSON.stringify(notificationsToSave));
    } catch (error) {
      console.error('Failed to save notifications to localStorage:', error);
    }
  }

  // Load notifications from localStorage
  private loadNotifications(): void {
    try {
      const savedNotifications = localStorage.getItem('admin-notifications');
      if (savedNotifications) {
        this.notifications = JSON.parse(savedNotifications).map((n: any) => ({
          ...n,
          timestamp: new Date(n.timestamp)
        }));
        this.updateNotifications();
      }
    } catch (error) {
      console.error('Failed to load notifications from localStorage:', error);
    }
  }

  // Get notification icon based on type
  getNotificationIcon(type: 'success' | 'error' | 'info' | 'warning'): string {
    const icons = {
      success: 'fas fa-check-circle',
      error: 'fas fa-exclamation-circle',
      info: 'fas fa-info-circle',
      warning: 'fas fa-exclamation-triangle'
    };
    return icons[type];
  }

  // Get notification color class based on type
  getNotificationColorClass(type: 'success' | 'error' | 'info' | 'warning'): string {
    const colors = {
      success: 'notification-success',
      error: 'notification-error',
      info: 'notification-info',
      warning: 'notification-warning'
    };
    return colors[type];
  }

  // Get notification background color
  getNotificationBackgroundColor(type: 'success' | 'error' | 'info' | 'warning'): string {
    const colors = {
      success: '#d4edda',
      error: '#f8d7da',
      info: '#d1ecf1',
      warning: '#fff3cd'
    };
    return colors[type];
  }

  // Get notification text color
  getNotificationTextColor(type: 'success' | 'error' | 'info' | 'warning'): string {
    const colors = {
      success: '#155724',
      error: '#721c24',
      info: '#0c5460',
      warning: '#856404'
    };
    return colors[type];
  }

  // Get notification border color
  getNotificationBorderColor(type: 'success' | 'error' | 'info' | 'warning'): string {
    const colors = {
      success: '#c3e6cb',
      error: '#f5c6cb',
      info: '#bee5eb',
      warning: '#ffeaa7'
    };
    return colors[type];
  }
}
