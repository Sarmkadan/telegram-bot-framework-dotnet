using TelegramBotFramework.Keyboard;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBotFramework.Tests.Keyboard
{
    public static class ReplyKeyboardBuilderTestsValidation
    {
        public static IReadOnlyList<string> Validate(this ReplyKeyboardBuilderTests value)
        {
            var errors = new List<string>();
            // TODO: implement validation logic here
            return errors;
        }

        public static bool IsValid(this ReplyKeyboardBuilderTests value)
        {
            var errors = Validate(value);
            return errors.Count == 0;
        }

        public static void EnsureValid(this ReplyKeyboardBuilderTests value)
        {
            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException("validation failed: " + string.Join("; ", errors));
            }
        }
    }
}