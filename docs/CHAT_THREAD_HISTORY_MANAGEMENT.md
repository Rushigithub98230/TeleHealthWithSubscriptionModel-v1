# Chat Thread and History Management

## Overview

The telehealth platform implements a comprehensive chat system that maintains persistent threads and complete chat history through a multi-layered architecture combining database storage, real-time SignalR communication, and file storage for attachments.

## Database Architecture

### Core Entities

#### 1. ChatRoom (Thread Management)
```csharp
public class ChatRoom : BaseEntity
{
    public enum ChatRoomType
    {
        PatientProvider,  // One-on-one patient-provider chat
        Group,           // Multi-participant group chat
        Support,         // Support team chat
        Consultation     // Consultation-specific chat
    }

    public enum ChatRoomStatus
    {
        Active,         // Currently active
        Paused,         // Temporarily paused
        Archived,       // Archived but accessible
        Deleted         // Soft deleted
    }

    // Thread identification
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ChatRoomType Type { get; set; }
    public ChatRoomStatus Status { get; set; }

    // Thread relationships
    public Guid? PatientId { get; set; }
    public Guid? ProviderId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public Guid? ConsultationId { get; set; }

    // Thread metadata
    public DateTime? LastActivityAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public bool IsEncrypted { get; set; } = true;
    public string? EncryptionKey { get; set; }

    // Navigation properties
    public virtual ICollection<Message> Messages { get; set; }
    public virtual ICollection<ChatRoomParticipant> Participants { get; set; }
}
```

#### 2. Message (History Storage)
```csharp
public class Message : BaseEntity
{
    public enum MessageType
    {
        Text,       // Plain text messages
        Image,      // Image attachments
        Video,      // Video attachments
        Document,   // Document attachments
        Audio,      // Audio messages
        System      // System notifications
    }

    public enum MessageStatus
    {
        Sent,       // Message sent
        Delivered,  // Message delivered to recipient
        Read,       // Message read by recipient
        Failed      // Message delivery failed
    }

    // Thread association
    public Guid ChatRoomId { get; set; }
    public virtual ChatRoom ChatRoom { get; set; }

    // Message threading (reply functionality)
    public Guid? ReplyToMessageId { get; set; }
    public virtual Message? ReplyToMessage { get; set; }
    public virtual ICollection<Message> Replies { get; set; }

    // Message content
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public MessageStatus Status { get; set; }

    // File attachments
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }

    // Message metadata
    public DateTime? ReadAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool IsEncrypted { get; set; } = true;
    public string? EncryptionKey { get; set; }

    // Navigation properties
    public virtual ICollection<MessageReaction> Reactions { get; set; }
    public virtual ICollection<MessageReadReceipt> ReadReceipts { get; set; }
    public virtual ICollection<MessageAttachment> Attachments { get; set; }
}
```

#### 3. ChatRoomParticipant (Thread Access Control)
```csharp
public class ChatRoomParticipant : BaseEntity
{
    public enum ParticipantRole
    {
        Patient,    // Patient in consultation
        Provider,   // Healthcare provider
        Admin,      // Chat room administrator
        Moderator,  // Chat room moderator
        Observer    // Read-only participant
    }

    public enum ParticipantStatus
    {
        Active,     // Active participant
        Muted,      // Temporarily muted
        Banned,     // Banned from chat
        Left        // Left the chat
    }

    // Thread association
    public Guid ChatRoomId { get; set; }
    public virtual ChatRoom ChatRoom { get; set; }

    // Participant details
    public Guid UserId { get; set; }
    public virtual User User { get; set; }
    public ParticipantRole Role { get; set; }
    public ParticipantStatus Status { get; set; }

    // Activity tracking
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public DateTime? LastSeenAt { get; set; }

    // Permissions
    public bool CanSendMessages { get; set; } = true;
    public bool CanSendFiles { get; set; } = true;
    public bool CanInviteOthers { get; set; } = false;
    public bool CanModerate { get; set; } = false;

    // Muting
    public DateTime? MutedUntil { get; set; }
    public string? MuteReason { get; set; }
}
```

#### 4. MessageReadReceipt (Read Status Tracking)
```csharp
public class MessageReadReceipt : BaseEntity
{
    public Guid MessageId { get; set; }
    public virtual Message Message { get; set; }

    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}
```

#### 5. MessageReaction (Message Interactions)
```csharp
public class MessageReaction : BaseEntity
{
    public Guid MessageId { get; set; }
    public virtual Message Message { get; set; }

    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    public string Emoji { get; set; } = string.Empty;
}
```

## Thread Management

### 1. Thread Creation

#### Patient-Provider Threads
```csharp
public async Task<ApiResponse<ChatRoomDto>> CreatePatientProviderChatRoomAsync(
    Guid patientId, 
    Guid providerId, 
    Guid? subscriptionId = null)
{
    // Validate participants exist
    var patient = await _userRepository.GetByIdAsync(patientId);
    var provider = await _userRepository.GetByIdAsync(providerId);

    // Check subscription status if provided
    if (subscriptionId.HasValue)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId.Value);
        if (subscription?.Status != Subscription.SubscriptionStatus.Active)
        {
            return ApiResponse<ChatRoomDto>.ErrorResponse("Active subscription required", 403);
        }
    }

    // Create thread with metadata
    var createDto = new CreateChatRoomDto
    {
        Name = $"Chat: {patient.FirstName} {patient.LastName} - {provider.FirstName} {provider.LastName}",
        Description = "Patient-Provider consultation chat",
        Type = ChatRoom.ChatRoomType.PatientProvider,
        PatientId = patientId,
        ProviderId = providerId,
        SubscriptionId = subscriptionId,
        IsEncrypted = true,
        AllowFileSharing = true,
        AllowVoiceCalls = true,
        AllowVideoCalls = true
    };

    var chatRoomResult = await _chatStorageService.CreateChatRoomAsync(createDto);

    // Add participants to thread
    await _chatStorageService.AddParticipantAsync(chatRoomResult.Data.Id, patientId, 
        ChatRoomParticipant.ParticipantRole.Patient);
    await _chatStorageService.AddParticipantAsync(chatRoomResult.Data.Id, providerId, 
        ChatRoomParticipant.ParticipantRole.Provider);

    return chatRoomResult;
}
```

#### Group Threads
```csharp
public async Task<ApiResponse<ChatRoomDto>> CreateGroupChatRoomAsync(
    string name, 
    string? description, 
    List<Guid> participantIds, 
    Guid creatorId)
{
    // Create thread
    var createDto = new CreateChatRoomDto
    {
        Name = name,
        Description = description,
        Type = ChatRoom.ChatRoomType.Group,
        IsEncrypted = true,
        AllowFileSharing = true,
        AllowVoiceCalls = true,
        AllowVideoCalls = true
    };

    var chatRoomResult = await _chatStorageService.CreateChatRoomAsync(createDto);

    // Add all participants with appropriate roles
    foreach (var participantId in participantIds)
    {
        var role = participantId == creatorId ? 
            ChatRoomParticipant.ParticipantRole.Admin : 
            ChatRoomParticipant.ParticipantRole.Participant;
        await _chatStorageService.AddParticipantAsync(chatRoomResult.Data.Id, participantId, role);
    }

    return chatRoomResult;
}
```

### 2. Thread Access Control

#### Subscription-Based Access
```csharp
public async Task<ApiResponse<bool>> ValidateChatAccessAsync(Guid chatRoomId, Guid userId)
{
    var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId);
    if (chatRoom == null)
        return ApiResponse<bool>.ErrorResponse("Chat room not found", 404);

    // Check if user is participant
    var participant = await _chatRoomParticipantRepository.GetByChatRoomAndUserAsync(chatRoomId, userId);
    if (participant == null)
        return ApiResponse<bool>.ErrorResponse("Access denied", 403);

    // Check subscription requirements for patient-provider chats
    if (chatRoom.Type == ChatRoom.ChatRoomType.PatientProvider && 
        chatRoom.SubscriptionId.HasValue)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(chatRoom.SubscriptionId.Value);
        if (subscription?.Status != Subscription.SubscriptionStatus.Active)
        {
            return ApiResponse<bool>.ErrorResponse("Active subscription required", 403);
        }
    }

    return ApiResponse<bool>.SuccessResponse(true, "Access granted");
}
```

## Message History Management

### 1. Message Storage

#### Database Persistence
```csharp
public async Task<Message> CreateMessageAsync(Message message)
{
    // Set message metadata
    message.CreatedAt = DateTime.UtcNow;
    message.Status = Message.MessageStatus.Sent;
    message.IsEncrypted = true;
    message.EncryptionKey = GenerateEncryptionKey();

    // Encrypt message content
    if (!string.IsNullOrEmpty(message.Content))
    {
        message.Content = await _encryptionService.EncryptAsync(message.Content, message.EncryptionKey);
    }

    // Store in database
    _context.Messages.Add(message);
    await _context.SaveChangesAsync();

    // Update chat room last activity
    var chatRoom = await _chatRoomRepository.GetByIdAsync(message.ChatRoomId);
    if (chatRoom != null)
    {
        chatRoom.LastActivityAt = DateTime.UtcNow;
        await _chatRoomRepository.UpdateAsync(chatRoom);
    }

    return message;
}
```

#### File Attachment Handling
```csharp
public async Task<ApiResponse<MessageDto>> SendMessageWithAttachmentAsync(
    CreateMessageDto createDto, 
    IFormFile file, 
    Guid senderId)
{
    // Upload file to storage
    var uploadResult = await _fileStorageService.UploadFileAsync(file, "chat-attachments");
    if (!uploadResult.Success)
        return ApiResponse<MessageDto>.ErrorResponse("File upload failed", 500);

    // Create message with attachment metadata
    createDto.Type = GetMessageTypeFromFile(file);
    createDto.FileName = file.FileName;
    createDto.FilePath = uploadResult.Data.FilePath;
    createDto.FileType = file.ContentType;
    createDto.FileSize = file.Length;

    // Send message
    var messageResult = await _messagingService.SendMessageAsync(createDto, senderId);
    return messageResult;
}
```

### 2. Message Retrieval

#### Paginated History
```csharp
public async Task<IEnumerable<Message>> GetMessagesByChatRoomAsync(
    Guid chatRoomId, 
    int skip = 0, 
    int take = 50)
{
    return await _context.Messages
        .Include(m => m.Sender)
        .Include(m => m.ChatRoom)
        .Include(m => m.ReplyToMessage)
        .Include(m => m.Replies)
        .Include(m => m.Reactions)
        .Include(m => m.ReadReceipts)
        .Include(m => m.Attachments)
        .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
        .OrderByDescending(m => m.CreatedAt)
        .Skip(skip)
        .Take(take)
        .ToListAsync();
}
```

#### Search Functionality
```csharp
public async Task<IEnumerable<Message>> SearchMessagesAsync(
    Guid chatRoomId, 
    string searchTerm)
{
    return await _context.Messages
        .Include(m => m.Sender)
        .Include(m => m.ChatRoom)
        .Include(m => m.ReplyToMessage)
        .Where(m => m.ChatRoomId == chatRoomId && 
                   !m.IsDeleted && 
                   m.Content.Contains(searchTerm))
        .OrderByDescending(m => m.CreatedAt)
        .ToListAsync();
}
```

#### Date Range Filtering
```csharp
public async Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(
    Guid chatRoomId, 
    DateTime startDate, 
    DateTime endDate)
{
    return await _context.Messages
        .Include(m => m.Sender)
        .Include(m => m.ChatRoom)
        .Include(m => m.ReplyToMessage)
        .Where(m => m.ChatRoomId == chatRoomId && 
                   !m.IsDeleted && 
                   m.CreatedAt >= startDate && 
                   m.CreatedAt <= endDate)
        .OrderBy(m => m.CreatedAt)
        .ToListAsync();
}
```

### 3. Message Threading (Replies)

#### Reply Structure
```csharp
public async Task<ApiResponse<MessageDto>> SendReplyAsync(
    Guid originalMessageId, 
    CreateMessageDto createDto, 
    Guid senderId)
{
    // Validate original message exists
    var originalMessage = await _messageRepository.GetByIdAsync(originalMessageId);
    if (originalMessage == null)
        return ApiResponse<MessageDto>.ErrorResponse("Original message not found", 404);

    // Set reply relationship
    createDto.ReplyToMessageId = originalMessageId;
    createDto.ChatRoomId = originalMessage.ChatRoomId;

    // Send reply message
    var messageResult = await _messagingService.SendMessageAsync(createDto, senderId);
    return messageResult;
}
```

#### Thread Retrieval
```csharp
public async Task<IEnumerable<Message>> GetRepliesAsync(Guid messageId)
{
    return await _context.Messages
        .Include(m => m.Sender)
        .Include(m => m.ChatRoom)
        .Include(m => m.ReplyToMessage)
        .Where(m => m.ReplyToMessageId == messageId && !m.IsDeleted)
        .OrderBy(m => m.CreatedAt)
        .ToListAsync();
}
```

## Real-Time Message Handling

### 1. SignalR Hub Management

#### Connection Tracking
```csharp
public class ChatHub : Hub
{
    private static readonly Dictionary<string, string> _userConnections = new();
    private static readonly Dictionary<string, HashSet<string>> _chatRoomGroups = new();
    private static readonly Dictionary<string, DateTime> _userLastSeen = new();

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != Guid.Empty)
        {
            _userConnections[userId.ToString()] = Context.ConnectionId;
            _userLastSeen[userId.ToString()] = DateTime.UtcNow;
            
            // Notify other users about online status
            await NotifyUserOnlineStatus(userId, true);
        }
        await base.OnConnectedAsync();
    }
}
```

#### Message Broadcasting
```csharp
public async Task SendMessage(string chatRoomId, string content, string? replyToMessageId = null, string? filePath = null)
{
    var userId = GetUserId();
    if (userId == Guid.Empty) return;

    try
    {
        var createMessageDto = new CreateMessageDto
        {
            ChatRoomId = Guid.Parse(chatRoomId),
            Content = content,
            Type = string.IsNullOrEmpty(filePath) ? Message.MessageType.Text : GetMessageTypeFromFilePath(filePath),
            ReplyToMessageId = !string.IsNullOrEmpty(replyToMessageId) ? Guid.Parse(replyToMessageId) : null,
            FilePath = filePath
        };

        var result = await _chatService.SendMessageWithNotificationAsync(createMessageDto, userId);
        
        if (result.Success)
        {
            // Get the actual message data
            var messages = await _messagingService.GetChatRoomMessagesAsync(Guid.Parse(chatRoomId), 0, 1);
            if (messages.Success && messages.Data.Any())
            {
                var message = messages.Data.First();
                await Clients.Group(chatRoomId).SendAsync("MessageReceived", message);
                await Clients.Caller.SendAsync("MessageSent", message.Id);
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error sending message in chat room {ChatRoomId}", chatRoomId);
        await Clients.Caller.SendAsync("MessageFailed", "Failed to send message");
    }
}
```

### 2. Read Receipts

#### Real-Time Read Status
```csharp
public async Task MarkMessageAsRead(string messageId)
{
    var userId = GetUserId();
    if (userId == Guid.Empty) return;

    try
    {
        var result = await _messagingService.MarkMessageAsReadAsync(Guid.Parse(messageId), userId);
        if (result.Success)
        {
            var message = await _messagingService.GetMessageAsync(Guid.Parse(messageId));
            if (message.Success && message.Data != null)
            {
                await Clients.Group(message.Data.ChatRoomId.ToString())
                    .SendAsync("MessageRead", messageId, userId);
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
    }
}
```

### 3. Typing Indicators

#### Real-Time Typing Status
```csharp
public async Task SendTypingIndicator(string chatRoomId, bool isTyping)
{
    var userId = GetUserId();
    if (userId == Guid.Empty) return;

    try
    {
        var typingIndicator = new TypingIndicatorDto
        {
            ChatRoomId = Guid.Parse(chatRoomId),
            UserId = userId,
            UserName = GetUserName(),
            IsTyping = isTyping
        };

        await Clients.OthersInGroup(chatRoomId).SendAsync("TypingIndicator", typingIndicator);
        
        // Persist typing indicator for offline users
        await _messagingService.SendTypingIndicatorAsync(Guid.Parse(chatRoomId), userId, isTyping);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error sending typing indicator in chat room {ChatRoomId}", chatRoomId);
    }
}
```

## Data Persistence and Performance

### 1. Database Indexing

#### Performance Optimizations
```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // Chat room indexes
    builder.Entity<ChatRoom>()
        .HasIndex(cr => cr.PatientId)
        .HasDatabaseName("IX_ChatRooms_PatientId");

    builder.Entity<ChatRoom>()
        .HasIndex(cr => cr.ProviderId)
        .HasDatabaseName("IX_ChatRooms_ProviderId");

    builder.Entity<ChatRoom>()
        .HasIndex(cr => cr.SubscriptionId)
        .HasDatabaseName("IX_ChatRooms_SubscriptionId");

    builder.Entity<ChatRoom>()
        .HasIndex(cr => cr.LastActivityAt)
        .HasDatabaseName("IX_ChatRooms_LastActivityAt");

    // Message indexes
    builder.Entity<Message>()
        .HasIndex(m => m.ChatRoomId)
        .HasDatabaseName("IX_Messages_ChatRoomId");

    builder.Entity<Message>()
        .HasIndex(m => m.SenderId)
        .HasDatabaseName("IX_Messages_SenderId");

    builder.Entity<Message>()
        .HasIndex(m => m.CreatedAt)
        .HasDatabaseName("IX_Messages_CreatedAt");

    builder.Entity<Message>()
        .HasIndex(m => new { m.ChatRoomId, m.CreatedAt })
        .HasDatabaseName("IX_Messages_ChatRoomId_CreatedAt");

    // Participant indexes
    builder.Entity<ChatRoomParticipant>()
        .HasIndex(cp => new { cp.ChatRoomId, cp.UserId })
        .HasDatabaseName("IX_ChatRoomParticipants_ChatRoomId_UserId")
        .IsUnique();
}
```

### 2. Message Archiving

#### Automatic Archiving
```csharp
public async Task ArchiveOldMessagesAsync(int daysOld = 365)
{
    var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
    
    var oldMessages = await _context.Messages
        .Where(m => m.CreatedAt < cutoffDate && !m.IsDeleted)
        .ToListAsync();

    foreach (var message in oldMessages)
    {
        message.IsDeleted = true;
        message.UpdatedAt = DateTime.UtcNow;
    }

    await _context.SaveChangesAsync();
}
```

### 3. Encryption and Security

#### Message Encryption
```csharp
public async Task<string> EncryptMessageAsync(string content, string encryptionKey)
{
    using var aes = Aes.Create();
    aes.Key = Convert.FromBase64String(encryptionKey);
    aes.GenerateIV();

    using var encryptor = aes.CreateEncryptor();
    using var msEncrypt = new MemoryStream();
    using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
    using var swEncrypt = new StreamWriter(csEncrypt);

    await swEncrypt.WriteAsync(content);
    await swEncrypt.FlushAsync();
    await csEncrypt.FlushFinalBlockAsync();

    var encrypted = msEncrypt.ToArray();
    var result = Convert.ToBase64String(aes.IV.Concat(encrypted).ToArray());
    return result;
}
```

## Subscription Integration

### 1. Message Limits

#### Subscription-Based Limits
```csharp
public async Task<ApiResponse<bool>> CheckMessageLimitAsync(Guid userId, Guid chatRoomId)
{
    var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId);
    if (chatRoom?.SubscriptionId == null)
        return ApiResponse<bool>.SuccessResponse(true, "No subscription required");

    var subscription = await _subscriptionRepository.GetByIdAsync(chatRoom.SubscriptionId.Value);
    if (subscription == null)
        return ApiResponse<bool>.ErrorResponse("Subscription not found", 404);

    // Check message limits based on subscription plan
    var messageCount = await _messageRepository.GetMessageCountForUserInChatRoomAsync(userId, chatRoomId);
    var plan = await _subscriptionPlanRepository.GetByIdAsync(subscription.SubscriptionPlanId);

    if (messageCount >= plan.MonthlyMessageLimit)
    {
        return ApiResponse<bool>.ErrorResponse("Message limit reached for this month", 429);
    }

    return ApiResponse<bool>.SuccessResponse(true, "Message limit check passed");
}
```

### 2. Feature Access Control

#### Subscription Feature Validation
```csharp
public async Task<ApiResponse<bool>> ValidateChatFeaturesAsync(Guid chatRoomId, Guid userId, string feature)
{
    var chatRoom = await _chatRoomRepository.GetByIdAsync(chatRoomId);
    if (chatRoom?.SubscriptionId == null)
        return ApiResponse<bool>.ErrorResponse("Subscription required for this feature", 403);

    var subscription = await _subscriptionRepository.GetByIdAsync(chatRoom.SubscriptionId.Value);
    var plan = await _subscriptionPlanRepository.GetByIdAsync(subscription.SubscriptionPlanId);

    return feature switch
    {
        "file_sharing" => ApiResponse<bool>.SuccessResponse(plan.AllowFileSharing, "File sharing check"),
        "voice_calls" => ApiResponse<bool>.SuccessResponse(plan.AllowVoiceCalls, "Voice calls check"),
        "video_calls" => ApiResponse<bool>.SuccessResponse(plan.AllowVideoCalls, "Video calls check"),
        "group_chat" => ApiResponse<bool>.SuccessResponse(plan.AllowGroupChat, "Group chat check"),
        _ => ApiResponse<bool>.ErrorResponse("Unknown feature", 400)
    };
}
```

## Monitoring and Analytics

### 1. Chat Statistics

#### Real-Time Analytics
```csharp
public async Task<ApiResponse<Dictionary<string, object>>> GetChatRoomStatisticsAsync(Guid chatRoomId, Guid userId)
{
    // Validate access
    var accessResult = await _chatStorageService.ValidateChatAccessAsync(chatRoomId, userId);
    if (!accessResult.Success)
        return ApiResponse<Dictionary<string, object>>.ErrorResponse("Access denied", 403);

    // Get chat room
    var chatRoomResult = await _chatStorageService.GetChatRoomAsync(chatRoomId);
    if (!chatRoomResult.Success)
        return ApiResponse<Dictionary<string, object>>.ErrorResponse("Chat room not found", 404);

    // Get participants
    var participantsResult = await _chatStorageService.GetChatRoomParticipantsAsync(chatRoomId);
    var participantCount = participantsResult.Success ? participantsResult.Data.Count() : 0;

    // Get messages
    var messagesResult = await _chatStorageService.GetChatRoomMessagesAsync(chatRoomId, 0, 10000);
    var messages = messagesResult.Success ? messagesResult.Data.ToList() : new List<MessageDto>();

    // Get unread messages
    var unreadResult = await _chatStorageService.GetUnreadMessagesAsync(chatRoomId, userId);
    var unreadCount = unreadResult.Success ? unreadResult.Data.Count() : 0;

    // Calculate statistics
    var statistics = new Dictionary<string, object>
    {
        ["TotalMessages"] = messages.Count,
        ["ParticipantCount"] = participantCount,
        ["UnreadCount"] = unreadCount,
        ["LastActivity"] = chatRoomResult.Data.LastActivityAt,
        ["CreatedAt"] = chatRoomResult.Data.CreatedAt,
        ["MessageTypes"] = messages.GroupBy(m => m.Type)
            .ToDictionary(g => g.Key.ToString(), g => g.Count()),
        ["RecentActivity"] = messages.Take(10).ToList()
    };

    return ApiResponse<Dictionary<string, object>>.SuccessResponse(statistics, "Statistics retrieved successfully");
}
```

## Summary

The chat system maintains threads and history through:

1. **Persistent Database Storage**: All messages, threads, and metadata are stored in SQL Server with proper indexing
2. **Real-Time Communication**: SignalR hubs handle live message delivery and status updates
3. **Thread Management**: ChatRoom entities represent persistent threads with participant management
4. **Message History**: Complete message history with replies, reactions, and read receipts
5. **File Attachments**: Secure file storage with encryption and access control
6. **Subscription Integration**: Message limits and feature access based on subscription plans
7. **Security**: End-to-end encryption and access control for HIPAA compliance
8. **Performance**: Optimized queries with proper indexing and pagination
9. **Analytics**: Real-time statistics and monitoring capabilities

This architecture ensures that chat threads are persistent, searchable, and maintain complete history while providing real-time communication capabilities for the telehealth platform. 