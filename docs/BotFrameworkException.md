# BotFrameworkException

`BotFrameworkException` is the base exception type for the `telegram-bot-framework-dotnet` library, providing structured error handling for bot operations. It serves as the foundation for more specific exception types, allowing developers to catch and handle errors related to command execution, permissions, sessions, and user interactions in a consistent manner.

## API

### `public string? ErrorCode`
A machine-readable error code associated with the exception. This property may be `null` if no specific error code is assigned.
- **Purpose**: Enables programmatic identification of the error type for logging or recovery logic.
- **Thread Safety**: Safe to read from multiple threads; modifications should be synchronized externally if required.

---

### `public BotFrameworkException(string message) : base`
Initializes a new instance of `BotFrameworkException` with a specified error message.
- **Parameters**:
  - `message` (string): A human-readable description of the error.
- **Throws**: None.

---

### `public BotFrameworkException()`
Initializes a new instance of `BotFrameworkException` with default values.
- **Purpose**: Used when no additional context is required for the exception.
- **Throws**: None.

---

### `public BotFrameworkException(string message, Exception innerException)`
Initializes a new instance of `BotFrameworkException` with a specified error message and a reference to the inner exception that caused this exception.
- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `innerException` (Exception): The exception that caused the current exception.
- **Throws**: None.

---

### `public string? CommandName` *(Applicable to `CommandExecutionException`, `CommandNotFoundException`)*
The name of the command associated with the exception. This property may be `null` if the exception is not command-specific.
- **Purpose**: Identifies the command involved in the error, useful for debugging or user feedback.
- **Thread Safety**: Safe to read from multiple threads.

---

### `public CommandExecutionException(string message, string commandName)`
Initializes a new instance of `CommandExecutionException` with a specified error message and the name of the command that failed to execute.
- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `commandName` (string): The name of the command that caused the exception.
- **Throws**: None.

---

### `public CommandExecutionException(string message, string commandName, Exception innerException)`
Initializes a new instance of `CommandExecutionException` with a specified error message, the name of the command, and a reference to the inner exception.
- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `commandName` (string): The name of the command that caused the exception.
  - `innerException` (Exception): The exception that caused the current exception.
- **Throws**: None.

---

### `public CommandNotFoundException(string message, string commandName)`
Initializes a new instance of `CommandNotFoundException` with a specified error message and the name of the command that was not found.
- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `commandName` (string): The name of the command that could not be found.
- **Throws**: None.

---

### `public long? UserId` *(Applicable to `UserException`, `InsufficientPermissionException`)*
The unique identifier of the user associated with the exception. This property may be `null` if the exception is not user-specific.
- **Purpose**: Tracks the user involved in the error, useful for logging or user-specific recovery logic.
- **Thread Safety**: Safe to read from multiple threads.

---

### `public string? RequiredPermission` *(Applicable to `InsufficientPermissionException`)*
The permission required to perform the action that triggered the exception. This property may be `null` if no specific permission is associated.
- **Purpose**: Identifies the missing permission, enabling targeted error messages or recovery logic.
- **Thread Safety**: Safe to read from multiple threads.

---

### `public InsufficientPermissionException(string message, long userId, string requiredPermission)`
Initializes a new instance of `InsufficientPermissionException` with a specified error message, the user ID, and the required permission that was missing.
- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `userId` (long): The ID of the user lacking the required permission.
  - `requiredPermission` (string): The permission that was required but not granted.
- **Throws**: None.

---

### `public string? SessionId` *(Applicable to `SessionException`)*
The unique identifier of the session associated with the exception. This property may be `null` if the exception is not session-specific.
- **Purpose**: Tracks the session involved in the error, useful for debugging or session-specific recovery logic.
- **Thread Safety**: Safe to read from multiple threads.

---

### `public SessionException(string message, string sessionId)`
Initializes a new instance of `SessionException` with a specified error message and the session ID associated with the error.
- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `sessionId` (string): The ID of the session that caused the exception.
- **Throws**: None.

---

### `public SessionException(string message, string sessionId, Exception innerException)`
Initializes a new instance of `SessionException` with a specified error message, the session ID, and a reference to the inner exception.
- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `sessionId` (string): The ID of the session that caused the exception.
  - `innerException` (Exception): The exception that caused the current exception.
- **Throws**: None.

---

### `public UserException(string message, long userId)`
Initializes a new instance of `UserException` with a specified error message and the user ID associated with the error.
- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `userId` (long): The ID of the user involved in the exception.
- **Throws**: None.

---

### `public UserException(string message, long userId, Exception innerException)`
Initializes a new instance of `UserException` with a specified error message, the user ID, and a reference to the inner exception.
- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `userId` (long): The ID of the user involved in the exception.
  - `innerException` (Exception): The exception that caused the current exception.
- **Throws**: None.

## Usage

### Example 1: Handling Command Execution Errors
