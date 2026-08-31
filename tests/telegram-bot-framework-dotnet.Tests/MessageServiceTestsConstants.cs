#nullable enable
namespace TelegramBotFramework.Tests;

/// <summary>
/// Constants for MessageServiceTests to avoid magic values.
/// </summary>
internal static class MessageServiceTestsConstants
{
    // Test user and chat IDs
    public const long TestUserId = 12345;
    public const long TestChatId = 67890;
    public const long LargeChatId = 123456789;
    public const long InvalidId = 0;
    public const long NonExistingId = 999;

    // Test message IDs
    public const long TestMessageId = 1;
    public const long ExistingMessageId = 123;
    public const int CreatedMessageId = 100;
    public const int AnotherMessageId = 101;
    public const int TelegramPollMessageId = 42;

    // Test content
    public const string HelloWorldContent = "Hello world";
    public const string TestMessageContent = "Test message";
    public const string FirstMessageContent = "First message";
    public const string SecondMessageContent = "Second message";
    public const string ThirdMessageContent = "Third message";
    public const string FailedMessage1Content = "Failed message 1";
    public const string FailedMessage2Content = "Failed message 2";
    public const string OldMessageContent = "Old message";
    public const string RecentMessageContent = "Recent message";
    public const string TestQuestion = "Test question";
    public const string FavoriteColorQuestion = "What is your favorite color?";
    public const string PhotoAlbumCaption = "Photo album";
    public const string ErrorMessage = "Something went wrong";
    public const string ApiErrorMessage = "API error";
    public const string FirstPhotoCaption = "First photo";
    public const string SecondPhotoCaption = "Second photo";
    public const string PhotoMediaType = "photo";
    public const string QuizPollType = "quiz";
    public const string EmptyQuestion = "";
    public const string GeneratedOptionFormat = "Option {0}";
    public const string GeneratedPhotoFileFormat = "photo{0}.jpg";

    // Test options
    public static readonly string[] ColorOptions = { "Red", "Green", "Blue" };
    public static readonly string[] TwoOptions = { "Option 1", "Option 2" };

    // Test media items
    public static readonly string Photo1Url = "photo1.jpg";
    public static readonly string Photo2Url = "photo2.jpg";

    // Test numbers and counts
    public const int DaysOld = 30;
    public const int RecentMessageMinutesOld = 5;
    public const int OldestMessageMinutesOld = 10;
    public const int RecentMessageHoursOld = 1;
    public const int OlderThanCutoffDays = 1;
    public const int RequestedMessageCount = 2;
    public const int FailedMessageLimit = 5;
    public const int ExpectedFailedMessageCount = 2;
    public const int ExpectedMediaMessageCount = 2;
    public const int FirstPosition = 0;
    public const int SecondPosition = 1;
    public const int OneBasedIndexOffset = 1;
    public const int TooManyOptionsCount = 11;
    public const int TooManyMediaItemsCount = 11;
    public const int ProcessingMessageCount = 2;
    public const int ReceivedMessageCount = 3;
    public const int ExpectedUnprocessedCount = 5; // 2 processing + 3 received

    // Test message IDs for GetUnprocessedMessageCountAsync test
    public const long FirstMessageId = 1;
    public const long SecondMessageId = 2;
    public const long ThirdMessageId = 3;
    public const long FourthMessageId = 4;
    public const long FifthMessageId = 5;

    // Log message patterns
    public const string LogMessageReceived = "Message received from user";
    public const string LogMessageProcessed = "Message marked as processed";
    public const string LogMessageFailed = "Message marked as failed";
    public const string LogArchived = "Archived";
    public const string LogPollSent = "Poll sent to chat";
    public const string LogPollSendFailed = "Failed to send poll to chat";
    public const string LogPollSendError = "Error sending poll to chat";
    public const string LogMediaGroupSent = "Media group sent to chat";
    public const string LogMediaGroupSendFailed = "Failed to send media group to chat";
    public const string LogMediaGroupSendError = "Error sending media group to chat";

    // Metadata keys
    public const string PollTypeMetadataKey = "poll_type";
    public const string PollOptionsMetadataKey = "options";
    public const string PollAllowsMultipleAnswersMetadataKey = "allows_multiple_answers";
    public const string PollMessageIdMetadataKey = "message_id";
    public const string MediaTypeMetadataKey = "media_type";
    public const string FileIdOrUrlMetadataKey = "file_id_or_url";
    public const string MediaPositionMetadataKey = "position";
    public const string MediaCaptionMetadataKey = "caption";
    public const string ErrorMetadataKey = "error";
}
