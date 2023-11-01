# API Reference

Complete API documentation for the Telegram Bot Framework.

## Base URL

```
https://localhost:5001/api
```

## Authentication

Most endpoints require authentication via:
- **Header**: `Authorization: Bearer {token}`
- **Or Header**: `X-API-Key: {token}`

Public endpoints (no auth required):
- `POST /api/bot/message`
- `GET /api/bot/health`

## Bot Endpoints

### POST /api/bot/message

Process incoming message from Telegram.

**Request Body:**
```json
{
  "userId": 123456789,
  "chatId": 123456789,
  "content": "Hello bot!",
  "type": "text",
  "metadata": {
    "messageId": 42,
    "source": "telegram"
  }
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "messageId": "msg-abc123",
  "status": "processed",
  "processedAt": "2026-05-04T10:30:00Z"
}
```

**Errors:**
- `400 Bad Request` - Invalid message format
- `429 Too Many Requests` - Rate limit exceeded
- `500 Internal Server Error` - Processing error

---

### GET /api/bot/health

Health check endpoint.

**Response (200 OK):**
```json
{
  "status": "healthy",
  "uptime": "2h 30m",
  "timestamp": "2026-05-04T10:30:00Z",
  "version": "1.0.0"
}
```

---

### GET /api/bot/user/{userId}

Get user information.

**Path Parameters:**
- `userId` (long) - User ID

**Response (200 OK):**
```json
{
  "id": "usr-123",
  "telegramId": 123456789,
  "firstName": "John",
  "lastName": "Doe",
  "username": "johndoe",
  "phoneNumber": "+1234567890",
  "role": "user",
  "status": "active",
  "createdAt": "2026-01-01T00:00:00Z",
  "updatedAt": "2026-05-04T10:30:00Z"
}
```

**Errors:**
- `404 Not Found` - User not found
- `401 Unauthorized` - Missing authentication

---

### GET /api/bot/session/{userId}

Get active user session.

**Path Parameters:**
- `userId` (long) - User ID

**Response (200 OK):**
```json
{
  "sessionId": "session-abc123",
  "userId": 123456789,
  "chatId": 123456789,
  "state": "active",
  "currentMenuId": "main_menu",
  "contextData": {
    "registration_step": "2",
    "user_form": "{...}"
  },
  "expiresAt": "2026-05-04T11:30:00Z",
  "createdAt": "2026-05-04T10:30:00Z"
}
```

---

### GET /api/bot/commands

List all available commands.

**Query Parameters:**
- `enabled` (bool, optional) - Filter by enabled status

**Response (200 OK):**
```json
{
  "commands": [
    {
      "name": "/start",
      "description": "Start the bot",
      "type": "standard",
      "requiresAdmin": false,
      "isEnabled": true,
      "rateLimitPerMinute": 30,
      "parameters": []
    },
    {
      "name": "/admin",
      "description": "Admin commands",
      "type": "standard",
      "requiresAdmin": true,
      "isEnabled": true,
      "rateLimitPerMinute": 60,
      "parameters": [
        {
          "name": "action",
          "type": "string",
          "isRequired": true
        }
      ]
    }
  ]
}
```

---

### GET /api/bot/menu/{menuId}

Get menu by ID.

**Path Parameters:**
- `menuId` (string) - Menu ID

**Response (200 OK):**
```json
{
  "id": "main_menu",
  "title": "Main Menu",
  "description": "Choose an option",
  "type": "inline",
  "isActive": true,
  "maxButtonsPerRow": 2,
  "buttons": [
    {
      "label": "Settings",
      "callbackData": "settings",
      "action": "navigate_menu"
    },
    {
      "label": "Help",
      "callbackData": "help",
      "action": "navigate_menu"
    }
  ]
}
```

---

## Admin Endpoints

All admin endpoints require authentication with admin role.

### GET /api/admin/config

Get bot configuration.

**Response (200 OK):**
```json
{
  "botToken": "***hidden***",
  "botUsername": "my_awesome_bot",
  "webhookUrl": "https://bot.example.com/api/bot/webhook",
  "useWebhook": true,
  "sessionTimeoutMinutes": 30,
  "enableLogging": true,
  "enableRateLimiting": true,
  "rateLimitPerMinute": 30
}
```

---

### GET /api/admin/statistics

Get bot statistics and metrics.

**Response (200 OK):**
```json
{
  "totalUsers": 1250,
  "activeUsers": 340,
  "bannedUsers": 12,
  "totalMessages": 45230,
  "messagesProcessedToday": 3240,
  "averageResponseTime": 245,
  "uptime": "7d 2h 15m",
  "cacheHitRate": 0.85,
  "commandsExecuted": 12450,
  "sessionsActive": 125
}
```

---

### GET /api/admin/admins

List all administrators.

**Response (200 OK):**
```json
{
  "admins": [
    {
      "id": "usr-123",
      "telegramId": 123456789,
      "firstName": "John",
      "lastName": "Doe",
      "role": "admin",
      "promotedAt": "2026-01-01T00:00:00Z"
    }
  ]
}
```

---

### POST /api/admin/promote-admin/{userId}

Promote user to admin.

**Path Parameters:**
- `userId` (long) - User ID to promote

**Response (200 OK):**
```json
{
  "success": true,
  "message": "User promoted to admin",
  "userId": 123456789,
  "newRole": "admin"
}
```

---

### POST /api/admin/demote-admin/{userId}

Demote admin to regular user.

**Path Parameters:**
- `userId` (long) - User ID to demote

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Admin demoted to user",
  "userId": 123456789,
  "newRole": "user"
}
```

---

### POST /api/admin/ban-user/{userId}

Ban a user.

**Path Parameters:**
- `userId` (long) - User ID to ban

**Request Body:**
```json
{
  "reason": "Spamming content"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "User banned",
  "userId": 123456789,
  "status": "banned",
  "reason": "Spamming content"
}
```

---

### POST /api/admin/unban-user/{userId}

Unban a user.

**Path Parameters:**
- `userId` (long) - User ID to unban

**Response (200 OK):**
```json
{
  "success": true,
  "message": "User unbanned",
  "userId": 123456789,
  "status": "active"
}
```

---

### POST /api/admin/commands

Register a new command.

**Request Body:**
```json
{
  "name": "/mycommand",
  "description": "My custom command",
  "handlerType": "MyCommandHandler",
  "type": "standard",
  "requiresAdmin": false,
  "isEnabled": true,
  "rateLimitPerMinute": 30,
  "parameters": [
    {
      "name": "text",
      "type": "string",
      "isRequired": true
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "commandId": "cmd-abc123",
  "name": "/mycommand"
}
```

---

### GET /api/admin/commands/{commandName}

Get command details.

**Path Parameters:**
- `commandName` (string) - Command name (e.g., "start")

**Response (200 OK):**
```json
{
  "name": "/start",
  "description": "Start the bot",
  "type": "standard",
  "requiresAdmin": false,
  "isEnabled": true,
  "parameters": []
}
```

---

### DELETE /api/admin/commands/{commandName}

Delete a command.

**Path Parameters:**
- `commandName` (string) - Command name

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Command deleted",
  "commandName": "/mycommand"
}
```

---

### GET /api/admin/menus

List all menus.

**Response (200 OK):**
```json
{
  "menus": [
    {
      "id": "main_menu",
      "title": "Main Menu",
      "type": "inline",
      "isActive": true,
      "buttonCount": 5
    }
  ]
}
```

---

### POST /api/admin/sessions/close-expired

Close all expired sessions.

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Expired sessions closed",
  "sessionsClosedCount": 42
}
```

---

## Error Responses

All errors follow this format:

```json
{
  "success": false,
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "details": {
      "field": "Additional context"
    }
  },
  "timestamp": "2026-05-04T10:30:00Z"
}
```

### Common Error Codes

| Code | Status | Description |
|------|--------|-------------|
| INVALID_REQUEST | 400 | Request format invalid |
| UNAUTHORIZED | 401 | Authentication required |
| FORBIDDEN | 403 | Insufficient permissions |
| NOT_FOUND | 404 | Resource not found |
| RATE_LIMIT_EXCEEDED | 429 | Too many requests |
| INTERNAL_ERROR | 500 | Server error |

---

## Rate Limiting

Responses include rate limit headers:

```
X-RateLimit-Limit: 30
X-RateLimit-Remaining: 29
X-RateLimit-Reset: 1651667400
```

---

## Data Types

### UserRole
- `user` - Regular user
- `moderator` - Moderator
- `admin` - Administrator
- `owner` - Bot owner

### UserStatus
- `active` - Active user
- `inactive` - Inactive
- `banned` - Banned user
- `suspended` - Temporarily suspended

### MessageType
- `text` - Text message
- `photo` - Photo message
- `video` - Video message
- `audio` - Audio message
- `file` - File message
- `command` - Command message

### MessageStatus
- `received` - Message received
- `processing` - Processing message
- `processed` - Message processed
- `failed` - Processing failed
- `archived` - Archived message

---

## Pagination

Endpoints that return lists support pagination:

```
GET /api/admin/users?page=1&pageSize=20&sortBy=createdAt&sortOrder=desc
```

**Response:**
```json
{
  "items": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 1250,
    "totalPages": 63
  }
}
```

---

## WebHook Events

When using webhook mode, Telegram sends updates to your webhook URL.

**Example webhook request:**
```json
{
  "update_id": 123456789,
  "message": {
    "message_id": 42,
    "date": 1651667400,
    "chat": {
      "id": 123456789,
      "type": "private"
    },
    "from": {
      "id": 123456789,
      "is_bot": false,
      "first_name": "John",
      "last_name": "Doe",
      "username": "johndoe"
    },
    "text": "/start"
  }
}
```

**Webhook response (must return 200 OK):**
```json
{
  "ok": true
}
```

---

## Examples

### Using curl

```bash
# Health check
curl https://localhost:5001/api/bot/health

# Get user
curl -H "Authorization: Bearer YOUR_TOKEN" \
     https://localhost:5001/api/bot/user/123456789

# Send message
curl -X POST https://localhost:5001/api/bot/message \
     -H "Content-Type: application/json" \
     -d '{
       "userId": 123456789,
       "chatId": 123456789,
       "content": "Hello",
       "type": "text"
     }'
```

### Using C#

```csharp
using var client = new HttpClient();
var response = await client.GetAsync("https://localhost:5001/api/bot/health");
var json = await response.Content.ReadAsStringAsync();
```

### Using JavaScript

```javascript
fetch('https://localhost:5001/api/bot/health')
  .then(r => r.json())
  .then(data => console.log(data));
```

---

## OpenAPI/Swagger

API documentation available at:
```
https://localhost:5001/swagger
```

Includes interactive testing interface.

---

## Versioning

API version in header:
```
X-API-Version: 1.0
```

Backward compatibility maintained for minor versions.
