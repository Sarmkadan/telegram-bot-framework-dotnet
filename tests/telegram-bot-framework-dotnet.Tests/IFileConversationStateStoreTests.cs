using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramBotFramework.ConversationFlow;

public interface IFileConversationStateStoreTests
{
    void Dispose();
    Task SaveStateAsync_LoadStateAsync_Roundtrip_ReturnsSameState();
    Task LoadStateAsync_MissingState_ReturnsNull();
    Task DeleteStateAsync_RemovesFile();
    Task LoadStateAsync_CorruptedFile_DeletesFileAndReturnsNull();
    Task LoadStateAsync_EmptyFile_DeletesFileAndReturnsNull();
    Task LoadStateAsync_InvalidStructure_DeletesFileAndReturnsNull();
    Task LoadAllActiveStatesAsync_ReturnsOnlyActiveStates();
    Task LoadAllActiveStatesAsync_NoFiles_ReturnsEmptyList();
    Task SaveStateAsync_NullState_ThrowsArgumentNullException();
    void Constructor_InvalidDirectory_ThrowsArgumentException(string? invalidDirectory);
    void GetFilePath_ReturnsCorrectPath();
    void Dispose_MultipleTimes_DoesNotThrow();
}