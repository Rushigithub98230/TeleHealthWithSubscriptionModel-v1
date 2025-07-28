import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';

// SignalR import - will be installed via npm
declare var signalR: any;

export interface ChatMessage {
  id: string;
  chatRoomId: string;
  senderId: string;
  senderName: string;
  content: string;
  messageType: 'text' | 'file' | 'image' | 'audio' | 'video';
  filePath?: string;
  fileName?: string;
  fileSize?: number;
  replyToMessageId?: string;
  reactions: MessageReaction[];
  isRead: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface MessageReaction {
  id: string;
  emoji: string;
  userId: string;
  userName: string;
  createdAt: string;
}

export interface ChatRoom {
  id: string;
  name: string;
  type: 'individual' | 'group';
  participants: ChatParticipant[];
  lastMessage?: ChatMessage;
  unreadCount: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ChatParticipant {
  id: string;
  name: string;
  email: string;
  role: 'patient' | 'provider' | 'admin';
  isOnline: boolean;
  lastSeen?: string;
  avatar?: string;
}

export interface CreateMessageDto {
  chatRoomId: string;
  content: string;
  messageType: 'text' | 'file' | 'image' | 'audio' | 'video';
  filePath?: string;
  fileName?: string;
  fileSize?: number;
  replyToMessageId?: string;
}

export interface TypingIndicator {
  chatRoomId: string;
  userId: string;
  userName: string;
  isTyping: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private hubConnection?: any;
  private chatState$ = new BehaviorSubject<{
    isConnected: boolean;
    currentRoom?: ChatRoom;
    onlineUsers: string[];
  }>({
    isConnected: false,
    onlineUsers: []
  });

  private messages$ = new BehaviorSubject<ChatMessage[]>([]);
  private typingIndicators$ = new BehaviorSubject<TypingIndicator[]>([]);
  private newMessage$ = new Subject<ChatMessage>();
  private messageRead$ = new Subject<string>();
  private reactionAdded$ = new Subject<{ messageId: string; reaction: MessageReaction }>();
  private reactionRemoved$ = new Subject<{ messageId: string; userId: string; emoji: string }>();

  constructor(private http: HttpClient) {
    this.initializeSignalR();
  }

  private initializeSignalR(): void {
    if (typeof signalR !== 'undefined') {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${environment.apiUrl}/chatHub`, {
          accessTokenFactory: () => this.getAuthToken()
        })
        .withAutomaticReconnect()
        .build();

      this.setupSignalREventHandlers();
      this.connectToSignalR();
    }
  }

  private setupSignalREventHandlers(): void {
    if (!this.hubConnection) return;

    // Connection events
    this.hubConnection.onreconnecting(() => {
      this.updateChatState({ isConnected: false });
    });

    this.hubConnection.onreconnected(() => {
      this.updateChatState({ isConnected: true });
    });

    this.hubConnection.onclose(() => {
      this.updateChatState({ isConnected: false });
    });

    // Chat events
    this.hubConnection.on('JoinedChatRoom', (chatRoomId: string) => {
      console.log('Joined chat room:', chatRoomId);
    });

    this.hubConnection.on('UserJoined', (userId: string, userName: string) => {
      console.log('User joined:', userName);
      this.addOnlineUser(userId);
    });

    this.hubConnection.on('UserLeft', (userId: string, userName: string) => {
      console.log('User left:', userName);
      this.removeOnlineUser(userId);
    });

    this.hubConnection.on('MessageReceived', (message: ChatMessage) => {
      this.addMessage(message);
      this.newMessage$.next(message);
    });

    this.hubConnection.on('MessageSent', (messageId: string) => {
      console.log('Message sent:', messageId);
    });

    this.hubConnection.on('MessageFailed', (error: string) => {
      console.error('Message failed:', error);
    });

    this.hubConnection.on('MessageRead', (messageId: string, userId: string) => {
      this.markMessageAsRead(messageId);
      this.messageRead$.next(messageId);
    });

    this.hubConnection.on('ReactionAdded', (messageId: string, reaction: MessageReaction) => {
      this.addReactionToMessage(messageId, reaction);
      this.reactionAdded$.next({ messageId, reaction });
    });

    this.hubConnection.on('ReactionRemoved', (messageId: string, userId: string, emoji: string) => {
      this.removeReactionFromMessage(messageId, userId, emoji);
      this.reactionRemoved$.next({ messageId, userId, emoji });
    });

    this.hubConnection.on('TypingIndicator', (indicator: TypingIndicator) => {
      this.updateTypingIndicator(indicator);
    });

    this.hubConnection.on('OnlineUsers', (chatRoomId: string, userIds: string[]) => {
      this.updateOnlineUsers(userIds);
    });

    this.hubConnection.on('NotificationReceived', (notification: any) => {
      console.log('Notification received:', notification);
    });

    this.hubConnection.on('MessageEdited', (messageId: string, updatedMessage: ChatMessage) => {
      this.updateMessage(messageId, updatedMessage);
      // Optionally emit an event or update UI
    });
    this.hubConnection.on('MessageDeleted', (messageId: string, userId: string) => {
      this.markMessageAsDeleted(messageId, userId);
      // Optionally emit an event or update UI
    });
  }

  private async connectToSignalR(): Promise<void> {
    try {
      await this.hubConnection?.start();
      this.updateChatState({ isConnected: true });
      console.log('Connected to SignalR chat hub');
    } catch (error) {
      console.error('Error connecting to SignalR:', error);
      this.updateChatState({ isConnected: false });
    }
  }

  private getAuthToken(): string {
    return localStorage.getItem('authToken') || '';
  }

  private updateChatState(updates: Partial<{ isConnected: boolean; currentRoom?: ChatRoom; onlineUsers: string[] }>): void {
    const currentState = this.chatState$.value;
    this.chatState$.next({ ...currentState, ...updates });
  }

  private addOnlineUser(userId: string): void {
    const currentState = this.chatState$.value;
    if (!currentState.onlineUsers.includes(userId)) {
      this.updateChatState({ onlineUsers: [...currentState.onlineUsers, userId] });
    }
  }

  private removeOnlineUser(userId: string): void {
    const currentState = this.chatState$.value;
    this.updateChatState({ onlineUsers: currentState.onlineUsers.filter(id => id !== userId) });
  }

  private updateOnlineUsers(userIds: string[]): void {
    this.updateChatState({ onlineUsers: userIds });
  }

  private addMessage(message: ChatMessage): void {
    const currentMessages = this.messages$.value;
    this.messages$.next([...currentMessages, message]);
  }

  private markMessageAsRead(messageId: string): void {
    const currentMessages = this.messages$.value;
    const updatedMessages = currentMessages.map(msg => 
      msg.id === messageId ? { ...msg, isRead: true } : msg
    );
    this.messages$.next(updatedMessages);
  }

  private addReactionToMessage(messageId: string, reaction: MessageReaction): void {
    const currentMessages = this.messages$.value;
    const updatedMessages = currentMessages.map(msg => {
      if (msg.id === messageId) {
        const existingReaction = msg.reactions.find(r => r.userId === reaction.userId && r.emoji === reaction.emoji);
        if (!existingReaction) {
          return { ...msg, reactions: [...msg.reactions, reaction] };
        }
      }
      return msg;
    });
    this.messages$.next(updatedMessages);
  }

  private removeReactionFromMessage(messageId: string, userId: string, emoji: string): void {
    const currentMessages = this.messages$.value;
    const updatedMessages = currentMessages.map(msg => {
      if (msg.id === messageId) {
        return {
          ...msg,
          reactions: msg.reactions.filter(r => !(r.userId === userId && r.emoji === emoji))
        };
      }
      return msg;
    });
    this.messages$.next(updatedMessages);
  }

  private updateTypingIndicator(indicator: TypingIndicator): void {
    const currentIndicators = this.typingIndicators$.value;
    const existingIndex = currentIndicators.findIndex(i => 
      i.chatRoomId === indicator.chatRoomId && i.userId === indicator.userId
    );

    if (indicator.isTyping) {
      if (existingIndex >= 0) {
        currentIndicators[existingIndex] = indicator;
      } else {
        currentIndicators.push(indicator);
      }
    } else {
      if (existingIndex >= 0) {
        currentIndicators.splice(existingIndex, 1);
      }
    }

    this.typingIndicators$.next([...currentIndicators]);
  }

  private updateMessage(messageId: string, updatedMessage: ChatMessage): void {
    const messages = this.messages$.value;
    const idx = messages.findIndex(m => m.id === messageId);
    if (idx !== -1) {
      const merged = { ...messages[idx], ...updatedMessage };
      this.messages$.next([...messages.slice(0, idx), merged, ...messages.slice(idx + 1)]);
    }
  }

  private markMessageAsDeleted(messageId: string, userId: string): void {
    const messages = this.messages$.value;
    const idx = messages.findIndex(m => m.id === messageId);
    if (idx !== -1) {
      const deletedMsg = { ...messages[idx], content: '[Message deleted]', filePath: undefined, fileName: undefined };
      this.messages$.next([...messages.slice(0, idx), deletedMsg, ...messages.slice(idx + 1)]);
    }
  }

  // Public API methods
  getChatState(): Observable<{ isConnected: boolean; currentRoom?: ChatRoom; onlineUsers: string[] }> {
    return this.chatState$.asObservable();
  }

  getMessages(): Observable<ChatMessage[]> {
    return this.messages$.asObservable();
  }

  getTypingIndicators(): Observable<TypingIndicator[]> {
    return this.typingIndicators$.asObservable();
  }

  onNewMessage(): Observable<ChatMessage> {
    return this.newMessage$.asObservable();
  }

  onMessageRead(): Observable<string> {
    return this.messageRead$.asObservable();
  }

  onReactionAdded(): Observable<{ messageId: string; reaction: MessageReaction }> {
    return this.reactionAdded$.asObservable();
  }

  onReactionRemoved(): Observable<{ messageId: string; userId: string; emoji: string }> {
    return this.reactionRemoved$.asObservable();
  }

  // SignalR methods
  async joinChatRoom(chatRoomId: string): Promise<void> {
    try {
      await this.hubConnection?.invoke('JoinChatRoom', chatRoomId);
    } catch (error) {
      console.error('Error joining chat room:', error);
      throw error;
    }
  }

  async leaveChatRoom(chatRoomId: string): Promise<void> {
    try {
      await this.hubConnection?.invoke('LeaveChatRoom', chatRoomId);
    } catch (error) {
      console.error('Error leaving chat room:', error);
      throw error;
    }
  }

  async sendMessage(chatRoomId: string, content: string, messageType: 'text' | 'file' | 'image' | 'audio' | 'video' = 'text', filePath?: string, fileName?: string, fileSize?: number, replyToMessageId?: string): Promise<void> {
    try {
      await this.hubConnection?.invoke('SendMessage', chatRoomId, content, replyToMessageId, filePath);
    } catch (error) {
      console.error('Error sending message:', error);
      throw error;
    }
  }

  async sendTypingIndicator(chatRoomId: string, isTyping: boolean): Promise<void> {
    try {
      await this.hubConnection?.invoke('SendTypingIndicator', chatRoomId, isTyping);
    } catch (error) {
      console.error('Error sending typing indicator:', error);
      throw error;
    }
  }

  async markMessageAsRead(messageId: string): Promise<void> {
    try {
      await this.hubConnection?.invoke('MarkMessageAsRead', messageId);
    } catch (error) {
      console.error('Error marking message as read:', error);
      throw error;
    }
  }

  async addReaction(messageId: string, emoji: string): Promise<void> {
    try {
      await this.hubConnection?.invoke('AddReaction', messageId, emoji);
    } catch (error) {
      console.error('Error adding reaction:', error);
      throw error;
    }
  }

  async removeReaction(messageId: string, emoji: string): Promise<void> {
    try {
      await this.hubConnection?.invoke('RemoveReaction', messageId, emoji);
    } catch (error) {
      console.error('Error removing reaction:', error);
      throw error;
    }
  }

  async getOnlineUsers(chatRoomId: string): Promise<void> {
    try {
      await this.hubConnection?.invoke('GetOnlineUsers', chatRoomId);
    } catch (error) {
      console.error('Error getting online users:', error);
      throw error;
    }
  }

  async editMessage(messageId: string, newContent: string): Promise<void> {
    try {
      await this.hubConnection?.invoke('EditMessage', messageId, newContent);
    } catch (error) {
      console.error('Error editing message:', error);
      throw error;
    }
  }

  async deleteMessage(messageId: string): Promise<void> {
    try {
      await this.hubConnection?.invoke('DeleteMessage', messageId);
    } catch (error) {
      console.error('Error deleting message:', error);
      throw error;
    }
  }

  // HTTP API methods
  getChatRooms(): Observable<any> {
    return this.http.get(`${environment.apiUrl}/chat/rooms`);
  }

  getChatRoom(chatRoomId: string): Observable<any> {
    return this.http.get(`${environment.apiUrl}/chat/rooms/${chatRoomId}`);
  }

  getChatRoomMessages(chatRoomId: string, page: number = 0, pageSize: number = 50): Observable<any> {
    return this.http.get(`${environment.apiUrl}/chat/rooms/${chatRoomId}/messages?page=${page}&pageSize=${pageSize}`);
  }

  createChatRoom(createDto: any): Observable<any> {
    return this.http.post(`${environment.apiUrl}/chat/rooms`, createDto);
  }

  updateChatRoom(chatRoomId: string, updateDto: any): Observable<any> {
    return this.http.put(`${environment.apiUrl}/chat/rooms/${chatRoomId}`, updateDto);
  }

  deleteChatRoom(chatRoomId: string): Observable<any> {
    return this.http.delete(`${environment.apiUrl}/chat/rooms/${chatRoomId}`);
  }

  searchMessages(chatRoomId: string, searchTerm: string): Observable<any> {
    return this.http.get(`${environment.apiUrl}/chat/rooms/${chatRoomId}/messages/search?term=${searchTerm}`);
  }

  uploadFile(chatRoomId: string, file: File, fileCategory: string = 'chat-attachments'): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('chatRoomId', chatRoomId);
    formData.append('fileCategory', fileCategory);
    return this.http.post(`${environment.apiUrl}/chat/upload`, formData);
  }

  // Utility methods
  disconnect(): void {
    this.hubConnection?.stop();
  }

  isConnected(): boolean {
    return this.chatState$.value.isConnected;
  }

  getCurrentRoom(): ChatRoom | undefined {
    return this.chatState$.value.currentRoom;
  }

  getOnlineUsers(): string[] {
    return this.chatState$.value.onlineUsers;
  }

  clearMessages(): void {
    this.messages$.next([]);
  }

  setCurrentRoom(room: ChatRoom): void {
    this.updateChatState({ currentRoom: room });
  }
} 