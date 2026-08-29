namespace TelegramBotFramework.Tests.Services
{
    public interface IInlineQueryServiceTests
    {
        Task HandleAsync_WithValidQuery_ReturnsPagedResults();
        Task HandleAsync_WithEmptyOffset_ReturnsFirstPage();
        Task HandleAsync_WithInvalidOffset_ReturnsFirstPage();
        Task HandleAsync_WithMultiplePages_ReturnsCorrectPage();
        Task GetCachedAsync_WithCachedResults_ReturnsPagedResults();
        Task GetCachedAsync_WithPageNumber_ReturnsCorrectPage();
        Task GetCachedAsync_WithoutCachedResults_ReturnsNull();
        Task InvalidateCacheAsync_RemovesCachedEntry();
        Task RecordQueryAsync_DoesNotThrow();
        Task HandleAsync_WithEmptyQueryString_ProcessesSuccessfully();
        void AddInlineQueryHandling_WithNullServices_Throws();
        void AddInlineQueryHandlingWithLocalCache_WithNullServices_Throws();
    }
}