# StringExtensionTests

Unit test class for `StringExtensions`, verifying behavior of string utility methods such as truncation, validation, repetition, extraction, and formatting.

## API

### `Truncate_VariousInputs_TruncatesCorrectly()`
Verifies that `Truncate` shortens strings to the specified length, preserving the beginning and appending an ellipsis when truncation occurs.

### `Truncate_NullInput_ReturnsNull()`
Ensures that passing `null` to `Truncate` returns `null` without throwing.

### `IsValidEmail_WithValidFormat_ReturnsTrue()`
Confirms that `IsValidEmail` returns `true` for strings matching a standard email format.

### `IsValidEmail_WithMissingAtSign_ReturnsFalse()`
Validates that `IsValidEmail` returns `false` when the input lacks an `@` symbol.

### `IsValidEmail_WithEmptyString_ReturnsFalse()`
Checks that `IsValidEmail` returns `false` for an empty string.

### `IsValidEmail_WithMissingDomain_ReturnsFalse()`
Ensures `IsValidEmail` returns `false` when the domain part is absent.

### `Repeat_PositiveCount_ProducesRepeatedString()`
Tests that `Repeat` returns a new string composed of the original repeated the specified number of times.

### `Repeat_ZeroCount_ReturnsEmpty()`
Validates that `Repeat` returns an empty string when the count is zero.

### `Repeat_NegativeCount_ReturnsEmpty()`
Confirms that `Repeat` returns an empty string when the count is negative.

### `ExtractNumbers_FromMixedString_ReturnsOnlyDigits()`
Ensures `ExtractNumbers` returns a string containing only the numeric characters from the input.

### `ExtractNumbers_FromStringWithNoDigits_ReturnsEmpty()`
Verifies that `ExtractNumbers` returns an empty string when the input contains no digits.

### `EnsureStartsWith_WhenPrefixMissing_PrependPrefix()`
Checks that `EnsureStartsWith` prepends the specified prefix when it is not already present at the start.

### `EnsureStartsWith_WhenAlreadyHasPrefix_ReturnsUnchanged()`
Validates that `EnsureStartsWith` returns the original string unchanged if the prefix is already present.

### `EnsureEndsWith_WhenSuffixMissing_AppendsSuffix()`
Ensures `EnsureEndsWith` appends the specified suffix when it is not already present at the end.

### `EnsureEndsWith_WhenAlreadyHasSuffix_ReturnsUnchanged()`
Confirms that `EnsureEndsWith` returns the original string unchanged if the suffix is already present.

### `Capitalize_WithLowercaseFirstChar_CapitalizesFirstChar()`
Tests that `Capitalize` converts the first character of the string to uppercase if it is lowercase.

### `Capitalize_WithAlreadyCapitalized_ReturnsUnchanged()`
Validates that `Capitalize` returns the original string unchanged if the first character is already uppercase.

### `IsAlphanumeric_WithPureAlphanumericString_ReturnsTrue()`
Ensures `IsAlphanumeric` returns `true` for strings containing only letters and digits.

### `IsAlphanumeric_WithSpecialCharacters_ReturnsFalse()`
Confirms that `IsAlphanumeric` returns `false` when the string contains special characters.

### `IsAlphanumeric_WithSpaces_ReturnsFalse()`
Validates that `IsAlphanumeric` returns `false` when the string contains whitespace.

## Usage
