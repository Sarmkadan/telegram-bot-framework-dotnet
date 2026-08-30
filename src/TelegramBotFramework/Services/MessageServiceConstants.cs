namespace TelegramBotFramework.Services;

/// <summary>
/// Constants for MessageService.
/// </summary>
internal static class MessageServiceConstants
{
    // Default limits
    public const int DefaultUserMessageLimit = 50;
    public const int DefaultFailedMessageLimit = 100;
    public const int DefaultArchiveDaysOld = 30;

    // Poll constraints
    public const int MaxPollQuestionLength = 256;
    public const int MinPollOptionsCount = 2;
    public const int MaxPollOptionsCount = 10;
    public const int MaxPollOptionLength = 100;

    // Media group constraints
    public const int MinMediaGroupItemsCount = 2;
    public const int MaxMediaGroupItemsCount = 10;

    // Metadata keys for poll messages
    public const string PollTypeMetadataKey = "poll_type";
    public const string PollOptionsMetadataKey = "options";
    public const string PollAllowsMultipleAnswersMetadataKey = "allows_multiple_answers";
    public const string PollMessageIdMetadataKey = "message_id";
    public const string PollTypeQuizValue = "quiz";

    // Metadata keys for media group messages
    public const string MediaTypeMetadataKey = "media_type";
    public const string FileIdOrUrlMetadataKey = "file_id_or_url";
    public const string PositionMetadataKey = "position";
    public const string CaptionMetadataKey = "caption";
    public const string MessageIdMetadataKey = "message_id";
}