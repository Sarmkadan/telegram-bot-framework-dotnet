#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace TelegramBotFramework.ConversationFlow
{
    /// <summary>
    /// Constants for ConversationFlowEngine to avoid magic strings and numbers.
    /// </summary>
    internal static class ConversationFlowEngineConstants
    {
        // Logging messages
        public const string FlowRegisteredLog = "Flow registered — Id: {FlowId}, Name: {FlowName}, Steps: {StepCount}";
        public const string FlowUnregisteredLog = "Flow unregistered — Id: {FlowId}";
        public const string AbortingExistingFlowLog = "Aborting existing flow for UserId {UserId} before starting '{FlowId}'";
        public const string FlowStartedLog = "Flow started — UserId: {UserId}, FlowId: {FlowId}, StateId: {StateId}";
        public const string ValidationFailedLog = "Validation failed — UserId: {UserId}, Step: {StepId}, Error: {Error}";
        public const string FlowCompletedLog = "Flow completed — UserId: {UserId}, FlowId: {FlowId}, Steps: {StepCount}";
        public const string FlowCompletedPrompt = "Completed.";
        public const string FlowAdvancedLog = "Flow advanced — UserId: {UserId}, FlowId: {FlowId}, Step: {StepId} → {NextStepId}";
        public const string FlowResumedLog = "Flow resumed — UserId: {UserId}, FlowId: {FlowId}, Step: {StepId}";
        public const string FlowInProgressButStateMissingLog = "Flow '{FlowId}' was in-progress for UserId {UserId} but state is not in memory — restart the flow to continue.";
        public const string FlowResetToInitialStepLog = "Flow reset to initial step after timeout — UserId: {UserId}, FlowId: {FlowId}";
        public const string CleanupEvictionReason = "Inactivity timeout (cleanup sweep)";
        public const string OnEvictionCallbackErrorLog = "OnEviction callback threw for UserId: {UserId}, FlowId: {FlowId}";
        public const string CleanupProcessedLog = "Cleanup processed {Count} expired flow states";
        public const string UnhandledExceptionLog = "Unhandled exception during flow step processing — UserId: {UserId}, FlowId: {FlowId}, StepId: {StepId}. Flow has been terminated to prevent inconsistent state.";
        public const string SupersededByNewFlowReason = "Superseded by a new flow";

        // Format strings
        public const string GuidFormat = "N";

        // Default values
        public const int DefaultHistoryLimit = 10;
        public const int MinHistoryLimit = 1;
    }
}