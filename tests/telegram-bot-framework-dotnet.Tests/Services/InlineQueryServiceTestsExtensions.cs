namespace TelegramBotFramework.Tests.Services;

using System;

/// <summary>
/// Extension methods for <see cref="InlineQueryServiceTests"/> to provide useful test helpers.
/// </summary>
public static class InlineQueryServiceTestsExtensions
{
    /// <summary>
    /// Attempts to run all HandleAsync test methods and returns a tuple indicating which passed.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>
    /// A tuple where each element indicates whether the corresponding test passed:
    /// Item1: HandleAsync_WithValidQuery_ReturnsPagedResults
    /// Item2: HandleAsync_WithEmptyOffset_ReturnsFirstPage
    /// Item3: HandleAsync_WithInvalidOffset_ReturnsFirstPage
    /// Item4: HandleAsync_WithMultiplePages_ReturnsCorrectPage
    /// Item5: HandleAsync_WithEmptyQueryString_ProcessesSuccessfully
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static async Task<bool> TryHandleAsync_WithValidQuery_ReturnsPagedResults(this InlineQueryServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);
        try
        {
            await tests.HandleAsync_WithValidQuery_ReturnsPagedResults();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to run all HandleAsync test methods and returns a tuple indicating which passed.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>
    /// A tuple where each element indicates whether the corresponding test passed:
    /// Item1: HandleAsync_WithValidQuery_ReturnsPagedResults
    /// Item2: HandleAsync_WithEmptyOffset_ReturnsFirstPage
    /// Item3: HandleAsync_WithInvalidOffset_ReturnsFirstPage
    /// Item4: HandleAsync_WithMultiplePages_ReturnsCorrectPage
    /// Item5: HandleAsync_WithEmptyQueryString_ProcessesSuccessfully
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static async Task<(bool validQuery, bool emptyOffset, bool invalidOffset, bool multiplePages, bool emptyQueryString)>
        TryAllHandleAsyncTests(this InlineQueryServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        bool validQuery = false;
        bool emptyOffset = false;
        bool invalidOffset = false;
        bool multiplePages = false;
        bool emptyQueryString = false;

        try
        {
            await tests.HandleAsync_WithValidQuery_ReturnsPagedResults();
            validQuery = true;
        }
        catch { }

        try
        {
            await tests.HandleAsync_WithEmptyOffset_ReturnsFirstPage();
            emptyOffset = true;
        }
        catch { }

        try
        {
            await tests.HandleAsync_WithInvalidOffset_ReturnsFirstPage();
            invalidOffset = true;
        }
        catch { }

        try
        {
            await tests.HandleAsync_WithMultiplePages_ReturnsCorrectPage();
            multiplePages = true;
        }
        catch { }

        try
        {
            await tests.HandleAsync_WithEmptyQueryString_ProcessesSuccessfully();
            emptyQueryString = true;
        }
        catch { }

        return (validQuery, emptyOffset, invalidOffset, multiplePages, emptyQueryString);
    }

    /// <summary>
    /// Attempts to run all GetCachedAsync test methods and returns a tuple indicating which passed.
    /// </summary>
    /// <param name="tests">The test instance.</param>
    /// <returns>
    /// A tuple where each element indicates whether the corresponding test passed:
    /// Item1: GetCachedAsync_WithCachedResults_ReturnsPagedResults
    /// Item2: GetCachedAsync_WithPageNumber_ReturnsCorrectPage
    /// Item3: GetCachedAsync_WithoutCachedResults_ReturnsNull
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static async Task<(bool cachedResults, bool pageNumber, bool withoutCachedResults)>
        TryAllGetCachedAsyncTests(this InlineQueryServiceTests tests)
    {
        ArgumentNullException.ThrowIfNull(tests);

        bool cachedResults = false;
        bool pageNumber = false;
        bool withoutCachedResults = false;

        try
        {
            await tests.GetCachedAsync_WithCachedResults_ReturnsPagedResults();
            cachedResults = true;
        }
        catch { }

        try
        {
            await tests.GetCachedAsync_WithPageNumber_ReturnsCorrectPage();
            pageNumber = true;
        }
        catch { }

        try
        {
            await tests.GetCachedAsync_WithoutCachedResults_ReturnsNull();
            withoutCachedResults = true;
        }
        catch { }

        return (cachedResults, pageNumber, withoutCachedResults);
    }
}