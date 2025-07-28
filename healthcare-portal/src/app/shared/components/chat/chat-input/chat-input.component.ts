import { Component, Input, Output, EventEmitter, ViewChild, ElementRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'app-chat-input',
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
    MatMenuModule,
    MatTooltipModule,
    MatChipsModule,
    MatSnackBarModule,
    MatDialogModule,
    MatProgressBarModule
  ],
  templateUrl: './chat-input.component.html',
  styleUrl: './chat-input.component.scss'
})
export class ChatInputComponent implements OnInit {
  @ViewChild('messageInput') messageInput!: ElementRef;
  @ViewChild('fileInput') fileInput!: ElementRef;

  @Input() placeholder = 'Type a message...';
  @Input() disabled = false;
  @Input() maxLength = 1000;
  @Input() allowedFileTypes = ['image/*', 'application/pdf', 'text/*'];
  @Input() maxFileSize = 10 * 1024 * 1024; // 10MB
  @Input() showEmojiPicker = true;
  @Input() showFileUpload = true;
  @Input() showVoiceMessage = true;

  @Output() messageSent = new EventEmitter<string>();
  @Output() fileSelected = new EventEmitter<File>();
  @Output() voiceMessage = new EventEmitter<void>();
  @Output() typing = new EventEmitter<boolean>();

  // Input state
  messageText = '';
  isTyping = false;
  isRecording = false;
  isUploading = false;
  uploadProgress = 0;

  // File upload state
  selectedFiles: File[] = [];
  dragOver = false;

  // Emoji picker state
  showEmojiPanel = false;
  emojiCategories = [
    { name: 'Recent', icon: '🕒', emojis: ['👍', '❤️', '😊', '😮', '😢', '😡', '👏', '🙏'] },
    { name: 'Smileys', icon: '😊', emojis: ['😀', '😃', '😄', '😁', '😆', '😅', '😂', '🤣', '😊', '😇', '🙂', '🙃', '😉', '😌', '😍', '🥰', '😘', '😗', '😙', '😚', '😋', '😛', '😝', '😜', '🤪', '🤨', '🧐', '🤓', '😎', '🤩', '🥳', '😏', '😒', '😞', '😔', '😟', '😕', '🙁', '☹️', '😣', '😖', '😫', '😩', '🥺', '😢', '😭', '😤', '😠', '😡', '🤬', '🤯', '😳', '🥵', '🥶', '😱', '😨', '😰', '😥', '😓', '🤗', '🤔', '🤭', '🤫', '🤥', '😶', '😐', '😑', '😯', '😦', '😧', '😮', '😲', '🥱', '😴', '🤤', '😪', '😵', '🤐', '🥴', '🤢', '🤮', '🤧', '😷', '🤒', '🤕'] },
    { name: 'Gestures', icon: '👋', emojis: ['👋', '🤚', '🖐️', '✋', '🖖', '👌', '🤌', '🤏', '✌️', '🤞', '🤟', '🤘', '🤙', '👈', '👉', '👆', '🖕', '👇', '☝️', '👍', '👎', '👊', '✊', '🤛', '🤜', '👏', '🙌', '👐', '🤲', '🤝', '🙏', '✍️', '💪', '🦾', '🦿', '🦵', '🦶', '👂', '🦻', '👃', '🧠', '🫀', '🫁', '🦷', '🦴', '👀', '👁️', '👅', '👄', '💋', '🩸'] },
    { name: 'Objects', icon: '💡', emojis: ['💡', '🔦', '🕯️', '🪔', '🧭', '🕰️', '⏰', '⏲️', '⏱️', '🕛', '🕧', '🕐', '🕜', '🕑', '🕝', '🕒', '🕞', '🕓', '🕟', '🕔', '🕠', '🕕', '🕡', '🕖', '🕢', '🕗', '🕣', '🕘', '🕤', '🕙', '🕥', '🕚', '🕦', '🕙', '🕥', '🕚', '🕦', '🕛', '🕧', '🕐', '🕜', '🕑', '🕝', '🕒', '🕞', '🕓', '🕟', '🕔', '🕠', '🕕', '🕡', '🕖', '🕢', '🕗', '🕣', '🕘', '🕤', '🕙', '🕥', '🕚', '🕦', '🕛', '🕧'] }
  ];

  // Voice recording state
  recordingTime = 0;
  recordingInterval?: any;

  constructor(
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.setupTypingDetection();
  }

  setupTypingDetection(): void {
    // Debounce typing events
    let typingTimeout: any;
    
    const emitTyping = (isTyping: boolean) => {
      if (this.isTyping !== isTyping) {
        this.isTyping = isTyping;
        this.typing.emit(isTyping);
      }
    };

    // Clear typing indicator after 3 seconds of no input
    const clearTyping = () => {
      clearTimeout(typingTimeout);
      typingTimeout = setTimeout(() => {
        emitTyping(false);
      }, 3000);
    };

    // Listen for input changes
    this.messageText = '';
  }

  onInputChange(): void {
    if (this.messageText.trim()) {
      this.typing.emit(true);
      // Clear typing indicator after 3 seconds
      setTimeout(() => {
        this.typing.emit(false);
      }, 3000);
    } else {
      this.typing.emit(false);
    }
  }

  sendMessage(): void {
    if (this.messageText.trim() && !this.disabled) {
      this.messageSent.emit(this.messageText.trim());
      this.messageText = '';
      this.typing.emit(false);
      this.focusInput();
    }
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  focusInput(): void {
    setTimeout(() => {
      this.messageInput?.nativeElement?.focus();
    }, 100);
  }

  // File upload methods
  onFileSelected(event: any): void {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.handleFiles(Array.from(files));
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
    
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFiles(Array.from(files));
    }
  }

  handleFiles(files: File[]): void {
    for (const file of files) {
      if (this.validateFile(file)) {
        this.selectedFiles.push(file);
        this.fileSelected.emit(file);
      }
    }
  }

  validateFile(file: File): boolean {
    // Check file size
    if (file.size > this.maxFileSize) {
      this.snackBar.open(`File ${file.name} is too large. Maximum size is ${this.formatFileSize(this.maxFileSize)}`, 'Close', { duration: 3000 });
      return false;
    }

    // Check file type
    const isValidType = this.allowedFileTypes.some(type => {
      if (type.endsWith('/*')) {
        const category = type.replace('/*', '');
        return file.type.startsWith(category);
      }
      return file.type === type;
    });

    if (!isValidType) {
      this.snackBar.open(`File type ${file.type} is not allowed`, 'Close', { duration: 3000 });
      return false;
    }

    return true;
  }

  removeFile(file: File): void {
    const index = this.selectedFiles.indexOf(file);
    if (index > -1) {
      this.selectedFiles.splice(index, 1);
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  // Emoji picker methods
  toggleEmojiPicker(): void {
    this.showEmojiPanel = !this.showEmojiPanel;
  }

  addEmoji(emoji: string): void {
    this.messageText += emoji;
    this.focusInput();
    this.showEmojiPanel = false;
  }

  // Voice recording methods
  startVoiceRecording(): void {
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
      navigator.mediaDevices.getUserMedia({ audio: true })
        .then(stream => {
          this.isRecording = true;
          this.recordingTime = 0;
          this.startRecordingTimer();
          this.voiceMessage.emit();
        })
        .catch(error => {
          console.error('Error accessing microphone:', error);
          this.snackBar.open('Unable to access microphone', 'Close', { duration: 3000 });
        });
    } else {
      this.snackBar.open('Voice recording not supported in this browser', 'Close', { duration: 3000 });
    }
  }

  stopVoiceRecording(): void {
    this.isRecording = false;
    this.stopRecordingTimer();
    this.recordingTime = 0;
  }

  startRecordingTimer(): void {
    this.recordingInterval = setInterval(() => {
      this.recordingTime++;
    }, 1000);
  }

  stopRecordingTimer(): void {
    if (this.recordingInterval) {
      clearInterval(this.recordingInterval);
      this.recordingInterval = undefined;
    }
  }

  formatRecordingTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }

  // Utility methods
  canSendMessage(): boolean {
    return !this.disabled && (this.messageText.trim().length > 0 || this.selectedFiles.length > 0);
  }

  getCharacterCount(): number {
    return this.messageText.length;
  }

  getRemainingCharacters(): number {
    return this.maxLength - this.messageText.length;
  }

  isNearLimit(): boolean {
    return this.messageText.length > this.maxLength * 0.8;
  }

  isOverLimit(): boolean {
    return this.messageText.length > this.maxLength;
  }
} 