import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';

import { ChatMessage, MessageReaction } from '../../../../core/services/chat.service';

@Component({
  selector: 'app-chat-message',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatTooltipModule,
    MatChipsModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './chat-message.component.html',
  styleUrl: './chat-message.component.scss'
})
export class ChatMessageComponent implements OnInit {
  @Input() message!: ChatMessage;
  @Input() isOwnMessage = false;
  @Input() showAvatar = true;
  @Input() showTimestamp = true;
  @Input() showReactions = true;
  @Input() showMenu = true;
  @Input() currentUserId = '';

  @Output() messageReaction = new EventEmitter<{ messageId: string; emoji: string }>();
  @Output() messageEdit = new EventEmitter<ChatMessage>();
  @Output() messageDelete = new EventEmitter<string>();
  @Output() messageReply = new EventEmitter<ChatMessage>();
  @Output() fileDownload = new EventEmitter<{ filePath: string; fileName: string }>();

  // Message state
  isEditing = false;
  editText = '';
  showReactionPicker = false;

  // Common reactions
  commonReactions = ['👍', '❤️', '😊', '😮', '😢', '😡', '👏', '🙏'];

  constructor(
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.editText = this.message.content;
  }

  // Message type checks
  isTextMessage(): boolean {
    return this.message.messageType === 'text';
  }

  isFileMessage(): boolean {
    return this.message.messageType === 'file';
  }

  isImageMessage(): boolean {
    return this.message.messageType === 'image';
  }

  isAudioMessage(): boolean {
    return this.message.messageType === 'audio';
  }

  isVideoMessage(): boolean {
    return this.message.messageType === 'video';
  }

  // File handling
  getFileIcon(): string {
    const extension = this.getFileExtension();
    switch (extension.toLowerCase()) {
      case 'pdf': return 'picture_as_pdf';
      case 'doc':
      case 'docx': return 'description';
      case 'xls':
      case 'xlsx': return 'table_chart';
      case 'ppt':
      case 'pptx': return 'slideshow';
      case 'jpg':
      case 'jpeg':
      case 'png':
      case 'gif': return 'image';
      case 'mp3':
      case 'wav': return 'audiotrack';
      case 'mp4':
      case 'avi': return 'videocam';
      default: return 'insert_drive_file';
    }
  }

  getFileExtension(): string {
    if (!this.message.fileName) return '';
    return this.message.fileName.split('.').pop() || '';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  downloadFile(): void {
    if (this.message.filePath) {
      this.fileDownload.emit({
        filePath: this.message.filePath,
        fileName: this.message.fileName || 'download'
      });
    }
  }

  // Reactions
  addReaction(emoji: string): void {
    this.messageReaction.emit({
      messageId: this.message.id,
      emoji: emoji
    });
    this.showReactionPicker = false;
  }

  hasReaction(emoji: string): boolean {
    return this.message.reactions.some((reaction: MessageReaction) => reaction.emoji === emoji);
  }

  getReactionCount(emoji: string): number {
    return this.message.reactions.filter((reaction: MessageReaction) => reaction.emoji === emoji).length;
  }

  getReactionUsers(emoji: string): string {
    const users = this.message.reactions
      .filter((reaction: MessageReaction) => reaction.emoji === emoji)
      .map((reaction: MessageReaction) => reaction.userName);
    return users.join(', ');
  }

  // Message actions
  startEdit(): void {
    this.isEditing = true;
    this.editText = this.message.content;
  }

  saveEdit(): void {
    if (this.editText.trim() && this.editText !== this.message.content) {
      const updatedMessage = { ...this.message, content: this.editText.trim() };
      this.messageEdit.emit(updatedMessage);
    }
    this.isEditing = false;
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.editText = this.message.content;
  }

  deleteMessage(): void {
    this.messageDelete.emit(this.message.id);
  }

  replyToMessage(): void {
    this.messageReply.emit(this.message);
  }

  // Time formatting
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

  // Menu actions
  copyMessage(): void {
    navigator.clipboard.writeText(this.message.content).then(() => {
      this.snackBar.open('Message copied to clipboard', 'Close', { duration: 2000 });
    });
  }

  reportMessage(): void {
    this.snackBar.open('Report feature coming soon', 'Close', { duration: 2000 });
  }

  // Image handling
  openImageDialog(): void {
    if (this.isImageMessage() && this.message.content) {
      // TODO: Implement image dialog
      this.snackBar.open('Image viewer coming soon', 'Close', { duration: 2000 });
    }
  }

  // Audio/Video handling
  playMedia(): void {
    if (this.isAudioMessage() || this.isVideoMessage()) {
      // TODO: Implement media player
      this.snackBar.open('Media player coming soon', 'Close', { duration: 2000 });
    }
  }

  // Utility methods
  canEdit(): boolean {
    return this.isOwnMessage && this.isTextMessage() && !this.message.isRead;
  }

  canDelete(): boolean {
    return this.isOwnMessage;
  }

  isMessageRead(): boolean {
    return this.message.isRead;
  }

  getMessageStatus(): string {
    if (this.isOwnMessage) {
      if (this.message.isRead) {
        return 'read';
      } else {
        return 'sent';
      }
    }
    return '';
  }
} 