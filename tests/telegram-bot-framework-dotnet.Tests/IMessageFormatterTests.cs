namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface for MessageFormatterTests.
/// </summary>
public interface IMessageFormatterTests
{
    void EscapeMarkdown_EscapesAllSpecialCharacters();
    void EscapeHtml_EscapesAllSpecialCharacters();
    void FormatAsPlainText_IncludesTimestampAndEditedFlag();
    void FormatAsMarkdown_ProducesCorrectTemplate();
    void TruncateForPreview_LongMessage_IsTruncatedWithEllipsis();
    void FormatAsConversation_MarkdownTrue_CombinesMultipleMessages();
    void FormatAsPlainText_EmptyContent_ProducesLinesWithoutException();
}