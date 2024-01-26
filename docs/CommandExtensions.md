# CommandExtensions

Provides utility methods for inspecting and manipulating command patterns in Telegram bot command strings. These extensions help parse, validate, and format command strings according to standard Telegram bot API conventions.

## API

### `HasParameters`

Determines whether a command string contains parameters.

- **Parameters**
  - `command` (string): The command string to check (e.g., "/start param1 param2").
- **Return value**
  - `true` if the command contains parameters; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `command` is `null`.

### `GetPrimaryPattern`

Extracts the primary command pattern from a full command string.

- **Parameters**
  - `command` (string): The command string to parse (e.g., "/start param1 param2").
- **Return value**
  - The primary command pattern (e.g., "/start").
- **Exceptions**
  - Throws `ArgumentNullException` if `command` is `null`.

### `IsStandardCommand`

Checks if a command string adheres to standard Telegram bot command format.

- **Parameters**
  - `command` (string): The command string to validate (e.g., "/start").
- **Return value**
  - `true` if the command is a standard Telegram bot command; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `command` is `null`.

### `GetFormattedString`

Formats a command string by extracting the primary pattern and parameters.

- **Parameters**
  - `command` (string): The command string to format (e.g., "/start param1 param2").
- **Return value**
  - The formatted command string (e.g., "/start param1 param2").
- **Exceptions**
  - Throws `ArgumentNullException` if `command` is `null`.

## Usage
