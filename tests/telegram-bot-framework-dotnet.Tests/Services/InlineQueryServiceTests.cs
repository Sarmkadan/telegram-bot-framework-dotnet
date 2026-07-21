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

public class InlineQueryServiceTests
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
            It.Is<string>(key => key.StartsWith("inline_query_")),
            It.IsAny<Func<Task<IList<InlineQueryResult>>>>(),
            It.IsAny<TimeSpan?>()),
        Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyOffset_ReturnsFirstPage()
    {
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
            pageSize: 10
        );

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
        result.NextOffset.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_WithInvalidOffset_ReturnsFirstPage()
    {
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
            pageSize: 10
        );

        // Assert
        result.Should().NotBeNull();
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_WithMultiplePages_ReturnsCorrectPage()
    {
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
            pageSize: 10
        );

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(5); // 25 total, page 3 with 10 per page = 5 items (21-25)
        result.PageNumber.Should().Be(3);
        result.TotalCount.Should().Be(25);
        result.NextOffset.Should().BeEmpty(); // Last page
    }

    [Fact]
    public async Task GetCachedAsync_WithCachedResults_ReturnsPagedResults()
    {
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
    }

    [Fact]
    public async Task GetCachedAsync_WithPageNumber_ReturnsCorrectPage()
    {
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
    }

    [Fact]
    public async Task GetCachedAsync_WithoutCachedResults_ReturnsNull()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync<IList<InlineQueryResult>>(It.IsAny<string>()))
            .ReturnsAsync((IList<InlineQueryResult>?)null);

        // Act
        var result = await _service.GetCachedAsync("nonexistent query");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateCacheAsync_RemovesCachedEntry()
    {
        // Arrange
        var queryText = "query to invalidate";

        // Act
        await _service.InvalidateCacheAsync(queryText);

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync(
            It.Is<string>(key => key == $"inline_query_{queryText.ToLowerInvariant().Trim()}")),
        Times.Once);
    }

    [Fact]
    public async Task RecordQueryAsync_DoesNotThrow()
    {
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
    }

    [Fact]
    public async Task HandleAsync_WithEmptyQueryString_ProcessesSuccessfully()
    {
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
            pageSize: 10
        );

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.NextOffset.Should().BeEmpty();
    }

    [Fact]
    public void AddInlineQueryHandling_WithNullServices_Throws()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddInlineQueryHandling();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddInlineQueryHandlingWithLocalCache_WithNullServices_Throws()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act
        Action act = () => services.AddInlineQueryHandlingWithLocalCache();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

}
