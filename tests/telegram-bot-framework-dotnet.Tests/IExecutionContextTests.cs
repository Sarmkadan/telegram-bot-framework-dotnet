using System;
using TelegramBotFramework.Models;

public interface IExecutionContextTests
{
    void Constructor_WithDefaultValues_InitializesCorrectly();
    void Constructor_WithUserAndSession_StoresReferences();
    void AddError_AddsErrorToErrorsList();
    void AddError_WithNullError_DoesNotAdd();
    void AddError_WithEmptyError_DoesNotAdd();
    void SetState_AddsStateToStatesDictionary();
    void SetState_OverwritesExistingState();
    void SetState_WithNullKey_DoesNotAdd();
    void SetState_WithEmptyKey_DoesNotAdd();
    void GetState_WithExistingKey_ReturnsValue();
    void GetState_WithNonExistingKey_ReturnsDefault();
    void GetState_WithWrongType_ReturnsDefault();
    void Validate_WithValidContext_ReturnsTrue();
    void Validate_WithNullUser_StillReturnsTrue();
    void Validate_WithNullSession_StillReturnsTrue();
    void Validate_WithNullMessage_StillReturnsTrue();
    void Validate_WithZeroUserId_AddsErrorAndReturnsFalse();
    void Validate_WithZeroChatId_AddsErrorAndReturnsFalse();
    void StopProcessing_SetsIsStoppedToTrue();
    void GetDuration_ReturnsTimeSpanSinceCreation();
}