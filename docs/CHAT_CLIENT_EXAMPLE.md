# Chat Client Implementation Guide

This document provides examples for implementing a JavaScript client to connect to the SignalR ChatHub for real-time chat functionality.

## Prerequisites

1. Install SignalR client library:
```bash
npm install @microsoft/signalr
```

## Basic Connection Setup

```javascript
import * as signalR from '@microsoft/signalr';

class ChatClient {
    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/chatHub', {
                accessTokenFactory: () => this.getAuthToken()
            })
            .withAutomaticReconnect()
            .build();
        
        this.setupEventHandlers();
    }

    async connect() {
        try {
            await this.connection.start();
            console.log('Connected to chat hub');
        } catch (err) {
            console.error('Error connecting to chat hub:', err);
        }
    }

    async disconnect() {
        try {
            await this.connection.stop();
            console.log('Disconnected from chat hub');
        } catch (err) {
            console.error('Error disconnecting from chat hub:', err);
        }
    }

    getAuthToken() {
        // Return your JWT token here
        return localStorage.getItem('authToken');
    }

    setupEventHandlers() {
        // Connection events
        this.connection.onreconnecting(() => {
            console.log('Reconnecting to chat hub...');
        });

        this.connection.onreconnected(() => {
            console.log('Reconnected to chat hub');
        });

        this.connection.onclose(() => {
            console.log('Disconnected from chat hub');
        });

        // Chat room events
        this.connection.on('JoinedChatRoom', (chatRoomId) => {
            console.log('Joined chat room:', chatRoomId);
            this.onJoinedChatRoom(chatRoomId);
        });

        this.connection.on('UserJoined', (userId, userName) => {
            console.log('User joined:', userName);
            this.onUserJoined(userId, userName);
        });

        this.connection.on('UserLeft', (userId, userName) => {
            console.log('User left:', userName);
            this.onUserLeft(userId, userName);
        });

        // Message events
        this.connection.on('MessageReceived', (message) => {
            console.log('Message received:', message);
            this.onMessageReceived(message);
        });

        this.connection.on('MessageSent', (messageId) => {
            console.log('Message sent:', messageId);
            this.onMessageSent(messageId);
        });

        this.connection.on('MessageFailed', (error) => {
            console.error('Message failed:', error);
            this.onMessageFailed(error);
        });

        // Typing indicator
        this.connection.on('TypingIndicator', (typingData) => {
            console.log('Typing indicator:', typingData);
            this.onTypingIndicator(typingData);
        });

        // Read receipts
        this.connection.on('MessageRead', (messageId, userId) => {
            console.log('Message read:', messageId, userId);
            this.onMessageRead(messageId, userId);
        });

        // Reactions
        this.connection.on('ReactionAdded', (messageId, reaction) => {
            console.log('Reaction added:', reaction);
            this.onReactionAdded(messageId, reaction);
        });

        this.connection.on('ReactionRemoved', (messageId, userId, emoji) => {
            console.log('Reaction removed:', messageId, userId, emoji);
            this.onReactionRemoved(messageId, userId, emoji);
        });

        // Notifications
        this.connection.on('NotificationReceived', (notification) => {
            console.log('Notification received:', notification);
            this.onNotificationReceived(notification);
        });

        // Online users
        this.connection.on('OnlineUsers', (chatRoomId, userIds) => {
            console.log('Online users:', userIds);
            this.onOnlineUsers(chatRoomId, userIds);
        });

        // Access denied
        this.connection.on('AccessDenied', (message) => {
            console.error('Access denied:', message);
            this.onAccessDenied(message);
        });
    }
}
```

## Chat Room Operations

```javascript
class ChatRoomManager {
    constructor(chatClient) {
        this.chatClient = chatClient;
    }

    async joinChatRoom(chatRoomId) {
        try {
            await this.chatClient.connection.invoke('JoinChatRoom', chatRoomId);
        } catch (err) {
            console.error('Error joining chat room:', err);
        }
    }

    async leaveChatRoom(chatRoomId) {
        try {
            await this.chatClient.connection.invoke('LeaveChatRoom', chatRoomId);
        } catch (err) {
            console.error('Error leaving chat room:', err);
        }
    }

    async getOnlineUsers(chatRoomId) {
        try {
            await this.chatClient.connection.invoke('GetOnlineUsers', chatRoomId);
        } catch (err) {
            console.error('Error getting online users:', err);
        }
    }
}
```

## Message Operations

```javascript
class MessageManager {
    constructor(chatClient) {
        this.chatClient = chatClient;
    }

    async sendMessage(chatRoomId, content, replyToMessageId = null, filePath = null) {
        try {
            await this.chatClient.connection.invoke('SendMessage', chatRoomId, content, replyToMessageId, filePath);
        } catch (err) {
            console.error('Error sending message:', err);
        }
    }

    async markMessageAsRead(messageId) {
        try {
            await this.chatClient.connection.invoke('MarkMessageAsRead', messageId);
        } catch (err) {
            console.error('Error marking message as read:', err);
        }
    }

    async addReaction(messageId, emoji) {
        try {
            await this.chatClient.connection.invoke('AddReaction', messageId, emoji);
        } catch (err) {
            console.error('Error adding reaction:', err);
        }
    }

    async removeReaction(messageId, emoji) {
        try {
            await this.chatClient.connection.invoke('RemoveReaction', messageId, emoji);
        } catch (err) {
            console.error('Error removing reaction:', err);
        }
    }
}
```

## Typing Indicator

```javascript
class TypingIndicator {
    constructor(chatClient) {
        this.chatClient = chatClient;
        this.typingTimeout = null;
    }

    async startTyping(chatRoomId) {
        try {
            await this.chatClient.connection.invoke('SendTypingIndicator', chatRoomId, true);
        } catch (err) {
            console.error('Error starting typing indicator:', err);
        }
    }

    async stopTyping(chatRoomId) {
        try {
            await this.chatClient.connection.invoke('SendTypingIndicator', chatRoomId, false);
        } catch (err) {
            console.error('Error stopping typing indicator:', err);
        }
    }

    // Auto-stop typing after user stops typing
    handleTyping(chatRoomId) {
        if (this.typingTimeout) {
            clearTimeout(this.typingTimeout);
        }

        this.startTyping(chatRoomId);

        this.typingTimeout = setTimeout(() => {
            this.stopTyping(chatRoomId);
        }, 3000); // Stop typing indicator after 3 seconds of inactivity
    }
}
```

## Notification System

```javascript
class NotificationManager {
    constructor(chatClient) {
        this.chatClient = chatClient;
    }

    async sendNotification(userId, title, message, chatRoomId = null) {
        try {
            await this.chatClient.connection.invoke('SendNotification', userId, title, message, chatRoomId);
        } catch (err) {
            console.error('Error sending notification:', err);
        }
    }

    async sendNotificationToChatRoom(chatRoomId, title, message) {
        try {
            await this.chatClient.connection.invoke('SendNotificationToChatRoom', chatRoomId, title, message);
        } catch (err) {
            console.error('Error sending notification to chat room:', err);
        }
    }

    // Handle incoming notifications
    onNotificationReceived(notification) {
        // Show browser notification
        if (Notification.permission === 'granted') {
            new Notification(notification.title, {
                body: notification.message,
                icon: '/path/to/icon.png'
            });
        }

        // Show in-app notification
        this.showInAppNotification(notification);
    }

    showInAppNotification(notification) {
        // Create notification element
        const notificationEl = document.createElement('div');
        notificationEl.className = 'notification';
        notificationEl.innerHTML = `
            <h4>${notification.title}</h4>
            <p>${notification.message}</p>
            <small>${new Date(notification.timestamp).toLocaleTimeString()}</small>
        `;

        // Add to notification container
        const container = document.getElementById('notification-container');
        container.appendChild(notificationEl);

        // Auto-remove after 5 seconds
        setTimeout(() => {
            notificationEl.remove();
        }, 5000);
    }
}
```

## Complete Usage Example

```javascript
// Initialize chat client
const chatClient = new ChatClient();
const chatRoomManager = new ChatRoomManager(chatClient);
const messageManager = new MessageManager(chatClient);
const typingIndicator = new TypingIndicator(chatClient);
const notificationManager = new NotificationManager(chatClient);

// Connect to hub
await chatClient.connect();

// Join a chat room
await chatRoomManager.joinChatRoom('chat-room-id');

// Send a message
await messageManager.sendMessage('chat-room-id', 'Hello, world!');

// Handle typing
const messageInput = document.getElementById('message-input');
messageInput.addEventListener('input', () => {
    typingIndicator.handleTyping('chat-room-id');
});

// Event handlers
chatClient.onMessageReceived = (message) => {
    // Add message to UI
    addMessageToUI(message);
};

chatClient.onTypingIndicator = (typingData) => {
    // Show typing indicator
    showTypingIndicator(typingData);
};

chatClient.onNotificationReceived = (notification) => {
    notificationManager.onNotificationReceived(notification);
};

// UI functions
function addMessageToUI(message) {
    const messageContainer = document.getElementById('messages');
    const messageEl = document.createElement('div');
    messageEl.className = 'message';
    messageEl.innerHTML = `
        <div class="message-header">
            <strong>${message.senderName}</strong>
            <small>${new Date(message.createdAt).toLocaleTimeString()}</small>
        </div>
        <div class="message-content">${message.content}</div>
    `;
    messageContainer.appendChild(messageEl);
    messageContainer.scrollTop = messageContainer.scrollHeight;
}

function showTypingIndicator(typingData) {
    const typingEl = document.getElementById('typing-indicator');
    if (typingData.isTyping) {
        typingEl.textContent = `${typingData.userName} is typing...`;
        typingEl.style.display = 'block';
    } else {
        typingEl.style.display = 'none';
    }
}
```

## HTML Structure

```html
<!DOCTYPE html>
<html>
<head>
    <title>Chat Application</title>
</head>
<body>
    <div id="chat-container">
        <div id="chat-rooms">
            <h3>Chat Rooms</h3>
            <div id="room-list"></div>
        </div>
        
        <div id="chat-main">
            <div id="chat-header">
                <h2 id="room-name">Select a chat room</h2>
                <div id="online-users"></div>
            </div>
            
            <div id="messages"></div>
            
            <div id="typing-indicator" style="display: none;"></div>
            
            <div id="message-input-container">
                <input type="text" id="message-input" placeholder="Type your message...">
                <button id="send-button">Send</button>
                <button id="attach-button">📎</button>
            </div>
        </div>
    </div>
    
    <div id="notification-container"></div>
    
    <script type="module" src="chat-client.js"></script>
</body>
</html>
```

## CSS Styling

```css
#chat-container {
    display: flex;
    height: 100vh;
}

#chat-rooms {
    width: 250px;
    border-right: 1px solid #ccc;
    padding: 10px;
}

#chat-main {
    flex: 1;
    display: flex;
    flex-direction: column;
}

#messages {
    flex: 1;
    overflow-y: auto;
    padding: 10px;
}

.message {
    margin-bottom: 10px;
    padding: 10px;
    border-radius: 5px;
    background-color: #f5f5f5;
}

.message-header {
    display: flex;
    justify-content: space-between;
    margin-bottom: 5px;
}

#message-input-container {
    padding: 10px;
    border-top: 1px solid #ccc;
    display: flex;
    gap: 10px;
}

#message-input {
    flex: 1;
    padding: 8px;
    border: 1px solid #ccc;
    border-radius: 4px;
}

#typing-indicator {
    padding: 5px 10px;
    font-style: italic;
    color: #666;
}

.notification {
    position: fixed;
    top: 20px;
    right: 20px;
    background: #333;
    color: white;
    padding: 15px;
    border-radius: 5px;
    z-index: 1000;
    max-width: 300px;
}
```

This implementation provides a complete real-time chat system with:

- Real-time message sending and receiving
- Typing indicators
- Read receipts
- Message reactions
- Push notifications
- Online user tracking
- File attachments
- Message threading
- HIPAA-compliant storage with switchable local/cloud storage

The system supports both local storage (for development/testing) and cloud storage (Azure Blob Storage) for production use, with easy switching between the two via configuration. 