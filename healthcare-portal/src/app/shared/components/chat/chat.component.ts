import { Component, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';

import { ChatService, ChatMessage, ChatRoom, ChatParticipant, TypingIndicator } from '../../../core/services/chat.service';
import { ChatMessageComponent } from './chat-message/chat-message.component';
import { ChatInputComponent } from './chat-input/chat-input.component';

@Component({
  selector: 'app-chat',
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
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    ChatMessageComponent,
    ChatInputComponent
  ],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss'
})
export class ChatComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('messageContainer') messageContainer!: ElementRef;
  @ViewChild('messageInput') messageInput!: ElementRef;

  // Chat state
  currentRoom: ChatRoom | null = null;
  messages: ChatMessage[] = [];
  rooms: ChatRoom[] = [];
  participants: ChatParticipant[] = [];
  typingUsers: TypingIndicator[] = [];

  // UI state
  loading = false;
  loadingMessages = false;
  loadingRooms = false;
  showRoomList = true;
  showParticipants = false;
  isTyping = false;
  messageText = '';
  searchTerm = '';

  // Connection state
  isConnected = false;
  connectionStatus = 'Disconnected';

  // Pagination
  currentPage = 1;
  pageSize = 50;
  hasMoreMessages = true;

  constructor(
    private chatService: ChatService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.initializeChat();
    this.subscribeToChatEvents();
  }

  ngAfterViewInit(): void {
    this.scrollToBottom();
  }

  ngOnDestroy(): void {
    this.chatService.disconnect();
  }

  private async initializeChat(): Promise<void> {
    try {
      this.loading = true;
      
      // Connect to SignalR
      await this.chatService.joinChatRoom('default');
      this.isConnected = true;
      this.connectionStatus = 'Connected';

      // Load chat rooms
      await this.loadChatRooms();

      // Auto-select first room if available
      if (this.rooms.length > 0 && !this.currentRoom) {
        await this.selectRoom(this.rooms[0]);
      }

    } catch (error: any) {
      console.error('Failed to initialize chat:', error);
      this.snackBar.open('Failed to connect to chat service', 'Close', { duration: 3000 });
      this.connectionStatus = 'Connection Failed';
    } finally {
      this.loading = false;
    }
  }

  private subscribeToChatEvents(): void {
    // Subscribe to new messages
    this.chatService.getMessages().subscribe(messages => {
      this.messages = messages;
      this.scrollToBottom();
    });

    // Subscribe to typing indicators
    this.chatService.getTypingIndicators().subscribe(typing => {
      this.typingUsers = typing;
    });

    // Subscribe to connection status
    this.chatService.getChatState().subscribe(state => {
      this.isConnected = state.isConnected;
      this.connectionStatus = state.isConnected ? 'Connected' : 'Disconnected';
    });

    // Subscribe to room updates
    this.chatService.getChatRooms().subscribe((response: any) => {
      if (response?.success) {
        this.rooms = response.data || [];
      }
    });
  }

  async loadChatRooms(): Promise<void> {
    try {
      this.loadingRooms = true;
      const response = await this.chatService.getChatRooms().toPromise();
      
      if (response?.success) {
        this.rooms = response.data || [];
      } else {
        throw new Error(response?.message || 'Failed to load chat rooms');
      }
    } catch (error: any) {
      console.error('Failed to load chat rooms:', error);
      this.snackBar.open('Failed to load chat rooms', 'Close', { duration: 3000 });
    } finally {
      this.loadingRooms = false;
    }
  }

  async selectRoom(room: ChatRoom): Promise<void> {
    try {
      this.currentRoom = room;
      this.loadingMessages = true;
      this.messages = [];
      this.currentPage = 1;
      this.hasMoreMessages = true;

      // Join the chat room
      await this.chatService.joinChatRoom(room.id);

      // Load messages
      await this.loadMessages();

      // Load participants
      await this.loadParticipants();

      // Update UI
      this.showRoomList = false;

    } catch (error: any) {
      console.error('Failed to select room:', error);
      this.snackBar.open('Failed to join chat room', 'Close', { duration: 3000 });
    } finally {
      this.loadingMessages = false;
    }
  }

  async loadMessages(): Promise<void> {
    if (!this.currentRoom) return;

    try {
      const response = await this.chatService.getChatRoomMessages(this.currentRoom.id, this.currentPage, this.pageSize).toPromise();
      
      if (response?.success) {
        const newMessages = response.data || [];
        this.messages = [...newMessages, ...this.messages];
        this.hasMoreMessages = newMessages.length === this.pageSize;
      } else {
        throw new Error(response?.message || 'Failed to load messages');
      }
    } catch (error: any) {
      console.error('Failed to load messages:', error);
      this.snackBar.open('Failed to load messages', 'Close', { duration: 3000 });
    }
  }

  async loadParticipants(): Promise<void> {
    if (!this.currentRoom) return;

    try {
      // For now, we'll get participants from the room object
      this.participants = this.currentRoom.participants || [];
    } catch (error: any) {
      console.error('Failed to load participants:', error);
      this.snackBar.open('Failed to load participants', 'Close', { duration: 3000 });
    }
  }

  async sendMessage(): Promise<void> {
    if (!this.currentRoom || !this.messageText.trim()) return;

    try {
      await this.chatService.sendMessage(
        this.currentRoom.id,
        this.messageText.trim(),
        'text'
      );
      
      this.messageText = '';
      this.isTyping = false;
    } catch (error: any) {
      console.error('Failed to send message:', error);
      this.snackBar.open('Failed to send message', 'Close', { duration: 3000 });
    }
  }

  async sendFile(file: File): Promise<void> {
    if (!this.currentRoom) return;

    try {
      const response = await this.chatService.uploadFile(this.currentRoom.id, file).toPromise();
      
      if (response?.success) {
        await this.chatService.sendMessage(
          this.currentRoom.id,
          response.data,
          'file',
          response.data,
          file.name,
          file.size
        );
      } else {
        throw new Error(response?.message || 'Failed to upload file');
      }
    } catch (error: any) {
      console.error('Failed to send file:', error);
      this.snackBar.open('Failed to send file', 'Close', { duration: 3000 });
    }
  }

  onTyping(): void {
    if (!this.currentRoom) return;

    if (!this.isTyping) {
      this.isTyping = true;
      this.chatService.sendTypingIndicator(this.currentRoom.id, true);
    }

    // Clear typing indicator after 3 seconds
    setTimeout(() => {
      this.isTyping = false;
      this.chatService.sendTypingIndicator(this.currentRoom!.id, false);
    }, 3000);
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.sendFile(file);
    }
  }

  loadMoreMessages(): void {
    if (this.hasMoreMessages && !this.loadingMessages) {
      this.currentPage++;
      this.loadMessages();
    }
  }

  scrollToBottom(): void {
    setTimeout(() => {
      if (this.messageContainer) {
        this.messageContainer.nativeElement.scrollTop = this.messageContainer.nativeElement.scrollHeight;
      }
    }, 100);
  }

  toggleRoomList(): void {
    this.showRoomList = !this.showRoomList;
  }

  toggleParticipants(): void {
    this.showParticipants = !this.showParticipants;
  }

  backToRooms(): void {
    this.currentRoom = null;
    this.messages = [];
    this.participants = [];
    this.showRoomList = true;
    this.showParticipants = false;
  }

  getRoomDisplayName(room: ChatRoom): string {
    if (room.type === 'individual') {
      return room.participants?.find(p => p.id !== this.getCurrentUserId())?.name || room.name;
    }
    return room.name;
  }

  getCurrentUserId(): string {
    // TODO: Get from auth service
    return '1';
  }

  isCurrentUser(message: ChatMessage): boolean {
    return message.senderId === this.getCurrentUserId();
  }

  formatMessageTime(timestamp: string): string {
    const date = new Date(timestamp);
    const now = new Date();
    const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);

    if (diffInHours < 24) {
      return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } else {
      return date.toLocaleDateString();
    }
  }

  getTypingText(): string {
    if (this.typingUsers.length === 0) return '';
    if (this.typingUsers.length === 1) {
      return `${this.typingUsers[0].userName} is typing...`;
    }
    return 'Multiple people are typing...';
  }

  canSendMessage(): boolean {
    return this.isConnected && this.currentRoom !== null && this.messageText.trim().length > 0;
  }

  // Message interaction handlers
  onMessageReaction(event: { messageId: string; emoji: string }): void {
    this.chatService.addReaction(event.messageId, event.emoji).then(() => {
      this.snackBar.open('Reaction added', 'Close', { duration: 2000 });
    }).catch((error: any) => {
      console.error('Failed to add reaction:', error);
      this.snackBar.open('Failed to add reaction', 'Close', { duration: 3000 });
    });
  }

  onMessageEdit(updatedMessage: ChatMessage): void {
    // TODO: Implement message editing API call
    this.snackBar.open('Message editing coming soon', 'Close', { duration: 2000 });
  }

  onMessageDelete(messageId: string): void {
    // TODO: Implement message deletion API call
    this.snackBar.open('Message deletion coming soon', 'Close', { duration: 2000 });
  }

  onMessageReply(message: ChatMessage): void {
    // TODO: Implement message reply functionality
    this.snackBar.open('Reply feature coming soon', 'Close', { duration: 2000 });
  }

  onFileDownload(event: { filePath: string; fileName: string }): void {
    // TODO: Implement file download functionality
    this.snackBar.open('File download coming soon', 'Close', { duration: 2000 });
  }

  // Voice message handler
  onVoiceMessage(): void {
    // TODO: Implement voice message functionality
    this.snackBar.open('Voice message feature coming soon', 'Close', { duration: 2000 });
  }
} 