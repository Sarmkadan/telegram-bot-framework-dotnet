#nullable enable
namespace TelegramBotFramework.ConversationFlow;

internal static class ConversationFlowExtensionsConstants
{
    public const string StateDirectoryCannotBeEmpty = "State directory cannot be empty.";
    public const string FlowIdMustNotBeEmpty = "FlowId must not be empty.";
    public const string NameMustNotBeEmpty = "Name must not be empty.";
    public const string TimeoutMustBePositive = "Timeout must be positive.";
    public const string StepIdMustNotBeEmpty = "Step.StepId must not be empty.";
    public const string DuplicateStepIdFormat = "A step with StepId '{0}' has already been added to this flow.";
    public const string FlowMustHaveAtLeastOneStepFormat = "Flow '{0}' must have at least one step before building.";
    public const string TransitionTargetDoesNotExistFormat = "Step '{0}' has a transition to '{1}' which does not exist in flow '{2}'.";
    public const string DefaultNextStepIdDoesNotExistFormat = "Step '{0}' references DefaultNextStepId '{1}' which does not exist in flow '{2}'.";
    public static readonly StringComparer StepIdComparer = StringComparer.OrdinalIgnoreCase;
}