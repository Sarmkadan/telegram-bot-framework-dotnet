namespace TelegramBotFramework.Tests;

public interface IInlineKeyboardBuilderTests
{
    void Build_WithSingleCallbackButton_CreatesOneRowOneButton();
    Task Build_WithSingleCallbackButton_CreatesOneRowOneButtonAsync(CancellationToken cancellationToken = default);
    void Build_WithUrlButton_SetsTypeAndUrl();
    Task Build_WithUrlButton_SetsTypeAndUrlAsync(CancellationToken cancellationToken = default);
    void Build_WithSwitchInlineButton_SetsTypeAndQuery();
    Task Build_WithSwitchInlineButton_SetsTypeAndQueryAsync(CancellationToken cancellationToken = default);
    void Build_AutoWrapsButtonsAtMaxPerRow();
    Task Build_AutoWrapsButtonsAtMaxPerRowAsync(CancellationToken cancellationToken = default);
    void NewRow_ForcesRowBreakBeforeMaxReached();
    Task NewRow_ForcesRowBreakBeforeMaxReachedAsync(CancellationToken cancellationToken = default);
    void ToButtonLabels_ReturnsTwoDimensionalLabelArray();
    Task ToButtonLabels_ReturnsTwoDimensionalLabelArrayAsync(CancellationToken cancellationToken = default);
    void ToMenu_ConvertsMarkupToMenuModel();
    Task ToMenu_ConvertsMarkupToMenuModelAsync(CancellationToken cancellationToken = default);
    void Build_WithNoButtons_ThrowsInvalidOperationException();
    Task Build_WithNoButtons_ThrowsInvalidOperationExceptionAsync(CancellationToken cancellationToken = default);
    void AddButton_WithCallbackDataExceeding64Bytes_ThrowsArgumentException();
    Task AddButton_WithCallbackDataExceeding64Bytes_ThrowsArgumentExceptionAsync(CancellationToken cancellationToken = default);
    void AddButton_WithEmptyText_ThrowsArgumentException();
    Task AddButton_WithEmptyText_ThrowsArgumentExceptionAsync(CancellationToken cancellationToken = default);
}