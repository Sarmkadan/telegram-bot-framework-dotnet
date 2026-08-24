using System;
using System.Collections.Generic;
using System.Linq;
using TelegramBotFramework.Services;
using TelegramBotFramework.Models;
using TelegramBotFramework.Integration;
using Moq;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

namespace TelegramBotFramework.Tests
{
    public class BroadcastServiceTestsValidation
    {
        public IReadOnlyList<string> Validate(BroadcastServiceTests value)
        {
            var errors = new List<string>();

            if (value == null)
            {
                errors.Add("Value is null");
            }

            if (value.BroadcastAsync_WithEmptyChatIds_ReturnsSuccessWithNoChats == null)
            {
                errors.Add("BroadcastAsync_WithEmptyChatIds_ReturnsSuccessWithNoChats is null");
            }

            if (value.BroadcastAsync_WithValidChats_SendsMessagesToAllChats == null)
            {
                errors.Add("BroadcastAsync_WithValidChats_SendsMessagesToAllChats is null");
            }

            if (value.BroadcastAsync_WithFailedMessages_CollectsFailures == null)
            {
                errors.Add("BroadcastAsync_WithFailedMessages_CollectsFailures is null");
            }

            if (value.BroadcastAsync_WithContinueOnErrorFalse_ThrowsOnFirstError == null)
            {
                errors.Add("BroadcastAsync_WithContinueOnErrorFalse_ThrowsOnFirstError is null");
            }

            if (value.BroadcastAsync_WithRateLimit_RespectsMessagesPerSecond == null)
            {
                errors.Add("BroadcastAsync_WithRateLimit_RespectsMessagesPerSecond is null");
            }

            if (value.BroadcastAsync_WithProgressCallback_CallsCallback == null)
            {
                errors.Add("BroadcastAsync_WithProgressCallback_CallsCallback is null");
            }

            if (value.BroadcastAsync_WithCancellation_CancelsOperation == null)
            {
                errors.Add("BroadcastAsync_WithCancellation_CancelsOperation is null");
            }

            if (value.BroadcastToUsersAsync_ConvertsUsersToChatIds == null)
            {
                errors.Add("BroadcastToUsersAsync_ConvertsUsersToChatIds is null");
            }

            if (value.GetRateLimitStats_ReturnsStatistics == null)
            {
                errors.Add("GetRateLimitStats_ReturnsStatistics is null");
            }

            if (value.BroadcastAsync_WithMessageFormatter_AppliesFormatter == null)
            {
                errors.Add("BroadcastAsync_WithMessageFormatter_AppliesFormatter is null");
            }

            if (value.Dispose_DisposesResources == null)
            {
                errors.Add("Dispose_DisposesResources is null");
            }

            return errors;
        }

        public bool IsValid(BroadcastServiceTests value)
        {
            return Validate(value).Count == 0;
        }

        public void EnsureValid(BroadcastServiceTests value)
        {
            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join("\n", errors));
            }
        }
    }
}
