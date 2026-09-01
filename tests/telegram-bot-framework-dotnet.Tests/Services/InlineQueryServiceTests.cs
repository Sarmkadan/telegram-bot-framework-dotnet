#nullable enable

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TelegramBotFramework.Caching;
using TelegramBotFramework.Models;
using TelegramBotFramework.Services;
using Xunit;

namespace TelegramBotFramework.Tests.Services;

public class InlineQueryServiceTests : IInlineQueryServiceTests
{
    private readonly Mock<ICacheProvider> _cacheMock = new();
    private readonly Mock<ILogger<InlineQueryService>> _loggerMock = new();
    private readonly IInlineQueryService _service;

    public InlineQueryServiceTests()
    {
        _service = new InlineQueryService(_cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidQuery_ReturnsPagedResults()
    {
        _loggerMock.Object.LogInformation("Starting valid inline query test for {QueryId} with page size {PageSize}", "test-query-123", 2);

        // Arrange
        var query = new InlineQuery
        {
            QueryId = "test-query-123",
            Query = "test search",
            UserId = 456,
            Offset = "1"
        };

        var results = new List<InlineQueryResult>
        {
            new InlineQueryResult { Title = "Result 1", Content = "Content 1" },
            new InlineQueryResult { Title = "Result 2", Content = "Content 2" },
            new InlineQueryResult { Title = "Result 3", Content = "Content 3" }
        };

        _cacheMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<IList<InlineQueryResult>>>>(),
            It.IsAny<TimeSpan?>()))
        .ReturnsAsync(results);

        // Act
        var result = await _service.HandleAsync(
            query,
            (q, ct) => Task.FromResult<IList<InlineQueryResult>>(results),
            pageSize: 2
        );

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.NextOffset.Should().Be("2");

        _cacheMock.Verify(c => c.GetOrCreateAsync(
            It.Is<string>(key => key.StartsWith(InlineQueryServiceTestsConstants.InlineQueryCacheKeyPrefix)),
            It.IsAny<Func<Task<IList<InlineQueryResult>>>>(),
            It.IsAny<TimeSpan?>()),
        Times.Once);

        _loggerMock.Object.LogInformation("Completed valid inline query test for {QueryId} with {ResultCount} results", query.QueryId, result.Results.Count);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyOffset_ReturnsFirstPage()
    {
        _loggerMock.Object.LogInformation("Starting empty offset inline query test for {QueryId}", "test-query-456");

        // Arrange
        var query = new InlineQuery
        {
            QueryId = "test-query-456",
            Query = "another search",
            UserId = 789,
            Offset = ""
        };

        var results = new List<InlineQueryResult>
        {
            new InlineQueryResult { Title = "First", Content = "First content" },
            new InlineQueryResult { Title = "Second", Content = "Second content" }
        };

        _cacheMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<IList<InlineQueryResult>>>>(),
            It.IsAny<TimeSpan?>()))
        .ReturnsAsync(results);

        // Act
        var result = await _service.HandleAsync(
            query,
            (q, ct) => Task.FromResult<IList<InlineQueryResult>>(results),
            pageSize: InlineQueryServiceTestsConstants.DefaultPageSize
        );

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.NextOffset.Should().BeEmpty();

        _loggerMock.Object.LogInformation("Completed empty offset inline query test for {QueryId} on page {PageNumber}", query.QueryId, result.PageNumber);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidOffset_ReturnsFirstPage()
    {
        _loggerMock.Object.LogInformation("Starting invalid offset inline query test for {QueryId} with offset {Offset}", "test-query-789", "invalid");
        _loggerMock.Object.LogWarning("Testing fallback to the first page for invalid offset {Offset}", "invalid");

        // Arrange
        var query = new InlineQuery
        {
            QueryId = "test-query-789",
            Query = "search with bad offset",
            UserId = 123,
            Offset = "invalid"
        };

        var results = new List<InlineQueryResult>
        {
            new InlineQueryResult { Title = "Result", Content = "Content" }
        };

        _cacheMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<IList<InlineQueryResult>>>>(),
            It.IsAny<TimeSpan?>()))
        .ReturnsAsync(results);

        // Act
        var result = await _service.HandleAsync(
            query,
            (q, ct) => Task.FromResult<IList<InlineQueryResult>>(results),
            pageSize: InlineQueryServiceTestsConstants.DefaultPageSize
        );

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);

        _loggerMock.Object.LogInformation("Completed invalid offset inline query test for {QueryId} on fallback page {PageNumber}", query.QueryId, result.PageNumber);
    }

    [Fact]
    public async Task HandleAsync_WithMultiplePages_ReturnsCorrectPage()
    {
        _loggerMock.Object.LogInformation("Starting multi-page inline query test for {QueryId} with offset {Offset}", "test-query-multi", "3");

        // Arrange
        var query = new InlineQuery
        {
            QueryId = "test-query-multi",
            Query = "multi page",
            UserId = 999,
            Offset = "3"
        };

        var allResults = Enumerable.Range(1, 25)
            .Select(i => new InlineQueryResult { Title = $"Result {i}", Content = $"Content {i}" })
            .ToList();

        _cacheMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<IList<InlineQueryResult>>>>(),
            It.IsAny<TimeSpan?>()))
        .ReturnsAsync(allResults);

        // Act
        var result = await _service.HandleAsync(
            query,
            (q, ct) => Task.FromResult<IList<InlineQueryResult>>(allResults),
            pageSize: InlineQueryServiceTestsConstants.DefaultPageSize
        );

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(5); // 25 total, page 3 with 10 per page = 5 items (21-25)
        result.PageNumber.Should().Be(3);
        result.TotalCount.Should().Be(25);
        result.NextOffset.Should().BeEmpty(); // Last page

        _loggerMock.Object.LogInformation("Completed multi-page inline query test for {QueryId} on page {PageNumber} with {ResultCount} results", query.QueryId, result.PageNumber, result.Results.Count);
    }

    [Fact]
    public async Task GetCachedAsync_WithCachedResults_ReturnsPagedResults()
    {
        _loggerMock.Object.LogInformation("Starting cached inline query test for {QueryText} on page {PageNumber}", "cached query", 1);

        // Arrange
        var cachedResults = new List<InlineQueryResult>
        {
            new InlineQueryResult { Title = "Cached 1", Content = "Cached content 1" },
            new InlineQueryResult { Title = "Cached 2", Content = "Cached content 2" },
            new InlineQueryResult { Title = "Cached 3", Content = "Cached content 3" }
        };

        _cacheMock.Setup(c => c.GetAsync<IList<InlineQueryResult>>(It.IsAny<string>()))
            .ReturnsAsync(cachedResults);

        // Act
        var result = await _service.GetCachedAsync("cached query", pageNumber: 1);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);

        _loggerMock.Object.LogInformation("Completed cached inline query test for {QueryText} with {ResultCount} results", "cached query", result.Results.Count);
    }

    [Fact]
    public async Task GetCachedAsync_WithPageNumber_ReturnsCorrectPage()
    {
        _loggerMock.Object.LogInformation("Starting paginated cache test for {QueryText} on page {PageNumber}", "paginated query", 2);

        // Arrange
        var cachedResults = Enumerable.Range(1, 20)
            .Select(i => new InlineQueryResult { Title = $"Item {i}", Content = $"Item content {i}" })
            .ToList();

        _cacheMock.Setup(c => c.GetAsync<IList<InlineQueryResult>>(It.IsAny<string>()))
            .ReturnsAsync(cachedResults);

        // Act
        var result = await _service.GetCachedAsync("paginated query", pageNumber: 2);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(10); // DefaultPageSize is 10, so page 2 has 10 results
        result.PageNumber.Should().Be(2);
        result.NextOffset.Should().BeEmpty(); // Page 2 is the last page for 20 items with 10 per page

        _loggerMock.Object.LogInformation("Completed paginated cache test for {QueryText} on page {PageNumber} with {ResultCount} results", "paginated query", result.PageNumber, result.Results.Count);
    }

    [Fact]
    public async Task GetCachedAsync_WithoutCachedResults_ReturnsNull()
    {
        _loggerMock.Object.LogInformation("Starting cache miss test for {QueryText}", "nonexistent query");
        _loggerMock.Object.LogWarning("Testing degraded cache-miss path for {QueryText}", "nonexistent query");

        // Arrange
        _cacheMock.Setup(c => c.GetAsync<IList<InlineQueryResult>>(It.IsAny<string>()))
            .ReturnsAsync((IList<InlineQueryResult>?)null);

        // Act
        var result = await _service.GetCachedAsync("nonexistent query");

        // Assert
        result.Should().BeNull();

        _loggerMock.Object.LogInformation("Completed cache miss test for {QueryText}", "nonexistent query");
    }

    [Fact]
    public async Task InvalidateCacheAsync_RemovesCachedEntry()
    {
        _loggerMock.Object.LogInformation("Starting cache invalidation test for {QueryText}", "query to invalidate");

        // Arrange
        var queryText = "query to invalidate";

        // Act
        await _service.InvalidateCacheAsync(queryText);

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync(
            It.Is<string>(key => key == $"{InlineQueryServiceTestsConstants.InlineQueryCacheKeyPrefix}{queryText.ToLowerInvariant().Trim()}")),
        Times.Once);

        _loggerMock.Object.LogInformation("Completed cache invalidation test for {QueryText}", queryText);
    }

    [Fact]
    public async Task RecordQueryAsync_DoesNotThrow()
    {
        _loggerMock.Object.LogInformation("Starting query recording test for {QueryId} with result count {ResultCount}", "log-test-123", 5);

        // Arrange
        var query = new InlineQuery
        {
            QueryId = "log-test-123",
            Query = "log this query",
            UserId = 111
        };

        // Act
        var act = () => _service.RecordQueryAsync(query, resultCount: 5);

        // Assert
        await act.Should().NotThrowAsync();

        _loggerMock.Object.LogInformation("Completed query recording test for {QueryId} with result count {ResultCount}", query.QueryId, 5);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyQueryString_ProcessesSuccessfully()
    {
        _loggerMock.Object.LogInformation("Starting empty query string test for {QueryId}", "empty-query-test");
        _loggerMock.Object.LogWarning("Testing degraded processing path for empty query text on {QueryId}", "empty-query-test");

        // Arrange
        var query = new InlineQuery
        {
            QueryId = "empty-query-test",
            Query = "",
            UserId = 222,
            Offset = ""
        };

        var results = new List<InlineQueryResult>();

        _cacheMock.Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<Task<IList<InlineQueryResult>>>>(),
            It.IsAny<TimeSpan?>()))
        .ReturnsAsync(results);

        // Act
        var result = await _service.HandleAsync(
            query,
            (q, ct) => Task.FromResult<IList<InlineQueryResult>>(results),
            pageSize: InlineQueryServiceTestsConstants.DefaultPageSize
        );

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.NextOffset.Should().BeEmpty();

        _loggerMock.Object.LogInformation("Completed empty query string test for {QueryId} with {ResultCount} results", query.QueryId, result.Results.Count);
    }

    [Fact]
    public void AddInlineQueryHandling_WithNullServices_Throws()
    {
        _loggerMock.Object.LogInformation("Starting service registration validation test for {RegistrationMethod}", nameof(InlineQueryExtensions.AddInlineQueryHandling));
        _loggerMock.Object.LogWarning("Testing fallback validation for null services in {RegistrationMethod}", nameof(InlineQueryExtensions.AddInlineQueryHandling));

        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddInlineQueryHandling();

        // Assert
        act.Should().Throw<ArgumentNullException>();

        _loggerMock.Object.LogInformation("Completed service registration validation test for {RegistrationMethod}", nameof(InlineQueryExtensions.AddInlineQueryHandling));
    }

    [Fact]
    public void AddInlineQueryHandlingWithLocalCache_WithNullServices_Throws()
    {
        _loggerMock.Object.LogInformation("Starting service registration validation test for {RegistrationMethod}", nameof(InlineQueryExtensions.AddInlineQueryHandlingWithLocalCache));
        _loggerMock.Object.LogWarning("Testing fallback validation for null services in {RegistrationMethod}", nameof(InlineQueryExtensions.AddInlineQueryHandlingWithLocalCache));

        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddInlineQueryHandlingWithLocalCache();

        // Assert
        act.Should().Throw<ArgumentNullException>();

        _loggerMock.Object.LogInformation("Completed service registration validation test for {RegistrationMethod}", nameof(InlineQueryExtensions.AddInlineQueryHandlingWithLocalCache));
    }

}
