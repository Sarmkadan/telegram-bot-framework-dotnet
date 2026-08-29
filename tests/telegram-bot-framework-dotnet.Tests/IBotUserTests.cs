#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FluentAssertions;
using TelegramBotFramework.Models;
using TelegramBotFramework.Repositories;
using Xunit;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface for unit tests of the <see cref="BotUser"/> class.
/// </summary>
public interface IBotUserTests
{
    void GetDisplayName_WithFirstAndLastName_ReturnsFullName();
    void GetDisplayName_WithoutLastName_ReturnsFirstNameOnly();
    void Validate_WithNonPositiveTelegramId_ThrowsInvalidOperationException();
    void Validate_WithEmptyFirstName_ThrowsInvalidOperationException();
    void UpdateActivity_IncrementsMessagesCount();
    void SetMetadata_AndGetMetadata_RoundTripsValue();
    void GetMetadata_WhenKeyNotPresent_ReturnsNull();
    void SetMetadata_OverwritesExistingKey();
}