# UserServiceTests

Unit test suite for the `UserService` class, verifying its core user management operations—retrieval, creation, update, deletion, activity recording, and search—against an in-memory or mocked data store. Each test method targets a specific behavior or edge case to ensure the service handles both nominal and boundary conditions correctly.

## API

### `public UserServiceTests`
Constructor. Initializes the test fixture, setting up any required dependencies such as a database context mock, an in-memory collection, or a service instance under test. No parameters.

### `public async Task GetOrCreateUserAsync_WithExistingUser_ReturnsExistingUser`
Verifies that when a user with a given Telegram ID already exists, `GetOrCreateUserAsync` returns that existing record without creating a duplicate.  
**Parameters:** None (self-contained test).  
**Returns:** A completed task once the assertion passes.  
**Throws:** Assertion failures if the returned user does not match the pre-existing entity.

### `public async Task GetOrCreateUserAsync_WithNonExistingUser_CreatesAndReturnsNewUser`
Confirms that for a Telegram ID not present in the store, `GetOrCreateUserAsync` creates a new user record with the supplied details and returns it.  
**Parameters:** None.  
**Returns:** A completed task after verifying the new user’s properties.  
**Throws:** Assertion failures if no user is created or the returned object is null.

### `public async Task GetOrCreateUserAsync_WithNullLastName_CreatesUserWithoutLastName`
Ensures that calling `GetOrCreateUserAsync` with a `null` last name does not cause an error; the created user has a `null` or empty last name field.  
**Parameters:** None.  
**Returns:** A completed task once the null-handling behavior is validated.  
**Throws:** Assertion failures if the service throws or stores an unexpected value.

### `public async Task GetOrCreateUserAsync_WithExistingUserWithDifferentDetails_UpdatesUser`
Tests the upsert logic: when the Telegram ID matches an existing user but the provided first name, last name, or username differ, the service updates the existing record in place.  
**Parameters:** None.  
**Returns:** A completed task after confirming the updated fields.  
**Throws:** Assertion failures if the old values remain unchanged.

### `public async Task GetUserByIdAsync_WithExistingUser_ReturnsUser`
Validates that `GetUserByIdAsync` returns the correct user object when the given ID exists in the data store.  
**Parameters:** None.  
**Returns:** A completed task after asserting the returned user is non-null and matches the expected entity.  
**Throws:** Assertion failures if the result is null or mismatched.

### `public async Task GetUserByIdAsync_WithNonExistingUser_ReturnsNull`
Checks that querying by an ID that does not exist yields `null` rather than throwing an exception.  
**Parameters:** None.  
**Returns:** A completed task once the null return is confirmed.  
**Throws:** Assertion failures if the method throws or returns a non-null object.

### `public async Task RecordUserActivityAsync_UpdatesLastActivityAndIncrementsMessagesCount`
Ensures that recording activity for an existing user updates the `LastActivity` timestamp and increments the `MessagesCount` by one.  
**Parameters:** None.  
**Returns:** A completed task after verifying both property changes.  
**Throws:** Assertion failures if the timestamp is stale or the count is not incremented.

### `public async Task RecordUserActivityAsync_WithNonExistingUser_DoesNotThrow`
Verifies that calling `RecordUserActivityAsync` for a Telegram ID not in the store completes without throwing an exception (no-op or graceful handling).  
**Parameters:** None.  
**Returns:** A completed task once the call succeeds without error.  
**Throws:** Assertion failures if an exception propagates.

### `public async Task UpdateUserAsync_UpdatesUserProperties`
Confirms that `UpdateUserAsync` applies all supplied property changes to an existing user (e.g., first name, last name, username, status).  
**Parameters:** None.  
**Returns:** A completed task after reading back the user and asserting the new values.  
**Throws:** Assertion failures if any field remains unchanged.

### `public async Task UpdateUserAsync_WithPartialUpdates_PreservesUnchangedValues`
Ensures that when only a subset of fields is provided in the update, the other existing values are left intact.  
**Parameters:** None.  
**Returns:** A completed task after verifying that omitted fields retain their original values.  
**Throws:** Assertion failures if untouched properties are overwritten with defaults.

### `public async Task DeleteUserAsync_WithExistingUser_DeletesAndReturnsTrue`
Tests that deleting an existing user removes the record from the store and returns `true`.  
**Parameters:** None.  
**Returns:** A completed task after confirming the user can no longer be retrieved.  
**Throws:** Assertion failures if the return value is `false` or the user still exists.

### `public async Task DeleteUserAsync_WithNonExistingUser_ReturnsFalse`
Verifies that attempting to delete a non-existent user returns `false` and does not alter the store.  
**Parameters:** None.  
**Returns:** A completed task once the false return is asserted.  
**Throws:** Assertion failures if the method throws or returns `true`.

### `public async Task SearchUsersAsync_FiltersByFirstName`
Validates that `SearchUsersAsync` with a partial or full first name query returns only users whose first name matches the filter.  
**Parameters:** None.  
**Returns:** A completed task after asserting the result set contains the expected subset.  
**Throws:** Assertion failures if non-matching users appear or matching users are omitted.

### `public async Task SearchUsersAsync_WithEmptyQuery_ReturnsAllUsers`
Ensures that an empty or whitespace-only search query returns the complete user list.  
**Parameters:** None.  
**Returns:** A completed task after comparing the result count to the total user count.  
**Throws:** Assertion failures if the result set is filtered unexpectedly.

### `public async Task GetUsersByStatusAsync_ReturnsFilteredUsers`
Confirms that `GetUsersByStatusAsync` returns only users whose status matches the specified value (e.g., active, blocked).  
**Parameters:** None.  
**Returns:** A completed task after verifying the filtered collection.  
**Throws:** Assertion failures if users with a different status are included.

## Usage

```csharp
// Example 1: Typical test arrangement using an in-memory collection
[Fact]
public async Task GetOrCreateUserAsync_WithExistingUser_ReturnsExistingUser()
{
    // Arrange
    var existingUser = new User { Id = 1, TelegramId = 12345, FirstName = "Alice" };
    var store = new List<User> { existingUser };
    var service = new UserService(store);

    // Act
    var result = await service.GetOrCreateUserAsync(12345, "Alice", "Smith", "alice_smith");

    // Assert
    Assert.NotNull(result);
    Assert.Equal(existingUser.Id, result.Id);
    Assert.Equal(1, store.Count); // No duplicate created
}
```

```csharp
// Example 2: Verifying update behavior with partial data
[Fact]
public async Task UpdateUserAsync_WithPartialUpdates_PreservesUnchangedValues()
{
    // Arrange
    var original = new User
    {
        Id = 10,
        TelegramId = 999,
        FirstName = "Bob",
        LastName = "Jones",
        Username = "bjones",
        Status = UserStatus.Active
    };
    var store = new List<User> { original };
    var service = new UserService(store);

    // Act
    await service.UpdateUserAsync(999, new UserUpdate { LastName = "Johnson" });

    // Assert
    var updated = store.Single(u => u.TelegramId == 999);
    Assert.Equal("Bob", updated.FirstName);      // Unchanged
    Assert.Equal("Johnson", updated.LastName);   // Changed
    Assert.Equal("bjones", updated.Username);    // Unchanged
    Assert.Equal(UserStatus.Active, updated.Status);
}
```

## Notes

- **Edge cases:** Tests explicitly cover null last names, non-existent IDs, empty search queries, and partial updates. The `RecordUserActivityAsync` method is expected to handle missing users without throwing, implying a defensive implementation (no-op or logged warning).
- **Thread safety:** These tests are designed for sequential execution against a single service instance. They do not validate concurrent access patterns. If the underlying `UserService` is intended for multi-threaded scenarios (e.g., multiple Telegram bot updates arriving simultaneously), additional concurrency tests should be added separately.
- **Data store independence:** The test signatures do not reveal whether the backing store is a real database, an in-memory list, or a mock. The suite can be adapted to any of these by changing the fixture setup in the constructor.
- **Idempotency:** `GetOrCreateUserAsync` with changed details updates the existing record rather than creating a second entry, confirming upsert semantics.
