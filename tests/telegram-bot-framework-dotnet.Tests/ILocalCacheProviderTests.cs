#nullable enable
using System.Threading.Tasks;

namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface defining the contract for LocalCacheProvider test methods.
/// </summary>
public interface ILocalCacheProviderTests
{
    Task SetAsync_ThenGetAsync_ReturnsStoredValue();
    Task GetAsync_WhenKeyDoesNotExist_ReturnsDefault();
    Task GetAsync_WhenEntryHasExpired_ReturnsDefault();
    Task GetAsync_WhenEntryNotExpired_ReturnsValue();
    Task RemoveAsync_ExistingKey_MakesValueUnavailable();
    Task ExistsAsync_WhenKeyPresent_ReturnsTrue();
    Task ExistsAsync_WhenKeyNotPresent_ReturnsFalse();
    Task ExistsAsync_WhenEntryExpired_ReturnsFalse();
    Task GetOrCreateAsync_WhenKeyMissing_InvokesFactoryAndPersistsResult();
    Task GetOrCreateAsync_WhenKeyExists_SkipsFactoryAndReturnsCached();
    Task FlushAsync_ClearsAllCachedEntries();
    Task GetStatisticsAsync_TracksCacheHitsAndMisses();
    Task GetAsync_WithNullKey_ReturnsDefaultValue();
    Task GetAsync_WithEmptyKey_ReturnsDefaultValue();
    Task GetAsync_WithWhitespaceKey_ReturnsDefaultValue();
    Task SetAsync_WithNullKey_DoesNotThrow();
    Task SetAsync_WithEmptyKey_DoesNotThrow();
    Task SetAsync_WithWhitespaceKey_DoesNotThrow();
    Task SetAsync_WithValidKey_StoresValue();
    Task SetAsync_WithExpiration_StoresValueWithExpiration();
}