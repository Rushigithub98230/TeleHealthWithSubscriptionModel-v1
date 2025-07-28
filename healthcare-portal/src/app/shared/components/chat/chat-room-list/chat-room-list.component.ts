import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';

import { ChatRoom, ChatParticipant } from '../../../core/services/chat.service';

@Component({
  selector: 'app-chat-room-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatListModule,
    MatDividerModule,
    MatBadgeModule,
    MatTooltipModule,
    MatMenuModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatTabsModule,
    MatExpansionModule,
    MatSlideToggleModule,
    MatCheckboxModule,
    MatSelectModule
  ],
  templateUrl: './chat-room-list.component.html',
  styleUrl: './chat-room-list.component.scss'
})
export class ChatRoomListComponent implements OnInit {
  @Input() rooms: ChatRoom[] = [];
  @Input() currentRoomId?: string;
  @Input() loading = false;
  @Input() showSearch = true;
  @Input() showFilters = true;
  @Input() showCreateRoom = true;
  @Input() currentUserId = '';

  @Output() roomSelected = new EventEmitter<ChatRoom>();
  @Output() roomCreated = new EventEmitter<Partial<ChatRoom>>();
  @Output() roomDeleted = new EventEmitter<string>();
  @Output() roomMuted = new EventEmitter<{ roomId: string; muted: boolean }>();
  @Output() roomPinned = new EventEmitter<{ roomId: string; pinned: boolean }>();

  // Filter and search state
  searchTerm = '';
  selectedFilter = 'all';
  showMutedRooms = true;
  showPinnedRooms = true;

  // Create room state
  showCreateRoomDialog = false;
  newRoomForm = {
    name: '',
    type: 'group',
    participants: [] as string[]
  };

  // Available filters
  filters = [
    { value: 'all', label: 'All Rooms', icon: 'chat' },
    { value: 'unread', label: 'Unread', icon: 'mark_email_unread' },
    { value: 'pinned', label: 'Pinned', icon: 'push_pin' },
    { value: 'direct', label: 'Direct', icon: 'person' },
    { value: 'group', label: 'Groups', icon: 'group' }
  ];

  // Room types
  roomTypes = [
    { value: 'individual', label: 'Direct Message', icon: 'person' },
    { value: 'group', label: 'Group Chat', icon: 'group' }
  ];

  constructor(
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    // Initialize component
  }

  // Filter and search methods
  getFilteredRooms(): ChatRoom[] {
    let filteredRooms = this.rooms;

    // Apply search filter
    if (this.searchTerm.trim()) {
      const searchLower = this.searchTerm.toLowerCase();
      filteredRooms = filteredRooms.filter(room => 
        room.name.toLowerCase().includes(searchLower) ||
        room.participants?.some(p => p.name.toLowerCase().includes(searchLower))
      );
    }

    // Apply type filter
    switch (this.selectedFilter) {
      case 'unread':
        filteredRooms = filteredRooms.filter(room => room.unreadCount > 0);
        break;
      case 'pinned':
        filteredRooms = filteredRooms.filter(room => room.isPinned);
        break;
      case 'direct':
        filteredRooms = filteredRooms.filter(room => room.type === 'individual');
        break;
      case 'group':
        filteredRooms = filteredRooms.filter(room => room.type === 'group');
        break;
    }

    // Apply visibility filters
    if (!this.showMutedRooms) {
      filteredRooms = filteredRooms.filter(room => !room.isMuted);
    }

    if (!this.showPinnedRooms) {
      filteredRooms = filteredRooms.filter(room => !room.isPinned);
    }

    return filteredRooms;
  }

  onSearchChange(): void {
    // Search is handled by getFilteredRooms()
  }

  onFilterChange(): void {
    // Filter is handled by getFilteredRooms()
  }

  // Room selection
  selectRoom(room: ChatRoom): void {
    this.roomSelected.emit(room);
  }

  isRoomSelected(room: ChatRoom): boolean {
    return room.id === this.currentRoomId;
  }

  // Room management
  createRoom(): void {
    this.showCreateRoomDialog = true;
  }

  submitCreateRoom(): void {
    if (this.newRoomForm.name.trim()) {
      this.roomCreated.emit({
        name: this.newRoomForm.name.trim(),
        type: this.newRoomForm.type as 'individual' | 'group',
        participants: this.newRoomForm.participants
      });
      this.closeCreateRoomDialog();
    } else {
      this.snackBar.open('Please enter a room name', 'Close', { duration: 2000 });
    }
  }

  closeCreateRoomDialog(): void {
    this.showCreateRoomDialog = false;
    this.newRoomForm = {
      name: '',
      type: 'group',
      participants: []
    };
  }

  deleteRoom(room: ChatRoom): void {
    // TODO: Add confirmation dialog
    this.roomDeleted.emit(room.id);
  }

  muteRoom(room: ChatRoom): void {
    this.roomMuted.emit({ roomId: room.id, muted: !room.isMuted });
  }

  pinRoom(room: ChatRoom): void {
    this.roomPinned.emit({ roomId: room.id, pinned: !room.isPinned });
  }

  // Room display methods
  getRoomDisplayName(room: ChatRoom): string {
    if (room.type === 'individual') {
      const otherParticipant = room.participants?.find(p => p.id !== this.currentUserId);
      return otherParticipant?.name || room.name;
    }
    return room.name;
  }

  getRoomAvatar(room: ChatRoom): string {
    if (room.type === 'individual') {
      const otherParticipant = room.participants?.find(p => p.id !== this.currentUserId);
      return otherParticipant?.avatar || 'assets/default-avatar.png';
    }
    return room.avatar || 'assets/default-avatar.png';
  }

  getRoomIcon(room: ChatRoom): string {
    switch (room.type) {
      case 'individual':
        return 'person';
      case 'group':
        return 'group';
      default:
        return 'chat';
    }
  }

  getRoomStatus(room: ChatRoom): string {
    if (room.type === 'individual') {
      const otherParticipant = room.participants?.find(p => p.id !== this.currentUserId);
      return otherParticipant?.isOnline ? 'online' : 'offline';
    }
    return 'group';
  }

  getLastMessagePreview(room: ChatRoom): string {
    if (room.lastMessage) {
      const content = room.lastMessage.content;
      return content.length > 30 ? content.substring(0, 30) + '...' : content;
    }
    return 'No messages yet';
  }

  formatLastMessageTime(room: ChatRoom): string {
    if (room.lastMessage) {
      const date = new Date(room.lastMessage.createdAt);
      const now = new Date();
      const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);

      if (diffInHours < 24) {
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      } else {
        return date.toLocaleDateString();
      }
    }
    return '';
  }

  // Utility methods
  canDeleteRoom(room: ChatRoom): boolean {
    // Only allow deletion for group rooms where user is admin
    return room.type === 'group' && room.isAdmin;
  }

  canMuteRoom(room: ChatRoom): boolean {
    return true; // All rooms can be muted
  }

  canPinRoom(room: ChatRoom): boolean {
    return true; // All rooms can be pinned
  }

  getUnreadCount(room: ChatRoom): number {
    return room.unreadCount || 0;
  }

  hasUnreadMessages(room: ChatRoom): boolean {
    return room.unreadCount > 0;
  }

  isRoomActive(room: ChatRoom): boolean {
    return room.isActive;
  }

  getRoomTypeLabel(room: ChatRoom): string {
    switch (room.type) {
      case 'individual':
        return 'Direct';
      case 'group':
        return 'Group';
      default:
        return 'Chat';
    }
  }

  getRoomTypeColor(room: ChatRoom): string {
    switch (room.type) {
      case 'individual':
        return 'primary';
      case 'group':
        return 'accent';
      default:
        return 'default';
    }
  }

  // Search and filter helpers
  clearSearch(): void {
    this.searchTerm = '';
  }

  resetFilters(): void {
    this.selectedFilter = 'all';
    this.showMutedRooms = true;
    this.showPinnedRooms = true;
  }

  getFilterIcon(filter: string): string {
    const filterObj = this.filters.find(f => f.value === filter);
    return filterObj?.icon || 'filter_list';
  }

  getFilterLabel(filter: string): string {
    const filterObj = this.filters.find(f => f.value === filter);
    return filterObj?.label || 'Filter';
  }
} 