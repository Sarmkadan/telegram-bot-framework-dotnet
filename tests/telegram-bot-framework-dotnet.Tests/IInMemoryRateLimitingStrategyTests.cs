#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Interface for InMemoryRateLimitingStrategyTests
// =============================================================================

using System.Threading.Tasks;

namespace TelegramBotFramework.Strategies.Tests;

/// <summary>
/// Interface for tests of the InMemoryRateLimitingStrategy class.
/// </summary>
public interface IInMemoryRateLimitingStrategyTests
{
    void IsRequestAllowed_RequestsAtWindowBoundaryAreExpired();
    void IsRequestAllowed_RequestsInsideWindowAreNotExpired();
    void IsRequestAllowed_RequestsOutsideWindowAreExpired();
    void GetRemainingRequests_HandlesBoundaryConditionsCorrectly();
    Task IsActionAllowedAsync_HandlesBoundaryConditionsCorrectly();
    void IsRequestAllowed_DifferentIdentifiersLimitedIndependently();
}